(function(){document.documentElement.lang='en';var labels={"homeLink": "Home", "aboutLink": "About us", "contactLink": "Contact us", "topbar": "Hashemite Kingdom of Jordan \u2022 Yarmouk University \u2022 Faculty of Information Technology and Computer Sciences", "semester": "Second Semester 2025/2026", "portal": "Unified sign-in portal", "signIn": "Sign in", "university": "Yarmouk University", "faculty": "Faculty of Information Technology and Computer Sciences", "footerDescription": "The University Internship Management System (UIMS) organizes field training for students of the Faculty of Information Technology and Computer Sciences.", "majorCS": "Computer Science (CS)", "majorSE": "Software Engineering (SE)", "majorCyber": "Cybersecurity", "majorCIS": "Computer Information Systems (CIS)", "majorAI": "Artificial Intelligence (AI)", "majorApps": "Web and Mobile Application Development", "portals": "System portals and dashboards", "portalLink": "Unified sign-in portal", "studentDashboard": "Student dashboard", "academicDashboard": "Academic supervisor dashboard", "companyDashboard": "Company supervisor dashboard", "coordinatorDashboard": "Training coordinator dashboard", "copyright": "Copyright \u00a9 "};document.querySelectorAll('[data-home-shell-i18n]').forEach(function(el){var text=labels[el.dataset.homeShellI18n];if(text)el.textContent=text;});document.title='About us | Yarmouk University';}());
(function () {
            var page = document.getElementById('aboutPage');
            if (!page) return;
            var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
            var hero = page.querySelector('.about-hero');
            if (!reduceMotion && hero && window.matchMedia('(hover: hover) and (pointer: fine)').matches) {
                hero.addEventListener('pointermove', function (event) {
                    var rect = hero.getBoundingClientRect();
                    hero.style.setProperty('--hero-x', ((event.clientX - rect.left) / rect.width * 100) + '%');
                    hero.style.setProperty('--hero-y', ((event.clientY - rect.top) / rect.height * 100) + '%');
                });
            }
            var counters = page.querySelectorAll('.about-counter');

            function countUp(node) {
                var target = parseInt(node.getAttribute('data-count'), 10) || 0;
                if (reduceMotion) { node.textContent = target; return; }
                var start = performance.now();
                var duration = 1050;
                node.textContent = '0';
                function frame(now) {
                    var progress = Math.min((now - start) / duration, 1);
                    var eased = 1 - Math.pow(1 - progress, 4);
                    node.textContent = String(Math.round(target * eased));
                    if (progress < 1) window.requestAnimationFrame(frame);
                }
                window.requestAnimationFrame(frame);
            }

            if ('IntersectionObserver' in window && !reduceMotion) {
                page.classList.add('about-motion-ready');
                var observer = new IntersectionObserver(function (entries) {
                    entries.forEach(function (entry) {
                        if (!entry.isIntersecting) return;
                        entry.target.classList.add('is-visible');
                        entry.target.querySelectorAll('.about-counter').forEach(countUp);
                        observer.unobserve(entry.target);
                    });
                }, { threshold: 0.16, rootMargin: '0px 0px -35px 0px' });
                page.querySelectorAll('.about-reveal, .about-fact').forEach(function (node) { observer.observe(node); });
            } else {
                counters.forEach(function (node) { node.textContent = node.getAttribute('data-count'); });
            }

            if (reduceMotion || !window.matchMedia('(hover: hover) and (pointer: fine)').matches) return;

            page.querySelectorAll('[data-tilt]').forEach(function (card) {
                card.addEventListener('pointermove', function (event) {
                    var rect = card.getBoundingClientRect();
                    var x = (event.clientX - rect.left) / rect.width;
                    var y = (event.clientY - rect.top) / rect.height;
                    card.style.setProperty('--spot-x', (x * 100) + '%');
                    card.style.setProperty('--spot-y', (y * 100) + '%');
                    card.style.setProperty('--tilt-x', ((x - .5) * 7) + 'deg');
                    card.style.setProperty('--tilt-y', ((.5 - y) * 7) + 'deg');
                });
                card.addEventListener('pointerleave', function () {
                    card.style.setProperty('--tilt-x', '0deg');
                    card.style.setProperty('--tilt-y', '0deg');
                });
            });

            page.querySelectorAll('.about-magnetic').forEach(function (button) {
                button.addEventListener('pointermove', function (event) {
                    var rect = button.getBoundingClientRect();
                    button.style.setProperty('--mag-x', (((event.clientX - rect.left) / rect.width - .5) * 7) + 'px');
                    button.style.setProperty('--mag-y', (((event.clientY - rect.top) / rect.height - .5) * 5) + 'px');
                });
                button.addEventListener('pointerleave', function () {
                    button.style.setProperty('--mag-x', '0px');
                    button.style.setProperty('--mag-y', '0px');
                });
            });
        }());
(function(){
 var start=document.getElementById('startCampusTour'),viewer=document.getElementById('campusViewer');
 if(!start||!viewer)return;
 start.addEventListener('click',function(){
  var frame=document.createElement('iframe');frame.src='https://it.yu.edu.jo/vt/';frame.title='Yarmouk IT faculty interactive virtual tour';frame.allow='fullscreen; gyroscope; accelerometer';frame.allowFullscreen=true;
  viewer.replaceChildren(frame);document.getElementById('campusStatus').textContent='Tour loading. If it does not appear, open it in a new tab using the link alongside.';
  frame.addEventListener('load',function(){document.getElementById('campusStatus').textContent='Explore the faculty using the tour controls. You can also open it in a new tab.';});
 });
}());
(function(){
 var page=document.getElementById('aboutPage');if(!page)return;
 var motion=document.createElement('button');motion.type='button';motion.className='about-button about-button-light';motion.textContent='Pause animations';motion.setAttribute('aria-pressed','false');
 page.querySelector('.about-hero-actions').append(motion);
 motion.addEventListener('click',function(){var paused=page.classList.toggle('motion-paused');motion.setAttribute('aria-pressed',String(paused));motion.textContent=paused?'Resume animations':'Pause animations';});
 if('IntersectionObserver' in window){var observer=new IntersectionObserver(function(entries){entries.forEach(function(entry){if(entry.isIntersecting){entry.target.classList.add('journey-visible');observer.unobserve(entry.target);}});},{threshold:.15});page.querySelectorAll('.journey-track article').forEach(function(el){observer.observe(el);});}
}());
(function () {
    var art = document.getElementById('yuArt');
    var page = document.getElementById('aboutPage');
    var reduced = window.matchMedia('(prefers-reduced-motion: reduce)');
    if (!art || !window.matchMedia('(hover: hover) and (pointer: fine)').matches) return;
    art.addEventListener('pointermove', function (event) {
        if (reduced.matches || page.classList.contains('motion-paused')) return;
        var bounds = art.getBoundingClientRect();
        art.style.setProperty('--crest-y', (-14 + ((event.clientX - bounds.left) / bounds.width - .5) * 24) + 'deg');
        art.style.setProperty('--crest-x', (5 - ((event.clientY - bounds.top) / bounds.height - .5) * 14) + 'deg');
    });
    art.addEventListener('pointerleave', function () {
        art.style.removeProperty('--crest-x');
        art.style.removeProperty('--crest-y');
    });
}());
