(() => {
    'use strict';

    const icons = {
        people: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2m20 0v-2a4 4 0 0 0-3-3.87M16 3a4 4 0 0 1 0 8"/><circle cx="9" cy="7" r="4"/>',
        student: '<path d="m2 9 10-5 10 5-10 5L2 9Zm4 2v6c4 3 8 3 12 0v-6m4-2v8"/>',
        building: '<path d="M3 21h18M5 21V7l8-4v18m0-10h6v10M8 8v1m0 3v1m0 3v1m8-3v1m0 3v1"/>',
        supervisor: '<rect x="3" y="4" width="18" height="13" rx="2"/><path d="M8 21h8m-4-4v4m-5-9 3-3 3 3 4-5"/>',
        briefcase: '<rect x="3" y="7" width="18" height="14" rx="2"/><path d="M8 7V3h8v4M3 12c5 4 13 4 18 0m-9 1v3"/>',
        check: '<path d="m9 12 2 2 4-4M12 2l8 4v6c0 5-8 10-8 10S4 17 4 12V6l8-4Z"/>',
        bell: '<path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9m-11 12a2 2 0 0 0 4 0"/>',
        plus: '<path d="M12 5v14M5 12h14"/>',
        arrow: '<path d="M5 12h14m-5-5 5 5-5 5"/>',
        chart: '<path d="M4 20V10h3v10m4 0V4h3v16m4 0V7h3v13"/>',
        chevron: '<path d="m9 5 7 7-7 7"/>'
    };
    const icon = name => `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.65" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${icons[name]}</svg>`;
    const state = window.CoordinatorBackend.state();
    const counts = state.data;
    const overview = {
        statistics: [
            { label: 'Active users', value: counts?.totalUsers ?? "—", description: 'University account directory', icon: 'people', route: 'ManageStudents.aspx' },
            { label: 'Active students', value: counts?.students ?? "—", description: 'Registered student trainees', icon: 'student', route: 'ManageStudents.aspx' },
            { label: 'Partner companies', value: counts?.companies ?? "—", description: 'Training hosts and employers', icon: 'building', route: 'ManageCompanies.aspx' },
            { label: 'Academic supervisors', value: counts?.supervisors ?? "—", description: 'Faculty guidance and oversight', icon: 'supervisor', route: 'ManageAcademicSupervisors.aspx' },
            { label: 'Internship opportunities', value: counts?.opportunities ?? "—", description: 'Available training positions', icon: 'briefcase', route: 'MonitorPlacements.aspx' },
            { label: 'Active internships', value: counts?.activeInternships ?? "—", description: 'Students currently in training', icon: 'check', route: 'MonitorPlacements.aspx' }
        ],
        workspace: [
            { title: 'Students', description: 'Accounts & academic records', icon: 'student', route: 'ManageStudents.aspx' },
            { title: 'Academic supervisors', description: 'Faculty & student assignments', icon: 'supervisor', route: 'ManageAcademicSupervisors.aspx' },
            { title: 'Partner companies', description: 'Training providers & employers', icon: 'building', route: 'ManageCompanies.aspx' },
            { title: 'Student internships', description: 'Placements & training progress', icon: 'briefcase', route: 'MonitorPlacements.aspx' },
            { title: 'Training tasks', description: 'Review assigned trainee tasks', icon: 'check', route: 'ManageTasks.aspx' }
        ],
        reviews: [
            { title: 'Training hours to review', value: counts?.pendingHours ?? 0, route: 'MonitorPlacements.aspx' },
            { title: 'Supervisor assignments', value: counts?.pendingAssignments ?? 0, route: 'ManageStudents.aspx' },
            { title: 'Final completion reviews', value: counts?.pendingCompletion ?? 0, route: 'FinalCompletionReview.aspx' }
        ]
    };

    const total = overview.reviews.reduce((sum,item) => sum + item.value, 0);
    function mount() {
        const form = document.querySelector('form');
        const main = form && form.querySelector('main');
        const footer = form && form.querySelector('.yu-dashboard-main > footer, :scope > footer');
        if (!main || main.querySelector('.co-overview')) return;

        document.body.classList.add('co-dashboard');
        document.title = 'College overview — Yarmouk University';
        const today = new Intl.DateTimeFormat('en-GB', { weekday: 'long', day: 'numeric', month: 'long', timeZone: 'Asia/Amman' }).format(new Date());

        main.id = 'main';
        main.tabIndex = -1;
        main.innerHTML = `
            <section class="co-overview" aria-labelledby="co-title"><p role="status" data-dashboard-error></p>
                <div class="co-title-row">
                    <div class="co-title-group"><span class="co-eyebrow">Yarmouk University · Internship administration</span><h1 id="co-title">College overview<span aria-hidden="true">.</span></h1><p>A clear picture of your people, placements, and priorities.</p></div>
                    <div class="co-title-tools"><time class="co-date">${today}</time><div class="co-actions">
                        <button class="co-button co-button-secondary" type="button" data-export-dashboard>Export Excel</button>
                        <a class="co-button co-button-secondary" href="ManageAcademicSupervisors.aspx">${icon('people')}<span>Add supervisor</span></a>
                        <a class="co-button co-button-primary" href="ManageStudents.aspx">${icon('plus')}<span>Add student</span></a>
                    </div></div>
                </div>
                <div class="co-section-caption"><span>At a glance</span><span>Current account & training totals</span></div>
                <div class="co-statistics" aria-label="Internship overview">
                    <a class="co-spotlight" href="MonitorPlacements.aspx">
                        <div class="co-spotlight-top"><span class="co-spotlight-icon">${icon('briefcase')}</span><span class="co-training-tag"><span aria-hidden="true"></span>Training activity</span></div>
                        <div class="co-spotlight-content"><span class="co-spotlight-value">${counts?.activeInternships ?? '—'}</span><h2>Active internships</h2><p>Students taking their next step<br />through practical training.</p></div>
                        <span class="co-spotlight-link">View placements<span>${icon('arrow')}</span></span>
                        <svg class="co-spotlight-art" viewBox="0 0 200 300" fill="none" aria-hidden="true"><g stroke="currentColor"><path d="M12 300V120L110 35l98 85v180M30 300V128l80-69 80 69v172M48 300V137l62-53 62 53v163M67 300V175a43 43 0 0 1 86 0v125M84 300V175a26 26 0 0 1 52 0v125"/><path d="M110 35V9M99 20h22M12 277h188M12 288h188"/></g></svg>
                    </a>
                    ${overview.statistics.slice(0, 5).map(stat => `<a class="co-stat" href="${stat.route}">
                        <span class="co-stat-top"><span class="co-stat-label">${stat.label}</span><span class="co-stat-icon">${icon(stat.icon)}</span></span>
                        <span class="co-stat-value">${stat.value}</span>
                        <span class="co-stat-bottom"><span class="co-stat-description">${stat.description}</span>${icon('arrow')}</span>
                    </a>`).join('')}
                </div>
                <div class="co-content-grid">
                    <div class="co-side-column">
                        <section class="co-panel co-review" aria-labelledby="co-review-title">
                            <div class="co-panel-heading"><div><span class="co-panel-kicker">Your next steps</span><h2 id="co-review-title" tabindex="-1">Pending actions</h2></div><span class="co-count">${counts ? total : '—'}<small>${counts ? 'pending' : 'unavailable'}</small></span></div>
                            <div class="co-review-list">
                                ${overview.reviews.map(item => `<a class="co-review-link${counts && item.value ? ' has-pending' : ''}" href="${item.route}"><span class="co-review-indicator" aria-hidden="true">${icon(counts && item.value ? 'chart' : 'check')}</span><span class="co-review-label"><span>${item.title}</span><small>${!counts ? 'Unable to load' : item.value ? 'Needs your attention' : 'Up to date'}</small></span><strong class="co-review-count">${counts ? item.value : '—'}</strong><span class="co-review-action">${icon('arrow')}<span class="co-sr-only">${counts && item.value ? 'Review' : 'View'}</span></span></a>`).join('')}
                            </div>
                            <div class="co-review-foot">${icon('check')}<span>${!counts ? 'Dashboard data could not be loaded.' : total ? 'A few small steps keep every placement moving.' : 'All reviews and assignments are up to date.'}</span></div>
                        </section>
                    </div>
                    <section class="co-panel co-workspace" aria-labelledby="co-workspace-title">
                        <div class="co-panel-heading"><div><span class="co-panel-kicker">Everything in reach</span><h2 id="co-workspace-title">Your college workspace</h2></div><span class="co-workspace-mark">${icon('building')}</span></div>
                        <nav aria-label="Coordinator workspace">
                            ${overview.workspace.map(item => `<a class="co-workspace-link" href="${item.route}"><span class="co-workspace-icon">${icon(item.icon)}</span><span class="co-workspace-copy"><strong>${item.title}</strong><span>${item.description}</span></span>${icon('arrow')}</a>`).join('')}
                        </nav>
                    </section>
                </div>
                <aside class="co-note" aria-labelledby="co-note-title">
                            <h2 id="co-note-title">${icon('check')}Keep every placement on track</h2>
                            <p>Assign an academic supervisor to each trainee and review placement progress before approving final completion.</p>
                            <a href="ManageStudents.aspx">Review assignments ${icon('arrow')}</a>
                </aside>
            </section>`;
        main.querySelector('[data-export-dashboard]')?.addEventListener('click', () => {
            const rows = [...overview.statistics.map(item => [item.label, item.value, item.description]),
                ...overview.reviews.map(item => [item.title, item.value, 'Pending actions'])];
            window.UimsExcelExport?.download(`coordinator-dashboard-${new Date().toISOString().slice(0,10)}.csv`, ['Metric','Value','Description'], rows);
        });
        main.querySelector('[data-dashboard-error]').textContent = state.error || '';
        if (footer) footer.innerHTML = '<span>Yarmouk University</span><span>Faculty of Information Technology and Computer Science</span>';
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', mount, { once: true });
    else mount();
})();
