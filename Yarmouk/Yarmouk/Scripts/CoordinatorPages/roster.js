// ASP.NET handles reads and writes. The browser receives display data only.
window.CoordinatorBackend = {
    state() { return JSON.parse(document.getElementById('backend-data').textContent); },
    submit(action, details) {
        const form = document.querySelector('form');
        if (form.dataset.saving) return;
        form.dataset.saving = 'true';
        form.querySelector('[name="adminAction"]').value = action;
        form.querySelector('[name="adminPayload"]').value = JSON.stringify(details);
        form.noValidate = true;
        form.method = 'post';
        HTMLFormElement.prototype.submit.call(form);
    }
};
(() => {
    'use strict';
    const escape = value => String(value ?? '').replace(/[&<>"']/g, ch => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[ch]));

    window.YarmoukRoster = {
        mount(config) {
            const root = document.querySelector(`[data-roster-page="${config.page}"]`);
            if (!root || root.dataset.ready) return;
            root.dataset.ready = 'true';
            document.body.classList.add('co-roster-page');
            document.title = `${config.title} — Yarmouk University`;
            const header = document.querySelector('.yu-dashboard-main > header');
            if (header) header.innerHTML = '<a class="yr-header-brand" href="Dashboard.aspx"><strong>Yarmouk University</strong><span>Internship administration</span></a><a class="yr-header-account" href="Account.aspx" aria-label="My coordinator account"><span aria-hidden="true">YU</span><span>University coordinator</span></a>';

            const state = window.CoordinatorBackend.state();
            let records = state.data?.records || [];
            const clientFields = config.fields.filter(field => field.clientOnly);
            const clientStorageKey = `coordinatorRoster:${config.page}:display`;
            let clientValues = {};
            if (clientFields.length) {
                try { clientValues = JSON.parse(sessionStorage.getItem(clientStorageKey) || '{}') || {}; } catch (_) { }
                records.forEach(record => {
                    const saved = clientValues[record.id] || clientValues[record.email] || {};
                    clientFields.forEach(field => { record[field.key] = saved[field.key] || ''; });
                });
            }
            const submit = window.CoordinatorBackend.submit;
            if (state.data?.supervisors) {
                const options = [{value:'',label:'Unassigned'}, ...state.data.supervisors];
                config.columns.filter(c => c.type === 'supervisor').forEach(c => c.options = options);
                config.fields.filter(f => f.key === 'supervisor').forEach(f => f.options = options);
            }
            const department = config.fields.find(f => f.key === 'department');
            if (department && state.data?.departments?.length) department.options = state.data.departments;
            let editingId = null, pendingDelete = null, opener = null;
            const tbody = root.querySelector('[data-roster-body]');
            const feedback = root.querySelector('[data-roster-feedback]');
            const dialog = root.querySelector('[data-roster-dialog]');
            const fields = root.querySelector('[data-editor-fields]');
            const error = root.querySelector('[data-editor-error]');
            const deleteDialog = root.querySelector('[data-delete-dialog]');
            const accessDialog = root.querySelector('[data-access-dialog]');
            const permanentDialog = root.querySelector('[data-permanent-dialog]');
            let accountRecord = null;

            function cell(column, record) {
                const value = record[column.key];
                if (column.type === 'identity') return `<strong class="yr-person-name">${escape(value)}</strong><span class="yr-person-email">${escape(record[column.detailKey || 'email'] || '—')}${record.isActive ? '' : ' · Inactive'}</span>`;
                if (column.type === 'gpa') return `<span class="yr-number">${(value == null || value === '' ? '—' : Number(value).toFixed(2))}</span>`;
                if (column.type === 'count') return `<span class="yr-number">${escape(value)}</span><span class="yr-cell-note">students</span>`;
                if (column.type === 'status') {
                    const tone = ['Active','Approved'].includes(value) ? 'positive' : value === 'Inactive' ? 'neutral' : 'pending';
                    return `<span class="yr-status yr-status-${tone}"><span aria-hidden="true"></span>${escape(value)}</span>`;
                }
                if (column.type === 'supervisor') return `<select class="yr-assignment" data-assignment="${escape(record.id)}" aria-label="Assigned supervisor for ${escape(record.name)}">${column.options.map(option => `<option value="${escape(option.value)}"${option.value === value ? ' selected' : ''}${option.disabled ? ' disabled' : ''}>${escape(option.label)}</option>`).join('')}</select>`;
                return escape(value || '—');
            }

            function render() {
                tbody.innerHTML = records.length ? records.map(record => `<tr>${config.columns.map((column, index) => `<${index === 0 ? 'th scope="row"' : 'td'} class="yr-column-${column.key}">${cell(column, record)}</${index === 0 ? 'th' : 'td'}>`).join('')}<td class="yr-actions-cell"><div class="yr-row-actions"><button class="yr-button yr-icon-button" type="button" data-edit="${escape(record.id)}" aria-label="Edit details for ${escape(record.name)}" title="Edit details" data-tooltip="Edit details"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="m14 5 5 5M4 20l4.2-.8L19 8.4a2.1 2.1 0 0 0-3-3L5.2 16.2 4 20Z"/></svg></button>${config.accountAccess ? `<button class="yr-button yr-icon-button" type="button" data-access="${escape(record.id)}" aria-label="Sign-in settings for ${escape(record.name)}" title="Sign-in settings" data-tooltip="Sign-in settings"><svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="3.5"/><path d="M5 20v-1.5a7 7 0 0 1 14 0V20M18 5v6m-3-3h6"/></svg></button>` : ''}<button class="yr-button yr-icon-button yr-danger" type="button" data-delete="${escape(record.id)}" aria-label="${record.isActive ? 'Deactivate' : 'Reactivate'} ${escape(record.name)}" title="${record.isActive ? 'Deactivate account' : 'Reactivate account'}" data-tooltip="${record.isActive ? 'Deactivate' : 'Reactivate'}"><svg viewBox="0 0 24 24" aria-hidden="true">${record.isActive ? '<circle cx="12" cy="12" r="8"/><path d="M6.3 6.3 17.7 17.7"/>' : '<path d="M20 11a8 8 0 1 1-2.3-5.7M20 4v6h-6"/>'}</svg></button>${config.permanentDelete ? `<button class="yr-button yr-icon-button yr-danger" type="button" data-permanent="${escape(record.id)}" aria-label="Delete ${escape(record.name)} permanently" title="Delete student permanently" data-tooltip="Delete permanently"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 7h16M10 11v6m4-6v6M6 7l1 14h10l1-14M9 7V4h6v3"/></svg></button>` : ''}</div></td></tr>`).join('') : `<tr><td class="yr-empty" colspan="${config.columns.length + 1}"><strong>No ${config.plural} yet.</strong><span>Register a ${config.singular} to begin.</span></td></tr>`;
            }

            function openEditor(record = null) {
                opener = document.activeElement;
                editingId = record ? record.id : null;
                error.textContent = '';
                root.querySelector('#editor-title').textContent = `${record ? 'Edit' : 'Register'} ${config.singular}`;
                root.querySelector('[data-editor-save]').textContent = record ? 'Save changes' : `Register ${config.singular}`;
                fields.innerHTML = config.fields.filter(field => !field.createOnly || !record).map(field => {
                    const id = `editor-${field.key}`;
                    let value = (record ? record[field.key] : field.default) ?? '';
                    if (field.type === 'date') value = String(value).replace(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/, (_, day, month, year) => `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`);
                    const emailReadOnly = record && field.key === 'email' && config.page !== 'ManageStudents';
                    const readonly = emailReadOnly ? ' readonly title="Update sign-in emails in Firebase Authentication"' : '';
                    if (field.options && value && !field.options.some(o => o.value === value)) field.options = [...field.options, {value,label:value}];
                    let input;
                    if (field.options) input = `<select id="${id}" data-field="${field.key}"${field.required ? ' required' : ''}>${field.options.map(option => `<option value="${escape(option.value)}"${option.value === value ? ' selected' : ''}${option.disabled ? ' disabled' : ''}>${escape(option.label)}</option>`).join('')}</select>`;
                    else input = `<input id="${id}" data-field="${field.key}" type="${field.type || 'text'}" value="${escape(value)}"${field.required ? ' required' : ''}${field.description ? ` aria-describedby="${id}-description"` : ''}${field.type === 'number' ? ` min="${field.min ?? 0}"${field.max != null ? ` max="${field.max}"` : ''} step="${field.step || 1}"` : field.type === 'password' ? '' : ' maxlength="254"'} ${readonly} autocomplete="${field.type === 'password' ? 'new-password' : 'off'}"${field.type === 'password' ? ' minlength="8" maxlength="128"' : ''} />`;
                    if (!field.options) input = input.replace(' />', `${field.inputMode ? ` inputmode="${escape(field.inputMode)}"` : ''}${field.pattern ? ` pattern="${escape(field.pattern)}"` : ''}${field.type === 'date' && field.max ? ` max="${escape(field.max)}"` : ''} />`);
                    return `<div class="yr-editor-field${field.wide ? ' yr-field-wide' : ''}"><label for="${id}">${escape(field.label)}${field.required ? ' <span aria-hidden="true">*</span>' : ''}</label>${input}${field.description ? `<small class="yr-field-note" id="${id}-description">${escape(field.description)}</small>` : ''}</div>`;
                }).join('');
                const departmentInput = fields.querySelector('[data-field="department"]');
                const collegeInput = fields.querySelector('[data-field="college"]');
                if (departmentInput && collegeInput && state.data?.departments?.length) {
                    const updateCollege = () => {
                        const major = state.data.departments.find(d => d.value === departmentInput.value);
                        if (major?.faculty) collegeInput.value = major.faculty;
                    };
                    if (!record) updateCollege();
                    departmentInput.addEventListener('change', updateCollege);
                }
                dialog.showModal();
                fields.querySelector('[data-field]')?.focus();
            }

            function saveEditor() {
                const inputs = [...fields.querySelectorAll('[data-field]')];
                inputs.forEach(input => input.setCustomValidity(''));
                for (const input of inputs) {
                    if (input.required && !input.value.trim()) input.setCustomValidity('Please complete this field.');
                    if (!input.reportValidity()) return;
                }
                const next = {id:editingId};
                inputs.forEach(input => { next[input.dataset.field] = input.value.trim(); });
                submit('save', next);
            }

            root.querySelector('[data-roster-add]').addEventListener('click', () => openEditor());
            root.querySelectorAll('[data-dialog-close]').forEach(button => button.addEventListener('click', () => dialog.close()));
            root.querySelector('[data-editor-save]').addEventListener('click', saveEditor);
            fields.addEventListener('input', event => { if (event.target.setCustomValidity) event.target.setCustomValidity(''); error.textContent = ''; });
            dialog.addEventListener('keydown', event => { if (event.key === 'Enter' && event.target.tagName === 'INPUT') { event.preventDefault(); saveEditor(); } });

            tbody.addEventListener('click', event => {
                const edit = event.target.closest('[data-edit]');
                if (edit) { const record = records.find(item => item.id === edit.dataset.edit); if (record) openEditor(record); return; }
                const access = event.target.closest('[data-access]');
                if (access && accessDialog) {
                    accountRecord = records.find(item => item.id === access.dataset.access);
                    if (!accountRecord) return;
                    opener = access;
                    accessDialog.querySelector('#access-email').value = accountRecord.email || '';
                    accessDialog.querySelector('#access-password').value = '';
                    accessDialog.querySelector('[data-access-error]').textContent = '';
                    accessDialog.showModal(); accessDialog.querySelector('#access-email').focus(); return;
                }
                const permanent = event.target.closest('[data-permanent]');
                if (permanent && permanentDialog) {
                    accountRecord = records.find(item => item.id === permanent.dataset.permanent);
                    if (!accountRecord) return;
                    opener = permanent;
                    permanentDialog.querySelector('[data-permanent-description]').textContent = `Permanently delete ${accountRecord.name} (${accountRecord.email})?`;
                    permanentDialog.showModal(); return;
                }
                const remove = event.target.closest('[data-delete]');
                if (!remove) return;
                pendingDelete = records.find(item => item.id === remove.dataset.delete);
                if (!pendingDelete) return;
                opener = remove;
                root.querySelector('[data-delete-description]').textContent = pendingDelete.isActive ? `Deactivate ${pendingDelete.name}? Their records will be kept, but they will no longer be able to sign in.` : `Reactivate ${pendingDelete.name}?`;
                root.querySelector('#delete-title').textContent = pendingDelete.isActive ? 'Deactivate account?' : 'Reactivate account?';
                root.querySelector('[data-delete-confirm]').textContent = pendingDelete.isActive ? 'Deactivate' : 'Reactivate';
                deleteDialog.showModal();
                root.querySelector('[data-delete-cancel]:not(.yr-close)').focus();
            });
            tbody.addEventListener('change', event => {
                const select = event.target.closest('[data-assignment]');
                if (!select) return;
                const record = records.find(item => item.id === select.dataset.assignment);
                if (!record) return;
                submit('assign', {id:record.id,supervisor:select.value});
            });
            root.querySelectorAll('[data-delete-cancel]').forEach(button => button.addEventListener('click', () => deleteDialog.close()));
            root.querySelector('[data-delete-confirm]').addEventListener('click', () => {
                if (!pendingDelete) return;
                submit('active', {id:pendingDelete.id,value:!pendingDelete.isActive});
            });
            if (accessDialog) {
                root.querySelectorAll('[data-access-close]').forEach(button => button.addEventListener('click', () => accessDialog.close()));
                root.querySelector('[data-access-save]').addEventListener('click', () => {
                    if (!accountRecord) return;
                    const email = accessDialog.querySelector('#access-email');
                    const password = accessDialog.querySelector('#access-password');
                    if (!email.reportValidity() || (password.value && !password.reportValidity())) return;
                    if (!password.value && email.value.trim() === accountRecord.email) {
                        accessDialog.querySelector('[data-access-error]').textContent = 'Change the email or enter a temporary password.'; return;
                    }
                    submit('credentials', {id:accountRecord.id,email:email.value.trim(),password:password.value});
                });
            }
            if (permanentDialog) {
                root.querySelectorAll('[data-permanent-close]').forEach(button => button.addEventListener('click', () => permanentDialog.close()));
                root.querySelector('[data-permanent-confirm]').addEventListener('click', () => {
                    if (accountRecord) submit('deleteStudent', {id:accountRecord.id});
                });
            }
            [dialog, deleteDialog, accessDialog, permanentDialog].filter(Boolean).forEach(modal => modal.addEventListener('close', () => {
                if (opener && document.contains(opener)) opener.focus();
                else root.querySelector('[data-roster-add]').focus();
            }));
            feedback.textContent = state.error || state.notice || '';
            if (!state.data) {
                root.querySelector('[data-roster-add]').disabled = true;
                tbody.innerHTML = `<tr><td colspan="${config.columns.length + 1}">Unable to load records. Check the error below and reload.</td></tr>`;
            } else render();
            if (state.error) {
                feedback.setAttribute('role', 'alert');
                feedback.tabIndex = -1;
                feedback.focus();
            }
        }
    };
})();
