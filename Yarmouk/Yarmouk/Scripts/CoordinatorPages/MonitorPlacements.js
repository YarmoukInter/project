// Coordinator placement monitoring backed by Firebase data.
(() => {
    'use strict';
    const rows = Array.isArray(window.uimsMonitorData) ? window.uimsMonitorData : [];
    const loadMessage = typeof window.uimsMonitorMessage === 'string' ? window.uimsMonitorMessage : '';
    const statusInfo = {
        active: { label: 'In progress', className: 'active' },
        attention: { label: 'Needs attention', className: 'attention' },
        complete: { label: 'Completed', className: 'complete' }
    };
    const paths = {
        search: '<circle cx="11" cy="11" r="7"></circle><path d="m16 16 4 4"></path>',
        users: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2m20 0v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"></path><circle cx="9" cy="7" r="4"></circle>',
        chart: '<path d="M3 3v18h18M7 14l5-5 4 3 5-7M16 5h5v5"></path>',
        alert: '<path d="m10.3 3.9-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.7-3.1l-8-14a2 2 0 0 0-3.4 0Z"></path><path d="M12 9v4m0 4h.01"></path>',
        check: '<path d="m5 12 4 4L19 6"></path><circle cx="12" cy="12" r="10"></circle>',
        briefcase: '<rect x="3" y="7" width="18" height="14" rx="2"></rect><path d="M8 7V4h8v3M3 12h18m-11 0v2h4v-2"></path>',
        more: '<circle cx="5" cy="12" r="1"></circle><circle cx="12" cy="12" r="1"></circle><circle cx="19" cy="12" r="1"></circle>',
        info: '<circle cx="12" cy="12" r="10"></circle><path d="M12 16v-4m0-4h.01"></path>'
    };
    const icon = name => `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${paths[name]}</svg>`;

    const escapeHtml = value => String(value == null ? '' : value).replace(/[&<>"']/g, character => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[character]));
    function mount() {
        const page = document.querySelector('[data-monitor-page]');
        if (!page || page.dataset.ready) return;
        page.dataset.ready = 'true';
        document.body.classList.add('co-monitor-body');
        document.title = 'Monitor Placements & Progress — UIMS';
        const script = Array.from(document.scripts).find(item => /\/CoordinatorPages\/MonitorPlacements\.js(?:\?|$)/i.test(item.src));
        const root = script ? new URL('../../CoordinatorPages/', script.src) : new URL('./', location.href);
        const route = name => new URL(name, root).href;
        const footer = document.querySelector('.yu-dashboard-main > footer, form > footer');
        if (footer) footer.innerHTML = '<span>Yarmouk University</span><span>Faculty of Information Technology and Computer Science</span>';
        const totals = { active: rows.filter(row => row.status === 'active').length, attention: rows.filter(row => row.status === 'attention').length, complete: rows.filter(row => row.status === 'complete').length };
        page.innerHTML = `
            <div class="mp-heading"><div><p class="mp-eyebrow">${icon('chart')}PLACEMENT OVERSIGHT</p><h1 id="mp-title">Monitor placements &amp; progress</h1><p class="mp-intro">Follow each student's internship journey, spot placements that may need support, and keep progress visible in one place.</p></div><div class="mp-heading-actions"><a class="mp-button mp-button-primary" href="${route('FinalCompletionReview.aspx')}">${icon('check')}<span>Final completion reviews</span></a></div></div>
            <aside class="mp-preview-note" id="mp-data-note" role="status">${icon('info')}<span id="mp-data-message"></span></aside>
            <section class="mp-stats" aria-label="Placement summary">
                <article class="mp-stat"><div class="mp-stat-top"><span class="mp-stat-label">Active placements</span><span class="mp-stat-icon">${icon('briefcase')}</span></div><strong class="mp-stat-value">${rows.length - totals.complete}</strong><span class="mp-stat-hint">Students currently in training</span></article>
                <article class="mp-stat"><div class="mp-stat-top"><span class="mp-stat-label">On track</span><span class="mp-stat-icon">${icon('chart')}</span></div><strong class="mp-stat-value">${totals.active}</strong><span class="mp-stat-hint">Progress is moving as expected</span></article>
                <article class="mp-stat"><div class="mp-stat-top"><span class="mp-stat-label">Needs attention</span><span class="mp-stat-icon">${icon('alert')}</span></div><strong class="mp-stat-value">${totals.attention}</strong><span class="mp-stat-hint">Follow up with the placement team</span></article>
                <article class="mp-stat"><div class="mp-stat-top"><span class="mp-stat-label">Completed</span><span class="mp-stat-icon">${icon('check')}</span></div><strong class="mp-stat-value">${totals.complete}</strong><span class="mp-stat-hint">Ready for final completion review</span></article>
            </section>
            <section class="mp-panel" aria-labelledby="mp-list-title">
                <div class="mp-panel-head"><div><h2 id="mp-list-title">Student placements</h2><p>Search, filter, and review training progress at a glance.</p></div></div>
                <div class="mp-filters"><label class="mp-search">${icon('search')}<input type="search" id="mp-search" placeholder="Search student, ID, company or supervisor" autocomplete="off" aria-label="Search placements"></label><select class="mp-select" id="mp-company" aria-label="Filter by training provider"><option value="">All training providers</option>${Array.from(new Set(rows.map(row => row.company))).sort().map(company => `<option value="${escapeHtml(company)}">${escapeHtml(company)}</option>`).join('')}</select><span class="mp-filter-count" id="mp-result-count" aria-live="polite"></span></div>
                <div class="mp-status-tabs" role="group" aria-label="Filter by placement status"><button class="mp-status-tab" type="button" data-filter="all" aria-pressed="true">All placements <span class="mp-tab-count">${rows.length}</span></button><button class="mp-status-tab" type="button" data-filter="active" aria-pressed="false">On track <span class="mp-tab-count">${totals.active}</span></button><button class="mp-status-tab" type="button" data-filter="attention" aria-pressed="false">Needs attention <span class="mp-tab-count">${totals.attention}</span></button><button class="mp-status-tab" type="button" data-filter="complete" aria-pressed="false">Completed <span class="mp-tab-count">${totals.complete}</span></button></div>
                <div class="mp-placement-list" id="mp-rows" aria-live="polite"></div><div class="mp-empty" id="mp-empty" hidden><span class="mp-empty-icon">${icon('search')}</span><h3>No matching placements</h3><p>Try changing the search term or clearing the selected filters.</p></div>
                <div class="mp-table-footer"><span id="mp-footer-count"></span><span class="mp-updated-label">${icon('check')}Live coordinator data</span></div>
            </section>
            <dialog class="mp-details-dialog" id="mp-details" aria-labelledby="mp-details-title"><div class="mp-details-head"><div><p>PLACEMENT RECORD</p><h2 id="mp-details-title"></h2><span id="mp-details-subtitle"></span></div><button type="button" class="mp-details-close" id="mp-details-close" aria-label="Close details">×</button></div><div class="mp-details-grid" id="mp-details-grid"></div><div class="mp-details-actions"><button type="button" class="mp-button mp-button-primary" id="mp-details-done">Done</button></div></dialog>`;

        const messageNode = page.querySelector('#mp-data-message');
        messageNode.textContent = loadMessage || (rows.length + ' placement record' + (rows.length === 1 ? '' : 's') + ' loaded from Firebase.');
        if (loadMessage && rows.length === 0) page.querySelector('#mp-data-note').setAttribute('role', 'alert');

        let activeFilter = 'all';
        const search = page.querySelector('#mp-search');
        const company = page.querySelector('#mp-company');
        const body = page.querySelector('#mp-rows');
        const resultCount = page.querySelector('#mp-result-count');
        const footerCount = page.querySelector('#mp-footer-count');
        const empty = page.querySelector('#mp-empty');
        const dialog = page.querySelector('#mp-details');
        const detail = (label, value) => `<div class="mp-details-item"><span>${label}</span><strong>${escapeHtml(value || 'Not recorded')}</strong></div>`;
        function showDetails(row) {
            const status = statusInfo[row.status] || statusInfo.active;
            page.querySelector('#mp-details-title').textContent = row.name || 'Placement details';
            page.querySelector('#mp-details-subtitle').textContent = row.id || '';
            page.querySelector('#mp-details-grid').innerHTML = [
                detail('Training role', row.role), detail('Training provider', row.company),
                detail('Location', row.city), detail('Academic supervisor', row.supervisor),
                detail('Training progress', `${row.progress}%`), detail('Tasks', row.tasks),
                detail('Evaluations', row.evaluations), detail('Approved hours', row.hours),
                detail('Status', status.label), detail('Last update', row.updated)
            ].join('');
            dialog.showModal();
        }
        function render() {
            const term = search.value.trim().toLocaleLowerCase();
            const visible = rows.filter(row => {
                const matchesStatus = activeFilter === 'all' || row.status === activeFilter;
                const matchesCompany = !company.value || row.company === company.value;
                const haystack = [row.name, row.id, row.company, row.supervisor, row.role].join(' ').toLocaleLowerCase();
                return matchesStatus && matchesCompany && haystack.includes(term);
            });
            body.innerHTML = visible.map(row => {
                const status = statusInfo[row.status] || statusInfo.active;
                return `<article class="mp-placement-card"><div class="mp-placement-person"><span class="mp-avatar uims-student-photo-avatar" data-student-photo-url="${escapeHtml(row.profileImageUrl || '')}"><span>${escapeHtml(row.initials)}</span></span><span class="mp-person-copy"><strong>${escapeHtml(row.name)}</strong><span>${escapeHtml(row.id)}</span></span></div><div class="mp-placement-company"><span class="mp-card-label">Training provider</span><span class="mp-company-copy"><strong>${escapeHtml(row.company)}</strong><span>${escapeHtml(row.city || 'Location not recorded')}</span></span></div><div class="mp-placement-progress"><span class="mp-progress-label"><span>Task submissions</span><strong>${row.progress}%</strong></span><span class="mp-progress-track" role="progressbar" aria-label="${escapeHtml(row.name)} training plan progress" aria-valuemin="0" aria-valuemax="100" aria-valuenow="${row.progress}"><span style="--progress:${row.progress}%"></span></span></div><div class="mp-placement-status"><span class="mp-card-label">Placement status</span><span class="mp-badge mp-badge-${status.className}">${status.label}</span></div><button type="button" class="mp-details-button" data-detail-index="${rows.indexOf(row)}">View details</button></article>`;
            }).join('');
            if (window.UimsStudentPhotos) window.UimsStudentPhotos.enhance(body);
            page.querySelector('#mp-empty').hidden = visible.length !== 0;
            page.querySelector('#mp-empty p').textContent = rows.length
                ? 'Try changing the search term or clearing the selected filters.'
                : (loadMessage || 'No placements are currently available.');
            resultCount.textContent = `${visible.length} of ${rows.length} placements`;
            footerCount.innerHTML = `Matches <strong>${visible.length}</strong> of <strong>${rows.length}</strong> placements`;
        }
        page.querySelectorAll('[data-filter]').forEach(button => button.addEventListener('click', () => {
            activeFilter = button.dataset.filter;
            page.querySelectorAll('[data-filter]').forEach(item => item.setAttribute('aria-pressed', String(item === button)));
            render();
        }));
        search.addEventListener('input', render);
        company.addEventListener('change', render);
        body.addEventListener('click', event => {
            const button = event.target.closest('[data-detail-index]');
            if (button && rows[Number(button.dataset.detailIndex)]) showDetails(rows[Number(button.dataset.detailIndex)]);
        });
        page.querySelector('#mp-details-close').addEventListener('click', () => dialog.close());
        page.querySelector('#mp-details-done').addEventListener('click', () => dialog.close());
        dialog.addEventListener('click', event => { if (event.target === dialog) dialog.close(); });
        render();
    }
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', mount, { once: true });
    else mount();
})();
