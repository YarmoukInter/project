(() => {
    'use strict';
    const config = document.querySelector('[data-completion-notifications]');
    if (!config) return;
    const endpoint = config.dataset.endpoint;
    const inbox = document.querySelector('[data-completion-inbox]');
    const feedback = document.querySelector('[data-completion-feedback]');
    const escape = value => String(value == null ? '' : value).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
    const dialog = document.querySelector('#completion-report-notice');
    let reports = [], pending = false;
    const shown = new Set();
    const link = id => endpoint + '?pdf=' + encodeURIComponent(id);
    async function api(action, body) {
        const response = await fetch(endpoint + '?action=' + action, {
            method: body ? 'POST' : 'GET', credentials: 'same-origin', cache: 'no-store',
            headers: body ? {'Content-Type':'application/json','X-Completion-CSRF':config.dataset.csrf} : {},
            body: body ? JSON.stringify(body) : undefined
        });
        if (response.redirected) throw new Error('Sign in again to view your reports.');
        const data = await response.json();
        if (!response.ok || !data.success) throw new Error(data.message || 'Reports could not be loaded.');
        return data;
    }
    function render() {
        const count = reports.filter(report => !report.read).length;
        document.querySelectorAll('[data-completion-count]').forEach(node => { node.textContent = count ? ` (${count} new)` : ''; });
        if (inbox) {
            inbox.innerHTML = reports.map(report => `<article class="completion-report-card"><p class="completion-eyebrow">${report.read ? 'FINAL REPORT' : 'NEW FINAL REPORT'}</p><h2>Internship completion approved</h2><p>${escape(report.companyName)}</p><dl><div><dt>Final grade</dt><dd>${escape(report.finalScore)} / 100</dd></div><div><dt>Approved hours</dt><dd>${escape(report.approvedHours)} / ${escape(report.requiredHours)}</dd></div><div><dt>Received</dt><dd>${escape(new Date(Number(report.sentAtUnixMs)).toLocaleDateString('en-GB'))}</dd></div></dl><a class="completion-primary" target="_blank" rel="noopener" href="${escape(link(report.id))}" data-report-open="${escape(report.id)}">Open / print PDF</a></article>`).join('');
            feedback.textContent = reports.length ? `${reports.length} report${reports.length === 1 ? '' : 's'} available. Use the PDF viewer to print or download a copy.` : 'No final report yet. Your report will appear here when the university coordinator sends it.';
        }
        const unread = reports.find(report => !report.read && !shown.has(report.id));
        if (unread && !dialog.open && !document.querySelector('dialog[open]')) {
            shown.add(unread.id);
            dialog.dataset.reportId = unread.id;
            dialog.querySelector('[data-report-notice-grade]').textContent = `${unread.finalScore} / 100`;
            const open = dialog.querySelector('[data-report-open]');
            open.href = link(unread.id); open.dataset.reportOpen = unread.id;
            dialog.showModal();
        }
    }
    async function refresh() {
        if (pending || document.hidden) return;
        pending = true;
        try { reports = (await api('list')).reports || []; render(); }
        catch (error) { if (feedback) feedback.textContent = error.message; }
        finally { pending = false; }
    }
    async function markRead(id) {
        try { await api('read', {id}); reports.forEach(report => { if (report.id === id) report.read = true; }); render(); }
        catch (error) { if (feedback) feedback.textContent = error.message; }
    }
    document.addEventListener('click', event => {
        const open = event.target.closest('[data-report-open]');
        if (open) { if (dialog.open) dialog.close(); markRead(open.dataset.reportOpen); }
        if (event.target.closest('[data-report-dismiss]')) { const id = dialog.dataset.reportId; dialog.close(); markRead(id); }
    });
    dialog.addEventListener('cancel', () => { const id = dialog.dataset.reportId; if (id) markRead(id); });
    document.addEventListener('visibilitychange', () => { if (!document.hidden) refresh(); });
    refresh();
    setInterval(refresh, 60000);
})();
