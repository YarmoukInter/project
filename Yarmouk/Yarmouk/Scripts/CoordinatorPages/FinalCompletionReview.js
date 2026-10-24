// Final completion review. Data is supplied by the page's authenticated Firebase read.
(() => {
    'use strict';

    const escapeHtml = value => String(value == null ? '' : value).replace(/[&<>"']/g, character => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[character]));

    const icons = {
        search: '<circle cx="11" cy="11" r="7"></circle><path d="m16 16 4 4"></path>',
        check: '<path d="m5 12 4 4L19 6"></path><circle cx="12" cy="12" r="10"></circle>',
        info: '<circle cx="12" cy="12" r="10"></circle><path d="M12 16v-4m0-4h.01"></path>',
        file: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8zM14 2v6h6M8 13h8m-8 4h8"></path>',
        close: '<path d="m18 6-12 12M6 6l12 12"></path>'
    };
    const icon = name => `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${icons[name]}</svg>`;

    function mount() {
        const page = document.querySelector('[data-final-review-page]');
        if (!page || page.dataset.ready) return;
        page.dataset.ready = 'true';
        document.body.classList.add('co-final-body');
        document.title = 'Final Completion Review — UIMS';

        const records = Array.isArray(window.uimsFinalReviewData) ? window.uimsFinalReviewData : [];
        const message = window.uimsFinalReviewMessage || '';
        const script = Array.from(document.scripts).find(item => /\/CoordinatorPages\/FinalCompletionReview\.js(?:\?|$)/i.test(item.src));
        const root = script ? new URL('../../CoordinatorPages/', script.src) : new URL('./', location.href);
        const route = name => new URL(name, root).href;
        const labels = {
            ready: ['Ready for coordinator review', 'ready'],
            missing: ['Evidence incomplete', 'missing'],
            approved: ['Completion approved', 'approved']
        };

        const header = document.querySelector('.yu-dashboard-main>header');
        const footer = document.querySelector('.yu-dashboard-main>footer');
        if (header) header.innerHTML = `<a class="co-brand" href="${route('Dashboard.aspx')}"><img src="${new URL('../../assets/yarmouk-logo-on-light.svg', script ? script.src : location.href).href}" alt="Yarmouk University" width="112" height="43"><span class="co-brand-copy"><strong>Yarmouk University</strong><span>Internship administration</span></span></a><div class="co-header-actions"><a class="co-notifications" href="${route('ContactMessages.aspx')}" aria-label="Contact messages">${icon('info')}</a><a class="co-account" href="${route('Account.aspx')}"><span class="co-avatar">YU</span><span>University coordinator</span></a></div>`;
        if (footer) footer.innerHTML = '<span>Yarmouk University</span><span>Faculty of Information Technology and Computer Science</span>';

        const count = status => records.filter(record => record.status === status).length;
        const providers = Array.from(new Set(records.map(record => record.company).filter(Boolean))).sort((a, b) => a.localeCompare(b));
        page.innerHTML = `
            <div class="fr-heading"><div><p class="fr-eyebrow">${icon('check')} COMPLETION WORKFLOW</p><h1 id="fr-title">Final completion review</h1><p class="fr-intro">Review company and academic final evaluations alongside approved training hours before making a completion decision.</p></div><div class="fr-heading-actions"><a class="fr-button" href="${route('MonitorPlacements.aspx')}">← Back to placement monitoring</a></div></div>
            <aside class="fr-preview" role="note">${icon('info')} <strong>Coordinator decision required.</strong> Review the source evaluations and university requirements before approving completion. The final grade uses 40% company evaluation and 60% academic evaluation.</aside>
            ${message ? `<div class="fr-alert" role="status">${escapeHtml(message)}</div>` : ''}
            <section class="fr-stats" aria-label="Completion review summary"><article class="fr-stat"><span class="fr-stat-label">Evaluation records</span><strong>${records.length}</strong><span class="fr-hint">Loaded from the system</span></article><article class="fr-stat"><span class="fr-stat-label">Ready for review</span><strong>${count('ready')}</strong><span class="fr-hint">Required evidence currently present</span></article><article class="fr-stat"><span class="fr-stat-label">Evidence incomplete</span><strong>${count('missing')}</strong><span class="fr-hint">At least one prerequisite is outstanding</span></article><article class="fr-stat"><span class="fr-stat-label">Approved</span><strong>${count('approved')}</strong><span class="fr-hint">Recorded coordinator decisions</span></article></section>
            <section class="fr-panel" aria-labelledby="fr-list-title"><div class="fr-panel-head"><h2 id="fr-list-title">Completion evaluations</h2><p>Open a record to review both evaluations, approved hours, and prerequisite status.</p></div>
                <div class="fr-filters"><label class="fr-search">${icon('search')}<input id="fr-search" type="search" placeholder="Search student, ID or training provider" aria-label="Search evaluations" autocomplete="off"></label><select id="fr-provider" class="fr-select" aria-label="Filter by training provider"><option value="">All training providers</option>${providers.map(provider => `<option value="${escapeHtml(provider)}">${escapeHtml(provider)}</option>`).join('')}</select><span class="fr-count" id="fr-count" aria-live="polite"></span></div>
                <div class="fr-tabs" role="group" aria-label="Filter by review status"><button type="button" class="fr-tab" data-status="all" aria-pressed="true">All · ${records.length}</button><button type="button" class="fr-tab" data-status="ready" aria-pressed="false">Ready · ${count('ready')}</button><button type="button" class="fr-tab" data-status="missing" aria-pressed="false">Incomplete · ${count('missing')}</button><button type="button" class="fr-tab" data-status="approved" aria-pressed="false">Approved · ${count('approved')}</button></div>
                <div class="fr-table-wrap"><table><caption class="visually-hidden">Final completion evaluations loaded from the system</caption><thead><tr><th>Student</th><th>Training provider</th><th>Evaluations</th><th>Approved hours</th><th>Status</th><th><span class="visually-hidden">Review</span></th></tr></thead><tbody id="fr-rows"></tbody></table><div class="fr-empty" id="fr-empty" hidden></div></div><div class="fr-footer"><span id="fr-footer-count"></span><span>Source: current Firebase records</span></div></section>
            <dialog id="fr-dialog" aria-labelledby="fr-dialog-title"><div class="fr-dialog-head"><div><h2 id="fr-dialog-title">Evaluation details</h2><p id="fr-dialog-subtitle"></p></div><button type="button" class="fr-close" id="fr-close" aria-label="Close details">${icon('close')}</button></div><div class="fr-dialog-body" id="fr-dialog-body"></div></dialog>`;

        const search = page.querySelector('#fr-search');
        const provider = page.querySelector('#fr-provider');
        const body = page.querySelector('#fr-rows');
        const table = page.querySelector('table');
        const empty = page.querySelector('#fr-empty');
        const dialog = page.querySelector('#fr-dialog');
        let filter = 'all';

        function render() {
            const query = search.value.trim().toLocaleLowerCase();
            const visible = records.filter(record => (filter === 'all' || record.status === filter)
                && (!provider.value || record.company === provider.value)
                && `${record.name} ${record.studentId} ${record.company} ${record.program}`.toLocaleLowerCase().includes(query));
            body.innerHTML = visible.map(record => {
                const state = labels[record.status] || labels.missing;
                return `<tr><td><div class="fr-person"><span class="fr-avatar">${escapeHtml(record.initials)}</span><span class="fr-person-copy"><strong>${escapeHtml(record.name)}</strong><span>${escapeHtml(record.studentId || 'Student ID unavailable')}${record.program ? ` · ${escapeHtml(record.program)}` : ''}</span></span></div></td><td><span class="fr-provider"><strong>${escapeHtml(record.company)}</strong><span>${escapeHtml(record.opportunity || '')}</span></span></td><td><span class="fr-docs">Company ${record.companySubmitted ? 'received' : 'pending'} · Academic ${record.academicSubmitted ? 'received' : 'pending'}</span></td><td><span class="fr-date">${record.hoursConfigured ? `${Number(record.approvedHours)} / ${Number(record.requiredHours)} hours` : `${Number(record.approvedHours)} hours · requirement not configured`}</span></td><td><span class="fr-badge fr-${state[1]}">${state[0]}</span></td><td><button type="button" class="fr-open" data-open="${escapeHtml(record.id)}">Review record</button></td></tr>`;
            }).join('');
            table.hidden = visible.length === 0;
            empty.hidden = visible.length > 0;
            empty.textContent = records.length ? 'No matching evaluations. Try changing the search or filters.' : (message || 'No final evaluation records are available yet.');
            page.querySelector('#fr-count').textContent = `${visible.length} of ${records.length} records`;
            page.querySelector('#fr-footer-count').textContent = `Matches ${visible.length} of ${records.length} records`;
        }

        function showDetails(record) {
            const state = labels[record.status] || labels.missing;
            page.querySelector('#fr-dialog-title').textContent = record.name || 'Evaluation details';
            page.querySelector('#fr-dialog-subtitle').textContent = `${record.id} · ${record.studentId || 'Student ID unavailable'}`;
            const checks = (record.checks || []).map(item => `<li class="${item.complete ? '' : 'pending'}"><strong>${item.complete ? '✓' : '!' } ${escapeHtml(item.label)}</strong><span> — ${escapeHtml(item.detail)}</span></li>`).join('');
            page.querySelector('#fr-dialog-body').innerHTML = `<div class="fr-details"><div class="fr-detail"><span>Training provider</span><strong>${escapeHtml(record.company)}</strong></div><div class="fr-detail"><span>Opportunity</span><strong>${escapeHtml(record.opportunity || 'Not recorded')}</strong></div><div class="fr-detail"><span>Review status</span><strong><span class="fr-badge fr-${state[1]}">${state[0]}</span></strong></div><div class="fr-detail"><span>Coordinator decision</span><strong>${record.coordinatorApproved ? 'Approved' : 'Not approved'}</strong></div></div><h3>Final evaluations</h3><div class="fr-review-grid"><div class="fr-detail"><span>Company supervisor evaluation</span><strong>${escapeHtml(record.companyScore)}</strong></div><div class="fr-detail"><span>Academic supervisor evaluation</span><strong>${escapeHtml(record.academicScore)}</strong></div></div><h3>Completion requirements</h3><ul class="fr-checklist">${checks}</ul>${!record.coordinatorApproved ? `<section class="fr-hours-entry"><h3>Verify training hours</h3><p>Record the total hours verified from attendance evidence. This does not add hours to the existing total.</p><label for="fr-hours">Total verified hours</label><input id="fr-hours" type="number" min="0" max="10000" step="1" value="${Number(record.approvedHours)}"><label for="fr-hours-note">Verification note</label><textarea id="fr-hours-note" maxlength="1000" rows="3" placeholder="Attendance records or evidence reviewed"></textarea><button type="button" class="fr-button" data-action="hours">Approve verified hours</button></section>` : `<p class="fr-note">Final grade: <strong>${escapeHtml(record.finalScore)} / 100</strong></p>`}<p class="fr-note">${record.reportSent ? 'The PDF has been delivered to the student’s Completion reports inbox.' : 'Approve completion, then send the PDF. The student will receive an in-app notice and a permanent download link.'}</p><p id="fr-action-feedback" role="status" aria-live="polite"></p><div class="fr-dialog-actions"><button type="button" class="fr-button" id="fr-close-secondary">Close</button>${!record.coordinatorApproved ? `<button type="button" class="fr-button" data-action="approve" ${record.status !== 'ready' ? 'disabled' : ''}>Approve completion</button>` : `<a class="fr-button" href="${page.dataset.endpoint}?pdf=${encodeURIComponent(record.id)}" target="_blank" rel="noopener">Open / print PDF</a><button type="button" class="fr-button" data-action="send" ${record.reportSent ? 'disabled' : ''}>${record.reportSent ? 'PDF delivered' : 'Send PDF to student'}</button>`}</div>`;
            if (!dialog.open) dialog.showModal();
            page.querySelectorAll('[data-action]').forEach(button => button.addEventListener('click', async () => {
                const feedback = page.querySelector('#fr-action-feedback');
                const action = button.dataset.action;
                const payload = { id: record.id };
                if (action === 'hours') {
                    payload.hours = page.querySelector('#fr-hours').value;
                    payload.note = page.querySelector('#fr-hours-note').value.trim();
                    if (!payload.note) { feedback.textContent = 'Add a verification note before approving hours.'; page.querySelector('#fr-hours-note').focus(); return; }
                }
                const controls = Array.from(page.querySelectorAll('[data-action]'));
                const states = controls.map(control => control.disabled);
                controls.forEach(control => control.disabled = true);
                feedback.textContent = action === 'send' ? 'Generating and delivering the PDF…' : 'Saving approval…';
                try {
                    const response = await fetch(page.dataset.endpoint + '?action=' + action, {
                        method: 'POST', credentials: 'same-origin',
                        headers: { 'Content-Type': 'application/json', 'X-Completion-CSRF': page.dataset.csrf },
                        body: JSON.stringify(payload)
                    });
                    if (response.redirected) throw new Error('Your session expired. Sign in again.');
                    const data = await response.json();
                    if (!response.ok || !data.success) throw new Error(data.message || 'The action could not be saved.');
                    sessionStorage.setItem('completion-review-open', record.id);
                    sessionStorage.setItem('completion-review-feedback', action === 'send' ? 'PDF delivered. The student can now open it from Completion reports.' : action === 'hours' ? 'Verified hours saved.' : 'Completion approved. You can now print or send the PDF.');
                    location.reload();
                } catch (error) {
                    feedback.textContent = error.message;
                    controls.forEach((control, index) => control.disabled = states[index]);
                }
            }));
            page.querySelector('#fr-close-secondary').onclick = () => dialog.close();
        }

        page.querySelectorAll('[data-status]').forEach(button => button.addEventListener('click', () => {
            filter = button.dataset.status;
            page.querySelectorAll('[data-status]').forEach(tab => tab.setAttribute('aria-pressed', String(tab === button)));
            render();
        }));
        search.addEventListener('input', render);
        provider.addEventListener('change', render);
        body.addEventListener('click', event => {
            const button = event.target.closest('[data-open]');
            if (!button) return;
            const record = records.find(item => item.id === button.dataset.open);
            if (record) showDetails(record);
        });
        page.querySelector('#fr-close').addEventListener('click', () => dialog.close());
        dialog.addEventListener('click', event => { if (event.target === dialog) dialog.close(); });
        render();
        const reopenId = sessionStorage.getItem('completion-review-open');
        const savedMessage = sessionStorage.getItem('completion-review-feedback');
        sessionStorage.removeItem('completion-review-open');
        sessionStorage.removeItem('completion-review-feedback');
        const reopened = records.find(record => record.id === reopenId);
        if (reopened) { showDetails(reopened); page.querySelector('#fr-action-feedback').textContent = savedMessage || ''; }
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', mount, { once: true });
    else mount();
})();
