(() => {
    'use strict';
    const team = document.getElementById('team');
    if (!team) return;
    const $ = (selector, scope = team) => scope.querySelector(selector);
    const $$ = (selector, scope = team) => Array.from(scope.querySelectorAll(selector));
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const teamMembers = [
        { id: '01', nameEn: 'Wissam Khreisat', role: 'Scrum Methodology Coordinator (Scrum Master)', badgeRole: 'Scrum Master', icon: 'fa-compass-drafting', imgUrl: '../Content/Images/Team/wessam_khraisat-transparent.png', bio: 'Coordinates work within the Scrum framework, organizes development sprints, tracks the removal of blockers, and ensures that system components integrate smoothly.', duties: ['Sprint planning', 'Facilitating team collaboration', 'Monitoring module integration'], areas: ['Scrum framework coordination', 'Development progress tracking', 'System component integration'], social: { linkedin: 'https://www.linkedin.com/in/divbux', github: 'https://github.com/wessamkhraisat98' } },
        { id: '02', nameEn: 'Amneh Al-Hazaimeh', role: 'Product Owner', badgeRole: 'Product Owner', icon: 'fa-crown', imgUrl: '../Content/Images/Team/Amneh Al-hazaimeh-transparent.png', bio: 'Defines the product vision and analyzes its requirements to meet the needs of Yarmouk University, while keeping the user experience of each role in focus.', duties: ['Analyzing system requirements', 'Managing the product requirements backlog', 'Mapping user journeys'], areas: ['Product analysis', 'User journey design', 'Development requirements management'], social: { linkedin: 'https://www.linkedin.com/in/amneh-alhazaimeh-a01a7a340/?isSelfProfile=true', github: 'https://github.com/amneh-01' } },
        { id: '03', nameEn: 'Ghaith Ammourah', role: 'Quality Assurance Engineer', badgeRole: 'QA Engineer', icon: 'fa-shield-halved', imgUrl: '../Content/Images/Team/ghaith-amourah-transparent.png', bio: 'Tests system functions, reviews user roles and permissions, and checks data flow to confirm that it meets the project requirements.', duties: ['System function testing', 'Role and permission review', 'Data integrity verification'], areas: ['Quality testing', 'Authentication and permissions review', 'Documenting technical findings'], social: { linkedin: 'https://www.linkedin.com/in/ghaith-amourah-0916b8434', github: 'https://github.com/gh2ith' } },
        { id: '04', nameEn: 'Saif Khatatbeh', role: 'Full-Stack Developer', badgeRole: 'Full-Stack Developer', icon: 'fa-wand-magic-sparkles', imgUrl: '../Content/Images/Team/Sief Alkhatatbeh-transparent.png', bio: 'Designs and builds the system interfaces and user experience, develops system functions with ASP.NET C#, and connects them to Firebase services.', duties: ['Interface and user experience design', 'Developing system functions with ASP.NET C#', 'Integration with Firebase services'], areas: ['Interface development', 'Back-end development', 'User experience design'], social: { linkedin: 'https://www.linkedin.com/in/saif-khatatbeh-1a6ab1317/?isSelfProfile=true', github: 'https://github.com/saifkhatatbeh17-droid' } },
        { id: '05', nameEn: 'Dana Al-Khatib', role: 'Full-Stack Developer', badgeRole: 'Full-Stack Developer', icon: 'fa-laptop-code', imgUrl: '../Content/Images/Team/Dana Alkhateeb-transparent.png', bio: 'Develops role-based interfaces, carries out the application and placement procedures, and follows student progress during field training.', duties: ['Role-based interface development', 'Organizing application and placement procedures', 'Tracking student progress'], areas: ['User interfaces', 'System development with ASP.NET Web Forms', 'Workflow procedures'], social: { linkedin: 'https://www.linkedin.com/in/dana-alkhateeb-3636a5422?utm_source=share_via&utm_content=profile&utm_medium=member_ios', github: 'https://github.com/DanaAlkhateeb123' } },
        { id: '06', nameEn: 'Haya Ahmad', role: 'Full-Stack Developer', badgeRole: 'Full-Stack Developer', icon: 'fa-layer-group', imgUrl: '../Content/Images/Team/Haya Ahmed-transparent.png', bio: 'Develops the functions for managing training opportunities and daily tasks, along with academic and field assessment forms.', duties: ['Training opportunity management', 'Field task tracking', 'Academic and field assessment forms'], areas: ['Task management', 'Assessment procedure development', 'Data synchronization'], social: { linkedin: 'https://www.linkedin.com/in/haya-ahmed-5a1314333/', github: 'https://github.com/Haya155' } },
        { id: '07', nameEn: 'Amani', role: 'Full-Stack Developer', badgeRole: 'Full-Stack Developer', icon: 'fa-microchip', imgUrl: '../Content/Images/Team/Amani Alazzam-transparent.png', bio: 'Develops sign-in and user and permission management with Firebase Authentication, and builds responsive Arabic interfaces.', duties: ['Sign-in management', 'User and permission management', 'Responsive Arabic interface development'], areas: ['Firebase Authentication', 'Permission management', 'Cloud data modeling'], social: { linkedin: 'http://www.linkedin.com/in/amani-azzam-57359936a', github: 'https://github.com/amanialazzam04/Amaniazzam' } }
    ];
    const roster = $('#roster');
    const panel = $('#dossier-panel');
    const dossier = $('#dossier');
    let activeIndex = 0;
    const safeText = (value) => value.replace(/[&<>"']/g, (character) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character]));
    function placeholder(member) {
        const initials = member.nameEn.split(/\s+/).map((part) => part[0]).slice(0, 2).join('');
        return 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 340 425"><rect width="340" height="425" fill="#DDE6DD"/><text x="170" y="236" text-anchor="middle" font-family="Georgia,serif" font-size="96" fill="#0C3026">' + initials + '</text></svg>');
    }
    function setImage(image, member) {
        image.onerror = () => { image.onerror = null; image.src = placeholder(member); };
        image.src = member.imgUrl;
    }
    roster.innerHTML = teamMembers.map((member, index) => '<button class="roster-tab" type="button" role="tab" id="tab-' + index + '" aria-controls="dossier-panel" aria-selected="false" tabindex="-1" data-index="' + index + '"><span class="roster-num">' + member.id + '</span><img class="roster-thumb" src="' + safeText(member.imgUrl) + '" alt="" width="52" height="52" loading="lazy" decoding="async"><span class="roster-text"><span class="roster-name">' + safeText(member.nameEn) + '</span><span class="roster-role">' + safeText(member.role) + '</span></span></button>').join('');
    $$('.roster-thumb').forEach((image, index) => setImage(image, teamMembers[index]));
    function render(index) {
        const member = teamMembers[index];
        $('#dossierNum').textContent = member.id;
        $('#dossierBadgeRole').textContent = member.badgeRole;
        $('#dossierRole').innerHTML = '<i class="fa-solid ' + safeText(member.icon) + '" aria-hidden="true"></i><span>Member ' + member.id + ' · ' + safeText(member.role) + '</span>';
        $('#dossierName').textContent = member.nameEn;
        $('#dossierBio').textContent = member.bio;
        $$('.dossier-social-button').forEach((link) => {
            const platform = link.dataset.platform;
            const profileUrl = member.social && member.social[platform];
            const pending = $('.social-pending', link);
            if (profileUrl) {
                link.href = profileUrl;
                link.target = '_blank';
                link.rel = 'noopener noreferrer';
                link.removeAttribute('aria-disabled');
                link.removeAttribute('tabindex');
                link.setAttribute('aria-label', member.nameEn + ' on ' + (platform === 'linkedin' ? 'LinkedIn' : 'GitHub') + ' (opens in a new tab)');
                pending.hidden = true;
            } else {
                link.removeAttribute('href');
                link.removeAttribute('target');
                link.removeAttribute('rel');
                link.setAttribute('aria-disabled', 'true');
                link.tabIndex = -1;
                link.setAttribute('aria-label', (platform === 'linkedin' ? 'LinkedIn' : 'GitHub') + ' profile coming soon for ' + member.nameEn);
                pending.hidden = false;
            }
        });
        $('#dossierDuties').innerHTML = member.duties.map((item) => '<li><i class="fa-solid fa-circle-check" aria-hidden="true"></i><span>' + safeText(item) + '</span></li>').join('');
        $('#dossierAreas').innerHTML = member.areas.map((item) => '<li class="chip">' + safeText(item) + '</li>').join('');
        const image = $('#dossierImg');
        image.alt = 'Portrait of ' + member.nameEn;
        setImage(image, member);
        panel.setAttribute('aria-labelledby', 'tab-' + index);
        $('#activeCounter').textContent = member.id;
    }
    function applySelection(index, focus) {
        activeIndex = (index + teamMembers.length) % teamMembers.length;
        const tabs = Array.from(team.querySelectorAll('.roster-tab'));
        tabs.forEach((tab, position) => {
            const selected = position === activeIndex;
            tab.setAttribute('aria-selected', String(selected));
            tab.tabIndex = selected ? 0 : -1;
        });
        if (focus) tabs[activeIndex].focus({ preventScroll: true });
        const selectedTab = tabs[activeIndex];
        const tabRect = selectedTab.getBoundingClientRect();
        const rosterRect = roster.getBoundingClientRect();
        const horizontalTarget = roster.scrollLeft + tabRect.left - rosterRect.left - (roster.clientWidth - tabRect.width) / 2;
        roster.scrollTo({ left: horizontalTarget, behavior: reduceMotion.matches ? 'auto' : 'smooth' });
        render(activeIndex);
    }

    function select(index, focus) {
        const nextIndex = (index + teamMembers.length) % teamMembers.length;
        if (nextIndex === activeIndex) {
            if (focus) Array.from(team.querySelectorAll('.roster-tab'))[activeIndex].focus({ preventScroll: true });
            return;
        }
        const sourceImage = Array.from(team.querySelectorAll('.roster-tab'))[nextIndex].querySelector('.roster-thumb');
        const targetImage = $('#dossierImg');
        if (reduceMotion.matches || typeof document.startViewTransition !== 'function') {
            applySelection(nextIndex, focus);
            return;
        }

        sourceImage.style.viewTransitionName = 'team-member-photo';
        let transition;
        try {
            transition = document.startViewTransition(() => {
                applySelection(nextIndex, focus);
                sourceImage.style.viewTransitionName = '';
                targetImage.style.viewTransitionName = 'team-member-photo';
            });
        } catch (error) {
            sourceImage.style.viewTransitionName = '';
            applySelection(nextIndex, focus);
            return;
        }
        const clearTransitionNames = () => {
            sourceImage.style.viewTransitionName = '';
            targetImage.style.viewTransitionName = '';
        };
        transition.ready.then(() => {
            const info = $('.dossier-info');
            if (info && typeof info.animate === 'function') {
                info.animate(
                    [{ opacity: 0, transform: 'translateY(7px)' }, { opacity: 1, transform: 'translateY(0)' }],
                    { duration: 260, easing: 'cubic-bezier(.22, .86, .2, 1)', fill: 'both' }
                );
            }
        }).catch(() => { });
        transition.finished.then(clearTransitionNames, clearTransitionNames);
    }
    roster.addEventListener('click', (event) => { const tab = event.target.closest('.roster-tab'); if (tab) select(Number(tab.dataset.index), true); });
    roster.addEventListener('keydown', (event) => { const step = { ArrowDown: 1, ArrowRight: 1, ArrowUp: -1, ArrowLeft: -1 }[event.key]; if (step !== undefined) { event.preventDefault(); select(activeIndex + step, true); } else if (event.key === 'Home') { event.preventDefault(); select(0, true); } else if (event.key === 'End') { event.preventDefault(); select(teamMembers.length - 1, true); } });
    function revealSelectedDossier() {
        const panelTop = panel.getBoundingClientRect().top + window.scrollY;
        const navbar = document.querySelector('.yu-public-navbar');
        const navbarRect = navbar ? navbar.getBoundingClientRect() : null;
        const visibleNavbarBottom = navbarRect && navbarRect.bottom > 0 && navbarRect.top < window.innerHeight ? navbarRect.bottom : 0;
        const top = Math.max(0, panelTop - visibleNavbarBottom - 18);
        window.scrollTo({ top, behavior: reduceMotion.matches ? 'auto' : 'smooth' });
    }
    function selectAndReveal(index) {
        select(index, false);
        window.requestAnimationFrame(() => window.requestAnimationFrame(revealSelectedDossier));
    }
    $('#prevMember').addEventListener('click', () => selectAndReveal(activeIndex - 1));
    $('#nextMember').addEventListener('click', () => selectAndReveal(activeIndex + 1));
    panel.addEventListener('pointermove', (event) => { if (event.pointerType !== 'mouse' || reduceMotion.matches) return; const rect = panel.getBoundingClientRect(); panel.style.setProperty('--tilt-x', ((.5 - (event.clientY - rect.top) / rect.height) * 3) + 'deg'); panel.style.setProperty('--tilt-y', (((event.clientX - rect.left) / rect.width - .5) * 4) + 'deg'); });
    panel.addEventListener('pointerleave', () => { panel.style.setProperty('--tilt-x', '0deg'); panel.style.setProperty('--tilt-y', '0deg'); });
    const pauseReasons = new Set();
    const setPauseReason = (reason, active) => {
        if (active) pauseReasons.add(reason);
        else pauseReasons.delete(reason);
        team.classList.toggle('is-paused', pauseReasons.size > 0);
    };
    dossier.addEventListener('focusin', () => setPauseReason('focus', true));
    dossier.addEventListener('focusout', (event) => {
        if (!dossier.contains(event.relatedTarget)) setPauseReason('focus', false);
    });
    const hero = document.getElementById('home');
    const dataflowCanvas = hero ? hero.querySelector('#heroDataflow') : null;
    const roleNodes = hero ? Array.from(hero.querySelectorAll('.role-node')) : [];
    const routes = hero ? Array.from(hero.querySelectorAll('[data-role-path]')) : [];
    const readoutTitle = hero && hero.querySelector('#readoutTitle');
    const readoutText = hero && hero.querySelector('#readoutText');
    let renderDataflow = null;

    function setRole(node) {
        roleNodes.forEach((item) => {
            const selected = item === node;
            item.classList.toggle('is-active', selected);
            item.setAttribute('aria-pressed', String(selected));
        });
        routes.forEach((route) => {
            route.classList.toggle('is-active', route.dataset.rolePath === node.dataset.role);
        });
        readoutTitle.textContent = node.dataset.title;
        readoutText.textContent = node.dataset.description;
        if (dataflowCanvas) {
            dataflowCanvas.dataset.activeRole = node.dataset.role;
            if (renderDataflow) renderDataflow(performance.now(), reduceMotion.matches);
        }
        const readout = hero.querySelector('.map-readout');
        if (typeof readout.animate === 'function') {
            const frames = reduceMotion.matches
                ? [{ opacity: .45 }, { opacity: 1 }]
                : [{ opacity: .45, transform: 'translateY(5px)' }, { opacity: 1, transform: 'translateY(0)' }];
            readout.animate(frames, { duration: reduceMotion.matches ? 160 : 260, easing: 'cubic-bezier(.16, 1, .3, 1)' });
        }
    }

    roleNodes.forEach((node) => node.addEventListener('click', () => setRole(node)));

    if (hero && dataflowCanvas) {
        const context = dataflowCanvas.getContext('2d', { alpha: true });
        if (context) {
            const streams = [
                { role: 'student', start: [.02, .16], bend: [.28, .08, .78, .12], duration: 4.8, phase: .02 },
                { role: 'provider', start: [.98, .18], bend: [.78, .12, .3, .1], duration: 5.5, phase: .31 },
                { role: 'supervisor', start: [.04, .83], bend: [.24, .9, .76, .88], duration: 5.2, phase: .57 },
                { role: 'coordinator', start: [.98, .78], bend: [.76, .9, .3, .84], duration: 4.6, phase: .79 }
            ];
            const pointer = { x: 0, y: 0, active: false };
            let width = 0;
            let height = 0;
            let frame = 0;
            let visible = true;
            let pixelRatio = 1;

            function resizeDataflow() {
                const bounds = dataflowCanvas.getBoundingClientRect();
                if (!bounds.width || !bounds.height) return;
                width = bounds.width;
                height = bounds.height;
                pixelRatio = Math.min(window.devicePixelRatio || 1, 1.5);
                dataflowCanvas.width = Math.round(width * pixelRatio);
                dataflowCanvas.height = Math.round(height * pixelRatio);
                context.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);
                renderDataflow(performance.now(), reduceMotion.matches);
            }

            function curvePoint(stream, progress, hub, bendX, bendY) {
                const sx = stream.start[0] * width;
                const sy = stream.start[1] * height;
                const c1x = sx + (hub.x - sx) * stream.bend[0] + bendX;
                const c1y = sy + (hub.y - sy) * stream.bend[1] + bendY;
                const c2x = sx + (hub.x - sx) * stream.bend[2] - bendX * .38;
                const c2y = sy + (hub.y - sy) * stream.bend[3] - bendY * .38;
                const t = progress;
                const inverse = 1 - t;
                const x = inverse ** 3 * sx + 3 * inverse ** 2 * t * c1x + 3 * inverse * t ** 2 * c2x + t ** 3 * hub.x;
                const y = inverse ** 3 * sy + 3 * inverse ** 2 * t * c1y + 3 * inverse * t ** 2 * c2y + t ** 3 * hub.y;
                return { x, y, sx, sy, c1x, c1y, c2x, c2y };
            }

            function drawHexagon(x, y, radius, rotation, alpha, color) {
                context.beginPath();
                for (let corner = 0; corner < 6; corner += 1) {
                    const angle = rotation + corner * Math.PI / 3;
                    const px = x + Math.cos(angle) * radius;
                    const py = y + Math.sin(angle) * radius;
                    if (corner === 0) context.moveTo(px, py);
                    else context.lineTo(px, py);
                }
                context.closePath();
                context.strokeStyle = color;
                context.globalAlpha = alpha;
                context.stroke();
                context.globalAlpha = 1;
            }

            renderDataflow = (now, still = false) => {
                if (!width || !height) return;
                context.clearRect(0, 0, width, height);
                const hub = { x: width * .53, y: height * .51 };
                const activeRole = dataflowCanvas.dataset.activeRole || 'student';
                const activeStream = streams.find((stream) => stream.role === activeRole);
                const bendX = pointer.active ? (pointer.x - hub.x) * .12 : 0;
                const bendY = pointer.active ? (pointer.y - hub.y) * .12 : 0;

                streams.forEach((stream) => {
                    const selected = stream === activeStream;
                    const start = curvePoint(stream, 0, hub, bendX, bendY);
                    context.beginPath();
                    context.moveTo(start.sx, start.sy);
                    context.bezierCurveTo(start.c1x, start.c1y, start.c2x, start.c2y, hub.x, hub.y);
                    context.lineWidth = selected ? 1.35 : 1;
                    context.strokeStyle = selected ? 'rgba(232,217,174,.46)' : 'rgba(219,231,223,.17)';
                    context.stroke();

                    context.beginPath();
                    context.arc(start.sx, start.sy, selected ? 3.2 : 2.3, 0, Math.PI * 2);
                    context.fillStyle = selected ? '#E8D9AE' : 'rgba(190,211,198,.62)';
                    context.shadowColor = selected ? 'rgba(232,217,174,.7)' : 'transparent';
                    context.shadowBlur = selected ? 12 : 0;
                    context.fill();
                    context.shadowBlur = 0;

                    const packetCount = selected ? 3 : 2;
                    for (let packet = 0; packet < packetCount; packet += 1) {
                        const travel = still ? .62 : (now / (stream.duration * 1000) + stream.phase + packet / packetCount) % 1;
                        const tailStart = Math.max(0, travel - (selected ? .075 : .045));
                        context.beginPath();
                        for (let sample = 0; sample <= 9; sample += 1) {
                            const t = tailStart + (travel - tailStart) * sample / 9;
                            const point = curvePoint(stream, t, hub, bendX, bendY);
                            if (sample === 0) context.moveTo(point.x, point.y);
                            else context.lineTo(point.x, point.y);
                        }
                        context.strokeStyle = selected ? 'rgba(232,217,174,.8)' : 'rgba(145,184,161,.4)';
                        context.lineWidth = selected ? 2.2 : 1.35;
                        context.shadowColor = selected ? 'rgba(232,217,174,.82)' : 'rgba(145,184,161,.4)';
                        context.shadowBlur = selected ? 13 : 7;
                        context.stroke();
                        context.shadowBlur = 0;
                        const head = curvePoint(stream, travel, hub, bendX, bendY);
                        context.beginPath();
                        context.arc(head.x, head.y, selected ? 2.8 : 1.9, 0, Math.PI * 2);
                        context.fillStyle = selected ? '#F3E6BF' : 'rgba(188,215,198,.88)';
                        context.shadowColor = selected ? 'rgba(232,217,174,.92)' : 'rgba(145,184,161,.65)';
                        context.shadowBlur = selected ? 14 : 8;
                        context.fill();
                        context.shadowBlur = 0;
                    }
                });

                context.globalCompositeOperation = 'lighter';
                context.fillStyle = 'rgba(232,217,174,.075)';
                context.beginPath();
                context.arc(hub.x, hub.y, 48, 0, Math.PI * 2);
                context.fill();
                context.globalCompositeOperation = 'source-over';
                context.lineWidth = 1;
                drawHexagon(hub.x, hub.y, 35, still ? 0 : now * .000035, .76, 'rgba(232,217,174,.72)');
                drawHexagon(hub.x, hub.y, 25, still ? Math.PI / 6 : -now * .000055, .45, 'rgba(174,205,188,.8)');
                context.beginPath();
                context.moveTo(hub.x - 9, hub.y);
                context.lineTo(hub.x, hub.y - 9);
                context.lineTo(hub.x + 9, hub.y);
                context.lineTo(hub.x, hub.y + 9);
                context.closePath();
                context.fillStyle = '#E8D9AE';
                context.shadowColor = 'rgba(232,217,174,.8)';
                context.shadowBlur = 18;
                context.fill();
                context.shadowBlur = 0;
            };

            function animateDataflow(now) {
                frame = 0;
                if (!visible || document.hidden) return;
                renderDataflow(now, reduceMotion.matches);
                if (!reduceMotion.matches) frame = window.requestAnimationFrame(animateDataflow);
            }

            function startDataflow() {
                if (frame || !visible || document.hidden) return;
                renderDataflow(performance.now(), reduceMotion.matches);
                if (!reduceMotion.matches) frame = window.requestAnimationFrame(animateDataflow);
            }

            hero.addEventListener('pointermove', (event) => {
                if (event.pointerType !== 'mouse' || reduceMotion.matches) return;
                const bounds = dataflowCanvas.getBoundingClientRect();
                if (event.clientX < bounds.left || event.clientX > bounds.right || event.clientY < bounds.top || event.clientY > bounds.bottom) {
                    pointer.active = false;
                    return;
                }
                pointer.x = event.clientX - bounds.left;
                pointer.y = event.clientY - bounds.top;
                pointer.active = true;
            }, { passive: true });
            hero.addEventListener('pointerleave', () => { pointer.active = false; }, { passive: true });

            if ('ResizeObserver' in window) new ResizeObserver(resizeDataflow).observe(dataflowCanvas);
            else window.addEventListener('resize', resizeDataflow, { passive: true });
            if ('IntersectionObserver' in window) {
                new IntersectionObserver(([entry]) => {
                    visible = entry.isIntersecting;
                    if (visible) startDataflow();
                    else if (frame) { window.cancelAnimationFrame(frame); frame = 0; }
                }, { rootMargin: '80px' }).observe(hero);
            }
            document.addEventListener('visibilitychange', () => {
                if (document.hidden && frame) { window.cancelAnimationFrame(frame); frame = 0; }
                else startDataflow();
            });
            reduceMotion.addEventListener('change', startDataflow);
            resizeDataflow();
            startDataflow();
        }
    }
    if (hero) {
        const interactionLight = document.createElement('div');
        interactionLight.className = 'interaction-light';
        interactionLight.setAttribute('aria-hidden', 'true');
        hero.prepend(interactionLight);
    }
    document.querySelectorAll('#home, #architecture .arch-card, #partner .partner-board').forEach((surface) => {
        let pointerFrame = 0;
        let pointerX = 55;
        let pointerY = 35;
        surface.addEventListener('pointermove', (event) => {
            if (event.pointerType !== 'mouse' || reduceMotion.matches) return;
            const rect = surface.getBoundingClientRect();
            pointerX = (event.clientX - rect.left) / rect.width * 100;
            pointerY = (event.clientY - rect.top) / rect.height * 100;
            if (pointerFrame) return;
            pointerFrame = requestAnimationFrame(() => {
                surface.style.setProperty('--light-x', pointerX.toFixed(1) + '%');
                surface.style.setProperty('--light-y', pointerY.toFixed(1) + '%');
                surface.classList.add('is-pointer-active');
                pointerFrame = 0;
            });
        }, { passive: true });
        surface.addEventListener('pointerleave', () => {
            if (pointerFrame) cancelAnimationFrame(pointerFrame);
            pointerFrame = 0;
            surface.classList.remove('is-pointer-active');
        }, { passive: true });
    });
    if ('IntersectionObserver' in window) {
        const teamObserver = new IntersectionObserver(([entry]) => {
            setPauseReason('offscreen', !entry.isIntersecting);
        }, { threshold: 0.15 });
        teamObserver.observe(team);

        const loopObserver = new IntersectionObserver((entries) => {
            entries.forEach((entry) => entry.target.classList.toggle('is-offscreen', !entry.isIntersecting));
        }, { rootMargin: '120px 0px' });
        document.querySelectorAll('#home [data-loop], #architecture[data-loop]').forEach((element) => loopObserver.observe(element));

        const revealElements = Array.from(document.querySelectorAll('#team [data-reveal], #architecture [data-reveal], #partner [data-reveal]'));
        if (reduceMotion.matches) {
            revealElements.forEach((element) => element.classList.add('is-visible'));
        } else {
            const revealObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach((entry) => {
                    if (!entry.isIntersecting) return;
                    entry.target.classList.add('is-visible');
                    if (entry.target.matches('.arch-card') && typeof entry.target.animate === 'function') {
                        const cards = Array.from(document.querySelectorAll('#architecture .arch-card'));
                        entry.target.animate(
                            [{ opacity: .72, translate: '0 12px' }, { opacity: 1, translate: '0 0' }],
                            { duration: 460, delay: Math.max(0, cards.indexOf(entry.target)) * 45, easing: 'cubic-bezier(.16,1,.3,1)' }
                        );
                    }
                    observer.unobserve(entry.target);
                });
            }, { threshold: 0.12, rootMargin: '0px 0px -6% 0px' });
            revealElements.forEach((element) => revealObserver.observe(element));
        }
    } else {
        document.querySelectorAll('#team [data-reveal], #architecture [data-reveal], #partner [data-reveal]').forEach((element) => element.classList.add('is-visible'));
    }

    reduceMotion.addEventListener('change', (event) => {
        if (event.matches) {
            document.querySelectorAll('#team [data-reveal], #architecture [data-reveal], #partner [data-reveal]').forEach((element) => element.classList.add('is-visible'));
        }
        panel.style.setProperty('--tilt-x', '0deg');
        panel.style.setProperty('--tilt-y', '0deg');
        if (renderDataflow) renderDataflow(performance.now(), event.matches);
    });

    document.addEventListener('visibilitychange', () => {
        document.documentElement.classList.toggle('page-hidden', document.hidden);
        setPauseReason('hidden', document.hidden);
    });
    const initialTab = roster.querySelector('.roster-tab');
    initialTab.setAttribute('aria-selected', 'true');
    initialTab.tabIndex = 0;
    render(0);
})();