(function(){
            var page=document.getElementById('aboutPage');if(!page)return;
            var shellWords={ar:{homeLink:'الرئيسية',aboutLink:'من نحن',contactLink:'تواصل معنا',topbar:'المملكة الأردنية الهاشمية • جامعة اليرموك • كلية تكنولوجيا المعلومات وعلوم الحاسوب',semester:'الفصل الثاني 2025/2026',portal:'بوابة الدخول الموحدة',signIn:'تسجيل الدخول',university:'جامعة اليرموك',faculty:'كلية تكنولوجيا المعلومات وعلوم الحاسوب',footerDescription:'منظومة التدريب الميداني والتقييم الأكاديمي (UIMS) لمتابعة التدريب العملي لطلبة كلية تكنولوجيا المعلومات.',majorCS:'علم الحاسوب (CS)',majorSE:'هندسة البرمجيات (SE)',majorCyber:'الأمن السيبراني',majorCIS:'نظم المعلومات (CIS)',majorAI:'الذكاء الاصطناعي (AI)',majorApps:'تطوير تطبيقات الويب والموبايل',portals:'البوابات واللوحات الرسمية للمنظومة',portalLink:'بوابة تسجيل الدخول الموحد',studentDashboard:'لوحة تحكم الطالب المتدرب',academicDashboard:'لوحة تحكم المشرف الأكاديمي',companyDashboard:'لوحة تحكم المشرف الميداني للشركة',coordinatorDashboard:'لوحة القيادة والمتابعة لمنسق التدريب',copyright:'جميع الحقوق محفوظة © '+new Date().getFullYear()+' جامعة اليرموك · إربد، الأردن'},en:{homeLink:'Home',aboutLink:'About us',contactLink:'Contact us',topbar:'Hashemite Kingdom of Jordan • Yarmouk University • Faculty of Information Technology and Computer Sciences',semester:'Second Semester 2025/2026',portal:'Unified sign-in portal',signIn:'Sign in',university:'Yarmouk University',faculty:'Faculty of Information Technology and Computer Sciences',footerDescription:'The University Internship Management System (UIMS) organizes field training for students of the Faculty of Information Technology and Computer Sciences.',majorCS:'Computer Science (CS)',majorSE:'Software Engineering (SE)',majorCyber:'Cybersecurity',majorCIS:'Computer Information Systems (CIS)',majorAI:'Artificial Intelligence (AI)',majorApps:'Web and Mobile Application Development',portals:'System portals and dashboards',portalLink:'Unified sign-in portal',studentDashboard:'Student dashboard',academicDashboard:'Academic supervisor dashboard',companyDashboard:'Company supervisor dashboard',coordinatorDashboard:'Training coordinator dashboard',copyright:'Copyright © '+new Date().getFullYear()+' Yarmouk University · Irbid, Jordan'}}, words={
                ar:{languageLabel:'English',kicker:'جامعة اليرموك · كلية تكنولوجيا المعلومات وعلوم الحاسوب',heroTitle:'منظومة واحدة، وتجربة تدريب أكثر وضوحًا',heroText:'منظومة التدريب الميداني والتقييم الأكاديمي (UIMS) تنظّم رحلة التدريب، وتجمع الطالب وجهة التدريب والمشرف الأكاديمي ومنسق الكلية في مساحة رقمية موحدة.',loginCta:'الدخول إلى المنظومة',contactCta:'تواصل معنا',university:'جامعة اليرموك',college:'كلية تكنولوجيا المعلومات وعلوم الحاسوب',systemLabel:'منظومة التدريب الميداني',facts:'حقائق عن التدريب',hoursFact:'أسابيع تدريب ميداني',creditsFact:'ساعات معتمدة',rolesFact:'أطراف تعمل معًا',storyKicker:'عن المنظومة',storyTitle:'من المعرفة الأكاديمية إلى خبرة عملية',storyP1:'تمنح تجربة التدريب الميداني الطلبة فرصة لتطبيق ما تعلموه في بيئة عمل حقيقية، والتعرف إلى متطلبات المهنة قبل التخرج.',storyP2:'تدعم UIMS هذه التجربة بتنظيم بيانات التدريب ومتابعة المهام والساعات والتقييمات، وتسهيل التواصل بين جميع الأطراف طوال فترة التدريب.',principle:'مسار واضح للطالب، ومتابعة منسقة للجامعة وجهة التدريب.',purposeKicker:'هدفنا',purposeTitle:'تجربة تدريب أكثر تنظيمًا وشفافية',purposeText:'تمكين الأطراف من متابعة الالتزامات والإنجازات والتقييمات ضمن خطوات مفهومة ومعلومات محدثة.',purposeOne:'توحيد رحلة التدريب ومعلوماتها',purposeTwo:'توضيح الأدوار ومتطلبات كل مرحلة',purposeThree:'دعم التوجيه والتغذية الراجعة',rolesKicker:'أطراف الرحلة',rolesTitle:'كل دور يعرف خطوته التالية',rolesIntro:'تربط المنظومة أربعة أدوار رئيسية ضمن تجربة تدريب واحدة.',studentTitle:'الطالب المتدرب',studentText:'يستكشف الفرص، ويتابع المهام والساعات، ويطّلع على التوجيه والتقييمات خلال التدريب.',companyTitle:'مشرف جهة التدريب',companyText:'يستقبل المتدرب، ويتابع حضوره ومهامه، ويقدم التقييم الميداني.',academicTitle:'المشرف الأكاديمي',academicText:'يرشد الطالب أكاديميًا، ويتابع تقدمه وتقاريره، ويرصد التقييم الأكاديمي.',coordinatorTitle:'منسق التدريب',coordinatorText:'ينظم بيانات التدريب والتوزيع والمتابعة، ويراجع إجراءات الإكمال.',evaluationKicker:'آلية التقييم',evaluationTitle:'مكونات التقييم',evaluationText:'تتبع آلية التقييم النماذج المعتمدة من القسم، وتشمل تقارير المشرف والجهة وتقرير الطالب والعرض.',companyWeight:'تقرير مشرف التدريب',academicWeight:'تقرير جهة التدريب',reportWeight:'التقرير التفصيلي والعرض',ctaKicker:'هل تحتاج إلى مساعدة؟',ctaTitle:'نحن هنا للإجابة عن استفساراتك',ctaButton:'انتقل إلى صفحة التواصل'},
                en:{languageLabel:'العربية',kicker:'Yarmouk University · Faculty of Information Technology and Computer Sciences',heroTitle:'One platform for a clearer internship experience',heroText:'The University Internship Management System (UIMS) organizes the internship journey and brings students, training partners, academic supervisors, and the faculty coordinator together in one digital space.',loginCta:'Sign in to UIMS',contactCta:'Contact us',university:'Yarmouk University',college:'Faculty of Information Technology and Computer Sciences',systemLabel:'University Internship Management System',facts:'Internship facts',hoursFact:'weeks of field training',creditsFact:'academic credit hours',rolesFact:'roles working together',storyKicker:'About the system',storyTitle:'From academic knowledge to practical experience',storyP1:'Field training gives students the opportunity to apply what they have learned in a real work environment and understand professional expectations before graduation.',storyP2:'UIMS supports this experience by organizing internship information, tracking tasks, hours, and evaluations, and helping all parties stay connected throughout the placement.',principle:'A clear path for students, with coordinated follow-up by the university and training partner.',purposeKicker:'Our purpose',purposeTitle:'A more organized and transparent internship experience',purposeText:'Help each participant follow commitments, progress, and evaluations through clear steps and up-to-date information.',purposeOne:'Bring the internship journey and its information together',purposeTwo:'Clarify each role and stage requirements',purposeThree:'Support guidance and constructive feedback',rolesKicker:'People in the journey',rolesTitle:'Every role knows what comes next',rolesIntro:'The system connects four key roles throughout one internship experience.',studentTitle:'Student trainee',studentText:'Explores opportunities, tracks tasks and hours, and follows guidance and evaluations during the placement.',companyTitle:'Training partner supervisor',companyText:'Welcomes the trainee, follows attendance and tasks, and provides the workplace evaluation.',academicTitle:'Academic supervisor',academicText:'Guides the student, follows progress and reports, and records the academic evaluation.',coordinatorTitle:'Internship coordinator',coordinatorText:'Organizes internship records, assignments, and follow-up, and reviews completion steps.',evaluationKicker:'Evaluation framework',evaluationTitle:'Evaluation components',evaluationText:'Evaluation follows department-approved forms and includes the supervisor and institution reports, plus the student’s detailed report and presentation.',companyWeight:'Training supervisor report',academicWeight:'Training institution report',reportWeight:'Detailed report and presentation',ctaKicker:'Need assistance?',ctaTitle:'We are here to answer your questions',ctaButton:'Go to contact page'}
            };
            function setLanguage(){var lang='en';page.lang=lang;page.dir='ltr';page.dataset.lang=lang;document.documentElement.lang=lang;page.querySelectorAll('[data-about-i18n]').forEach(function(el){var key=el.getAttribute('data-about-i18n');if(words.en[key])el.textContent=words.en[key];});document.querySelectorAll('[data-home-shell-i18n]').forEach(function(el){var key=el.getAttribute('data-home-shell-i18n');if(shellWords.en[key])el.textContent=shellWords.en[key];});var dateNode=document.getElementById('currentDateSpan');if(dateNode){dateNode.dir='ltr';dateNode.textContent=new Intl.DateTimeFormat('en-GB',{day:'2-digit',month:'long',year:'numeric'}).format(new Date());}page.querySelectorAll('[data-about-aria]').forEach(function(el){var key=el.getAttribute('data-about-aria');if(words.en[key])el.setAttribute('aria-label',words.en[key]);});document.title='About UIMS | Yarmouk University';}
            setLanguage();
        }());

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
