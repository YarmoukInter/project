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
            { title: 'Students', description: 'Student accounts and academic records', route: 'ManageStudents.aspx' },
            { title: 'Academic supervisors', description: 'Faculty accounts and student assignments', route: 'ManageAcademicSupervisors.aspx' },
            { title: 'Partner companies', description: 'Training providers and company accounts', route: 'ManageCompanies.aspx' },
            { title: 'Student internships', description: 'Placements, training progress and completion', route: 'MonitorPlacements.aspx' }
        ],
        reviews: [
            { title: 'Training hours to review', value: counts?.pendingHours ?? 0, route: 'MonitorPlacements.aspx' },
            { title: 'Supervisor assignments', value: counts?.pendingAssignments ?? 0, route: 'AssignAcademicSupervisors.aspx' },
            { title: 'Final completion reviews', value: counts?.pendingCompletion ?? 0, route: 'FinalCompletionReview.aspx' }
        ]
    };

    const total = overview.reviews.reduce((sum,item) => sum + item.value, 0);
    function mount() {
        const form = document.querySelector('form');
        const main = form && form.querySelector('main');
        const header = form && form.querySelector('.yu-dashboard-main > header, :scope > header');
        const footer = form && form.querySelector('.yu-dashboard-main > footer, :scope > footer');
        if (!main || main.querySelector('.co-overview')) return;

        document.body.classList.add('co-dashboard');
        document.title = 'College overview — Yarmouk University';
        const script = Array.from(document.scripts).find(item => /\/CoordinatorPages\/Dashboard\.js(?:\?|$)/i.test(item.src));
        const applicationRoot = script ? new URL('../../', script.src) : new URL('../', location.href);
        const logo = new URL('assets/yarmouk-logo-on-light.svg', applicationRoot).href;

        if (header) header.innerHTML = `
            <a class="co-skip" href="#main">Skip to dashboard</a>
            <a class="co-brand" href="Dashboard.aspx" aria-label="Yarmouk University dashboard">
                <img src="${logo}" alt="" width="120" height="44" />
                <span class="co-brand-copy"><strong>Yarmouk University</strong><span>Internship administration</span></span>
            </a>
            <div class="co-header-actions">
                <a class="co-notifications" href="ContactMessages.aspx" aria-label="Open contact messages">${icon('bell')}</a>
                <a class="co-account" href="Account.aspx"><span class="co-avatar" aria-hidden="true">YU</span><span>University coordinator</span>${icon('chevron')}</a>
            </div>`;

        main.id = 'main';
        main.tabIndex = -1;
        main.innerHTML = `
            <section class="co-overview" aria-labelledby="co-title"><p role="status" data-dashboard-error></p>
                <div class="co-title-row">
                    <div class="co-title-group"><h1 id="co-title">College overview</h1></div>
                    <div class="co-actions">
                        <a class="co-button co-button-secondary" href="ManageAcademicSupervisors.aspx">${icon('people')}<span>Add supervisor</span></a>
                        <a class="co-button co-button-primary" href="ManageStudents.aspx">${icon('plus')}<span>Add student</span></a>
                    </div>
                </div>
                <div class="co-statistics" aria-label="Internship overview">
                    ${overview.statistics.map(stat => `<a class="co-stat" href="${stat.route}">
                        <span class="co-stat-icon">${icon(stat.icon)}</span>
                        <span class="co-stat-value">${stat.value}</span>
                        <span class="co-stat-label">${stat.label}</span>
                        <span class="co-stat-description">${stat.description}</span>
                    </a>`).join('')}
                </div>
                <div class="co-content-grid">
                    <section class="co-panel co-workspace" aria-labelledby="co-workspace-title">
                        <div class="co-panel-heading"><h2 id="co-workspace-title">Your college workspace</h2></div>
                        <p class="co-panel-intro">Manage the people and progress behind every internship.</p>
                        <nav aria-label="Coordinator workspace">
                            ${overview.workspace.map(item => `<a class="co-workspace-link" href="${item.route}"><span><strong>${item.title}</strong><span>${item.description}</span></span>${icon('arrow')}</a>`).join('')}
                        </nav>
                    </section>
                    <div class="co-side-column">
                        <section class="co-panel co-review" aria-labelledby="co-review-title">
                            <div class="co-panel-heading"><h2 id="co-review-title">Pending actions</h2><span class="co-count">${counts ? total + ' pending' : 'Unavailable'}</span></div>
                            <div class="co-review-list">
                                ${overview.reviews.map(item => `<a class="co-review-link" href="${item.route}"><strong class="co-review-count">${counts ? item.value : '—'}</strong><span class="co-review-label"><span>${item.title}</span><small>${!counts ? 'Unable to load' : item.value ? 'Needs review' : 'Up to date'}</small></span><span class="co-review-action">${counts && item.value ? 'Review' : 'View'} ${icon('arrow')}</span></a>`).join('')}
                            </div>
                        </section>
                        <aside class="co-note" aria-labelledby="co-note-title">
                            <h2 id="co-note-title">${icon('check')}Keep every placement on track</h2>
                            <p>Assign an academic supervisor to each trainee and review placement progress before approving final completion.</p>
                            <a href="AssignAcademicSupervisors.aspx">Review assignments ${icon('arrow')}</a>
                        </aside>
                    </div>
                </div>
            </section>`;
        main.querySelector('[data-dashboard-error]').textContent = state.error || '';
        if (footer) footer.innerHTML = '<span>Yarmouk University</span><span>Faculty of Information Technology and Computer Science</span>';
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', mount, { once: true });
    else mount();
})();
