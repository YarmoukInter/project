<%@ Page Title="AboutUs" Language="C#" MasterPageFile="~/PublicPages/Section.Master" AutoEventWireup="true" CodeBehind="AboutUs.aspx.cs" Inherits="Yarmouk.PublicPages.AboutUs" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<!-- About Us design: university sculpture and animated campus experience. -->
<link href="<%= ResolveUrl("~/Content/PublicPages/AboutUs.css?v=sculpture-20261022") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/PublicPages/AboutUs.js?v=sculpture-20261022") %>" defer></script>
    <link rel="preconnect" href="https://fonts.googleapis.com" /><link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700;800;900&family=Inter:wght@400;500;600;700;800&family=Tajawal:wght@400;500;700;800&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" crossorigin="anonymous" />
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">

    <div class="about-page" id="aboutPage" lang="en" dir="ltr" data-lang="en">
        <section class="yu-intro" aria-labelledby="aboutTitle">
<div class="yu-intro-copy"><p class="yu-university">Yarmouk University <span>Irbid, Jordan</span></p>
<h1 id="aboutTitle">Knowledge.<br>Experience.<br>Possibility.</h1>
<p class="yu-intro-lead">A Yarmouk education goes beyond the classroom. We bring students, faculty and training partners together to turn learning into experience.</p>
<div class="about-hero-actions yu-intro-actions"><a class="yu-primary" href="#campusTour">Explore our campus <span aria-hidden="true">&#8599;</span></a><a class="yu-signin" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>Sign in to UIMS <span aria-hidden="true">&#8594;</span></a></div></div>
<div class="yu-art" id="yuArt"><span class="yu-watermark" aria-hidden="true">YU</span><div class="yu-art-stage"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 540 550" class="yu-sculpture" fill="none" role="img" aria-labelledby="yuCrestTitle yuCrestDesc">
  <title id="yuCrestTitle">Yarmouk University emblem illustration</title>
  <desc id="yuCrestDesc">A pointed frame based on the supplied university emblem, surrounding its black motif, red orbital lines and green wheat sprig. A web illustration, not a depiction of a historic gateway.</desc>
  <defs>
    <linearGradient id="rim" x1="80" y1="60" x2="425" y2="500" gradientUnits="userSpaceOnUse"><stop stop-color="#faf7ed"/><stop offset=".55" stop-color="#e9e2d1"/><stop offset="1" stop-color="#b9ad92"/></linearGradient>
    <linearGradient id="paper" x1="140" y1="80" x2="365" y2="475" gradientUnits="userSpaceOnUse"><stop stop-color="#fffefa"/><stop offset="1" stop-color="#f0ecdf"/></linearGradient>
    <path id="outline" d="M250 26C231 70 176 76 122 111C77 140 57 191 57 247V425C57 463 48 485 28 501H472C452 485 443 463 443 425V247C443 191 423 140 378 111C324 76 269 70 250 26Z"/>

  </defs>
  <g class="yu-crest-depth"><use href="#outline" transform="translate(24 18)" fill="#335c49"/><use href="#outline" transform="translate(18 13)" fill="#70917b"/><use href="#outline" transform="translate(10 7)" fill="#acbba4"/></g><g class="rim-fill">
    <use href="#outline" transform="translate(0 6)" fill="#001c0e" opacity=".45"/>
    <use href="#outline" fill="url(#rim)"/>
    <path d="M250 46C226 81 181 88 128 122C87 149 71 195 71 248V425C71 455 64 480 53 490H447C436 480 429 455 429 425V248C429 195 413 149 372 122C319 88 274 81 250 46Z" fill="url(#paper)" stroke="#c5a059" stroke-width="1.5"/>
  </g>
  <path class="rim-line" d="M250 26C231 70 176 76 122 111C77 140 57 191 57 247V425C57 463 48 485 28 501H472C452 485 443 463 443 425V247C443 191 423 140 378 111C324 76 269 70 250 26Z" stroke="url(#rim)" stroke-width="7" stroke-linejoin="round"/>
  <g class="motif">
    <path d="M185 106L228 230H235L240 198H261L267 230H274L315 106L266 365L250 478L234 365Z" fill="#101914"/>
    <path d="M214 254L250 448L286 254Z" fill="url(#paper)"/>
    <path d="M214 254L250 448L286 254" stroke="#101914" stroke-width="1.4"/>
  </g>
  <g class="wheat" fill="#4a9b45">
    <path d="M250 430V303" stroke="#4a9b45" stroke-width="2.2" stroke-linecap="round"/>
    <path d="M250 313C240 300 247 285 250 279C256 289 258 302 250 313Z"/>
    <path d="M249 336C235 332 232 319 234 310C244 313 250 325 249 336ZM251 336C265 332 268 319 266 310C256 313 250 325 251 336Z"/>
    <path d="M249 368C232 365 230 350 232 340C244 345 251 357 249 368ZM251 368C268 365 270 350 268 340C256 345 249 357 251 368Z"/>
    <path d="M249 399C232 397 231 383 233 374C245 378 250 388 249 399ZM251 399C268 397 269 383 267 374C255 378 250 388 251 399Z"/>
  </g>
  <g class="orbits" stroke="#df393e" stroke-width="4.5">
    <ellipse cx="250" cy="337" rx="58" ry="109" transform="rotate(-43 250 337)"/>
    <ellipse cx="250" cy="337" rx="58" ry="109" transform="rotate(43 250 337)"/>
  </g>

</svg>
</div><div class="yu-plinth" aria-hidden="true"></div><p class="yu-art-caption">Rooted in Yarmouk.<br><span>Built for what comes next.</span></p></div>
<div class="yu-intro-foot"><span>Faculty of Information Technology<br>and Computer Sciences</span><a href="#campusTour">Take a closer look <span aria-hidden="true">&#8595;</span></a></div></section>

        <section class="about-facts" aria-label="Training facts" data-about-aria="facts">
            <article class="about-fact about-reveal"><span class="about-fact-icon"><i class="fa-regular fa-clock" aria-hidden="true"></i></span><div><strong class="about-counter" data-count="8">8</strong><span data-about-i18n="hoursFact">weeks of field training</span></div></article>
            <article class="about-fact about-reveal"><span class="about-fact-icon about-fact-gold"><i class="fa-solid fa-graduation-cap" aria-hidden="true"></i></span><div><strong class="about-counter" data-count="3">3</strong><span data-about-i18n="creditsFact">academic credit hours</span></div></article>
            <article class="about-fact about-reveal"><span class="about-fact-icon about-fact-blue"><i class="fa-solid fa-people-group" aria-hidden="true"></i></span><div><strong class="about-counter" data-count="4">4</strong><span data-about-i18n="rolesFact">roles working together</span></div></article>
        </section>

        <section class="about-video" aria-labelledby="aboutVideoTitle">
            <div class="about-video-heading">
                <div>
                    <span class="about-section-kicker">UIMS · Yarmouk University</span>
                    <h2 id="aboutVideoTitle">A closer look at the training journey</h2>
                </div>
                <p>Watch the video introduction to the university and its field training experience.</p>
            </div>
            <div class="about-video-frame">
                <video controls preload="metadata" playsinline aria-label="UIMS and field training video introduction" aria-describedby="aboutVideoTitle">
                    <source src="<%= ResolveUrl("~/Content/Videos/uims-about-introduction.mp4") %>" type="video/mp4" />
                    Your browser does not support embedded video.
                </video>
            </div>
        </section>

        <section class="about-campus" id="campusTour" aria-labelledby="campusTitle">
            <div class="campus-heading"><div><span class="campus-label">Yarmouk University / 360°</span><h2 id="campusTitle"><span class="campus-en">Your first step starts here.</span></h2></div><p><span class="campus-en">Step inside the Faculty of Information Technology and Computer Science with an interactive virtual walkthrough.</span></p></div>
            <div class="campus-viewer" id="campusViewer">
                <div class="campus-cover"><div class="campus-orbits" aria-hidden="true"><i></i><i></i><i></i><strong>360°</strong></div><button type="button" id="startCampusTour"><span aria-hidden="true">↗</span><span class="campus-en">Enter the virtual tour</span></button><p><span class="campus-en">Look around. Follow the hotspots. Explore at your own pace.</span></p></div>
            </div>
            <div class="campus-toolbar"><p id="campusStatus" role="status"><span class="campus-en">Hosted on the faculty’s official website.</span></p><a href="https://it.yu.edu.jo/vt/" target="_blank" rel="noopener"><span class="campus-en">Open tour in a new tab ↗</span></a></div>
        </section>
        <section class="about-journey" aria-labelledby="journeyTitle"><h2 id="journeyTitle"><span class="campus-en">From the classroom to what comes next.</span></h2><div class="journey-track">
        <article><span>01</span><h3><span class="campus-en">Discover</span></h3><p><span class="campus-en">Find training opportunities that connect with your studies.</span></p></article>
        <article><span>02</span><h3><span class="campus-en">Experience</span></h3><p><span class="campus-en">Put knowledge into practice and keep track of your work.</span></p></article>
        <article><span>03</span><h3><span class="campus-en">Grow</span></h3><p><span class="campus-en">Use guidance and feedback to shape your next step.</span></p></article>
        </div></section>

        <section class="about-story about-bento-grid about-reveal" aria-labelledby="storyTitle">
            <div class="about-story-copy"><span class="about-section-kicker" data-about-i18n="storyKicker">About the system</span><h2 id="storyTitle" data-about-i18n="storyTitle">From academic knowledge to practical experience</h2><p data-about-i18n="storyP1">Field training gives students the opportunity to apply what they have learned in a real work environment and understand professional expectations before graduation.</p><p data-about-i18n="storyP2">UIMS supports this experience by organizing internship information, tracking tasks, hours, and evaluations, and helping all parties stay connected throughout the placement.</p><div class="about-principle"><i class="fa-solid fa-compass" aria-hidden="true"></i><span data-about-i18n="principle">A clear path for students, with coordinated follow-up by the university and training partner.</span></div></div>
            <aside class="about-purpose" data-tilt aria-labelledby="purposeTitle"><div class="about-purpose-icon"><i class="fa-solid fa-bullseye" aria-hidden="true"></i></div><span class="about-section-kicker" data-about-i18n="purposeKicker">Our purpose</span><h2 id="purposeTitle" data-about-i18n="purposeTitle">A more organized and transparent internship experience</h2><p data-about-i18n="purposeText">Help each participant follow commitments, progress, and evaluations through clear steps and up-to-date information.</p><ul><li><i class="fa-solid fa-check" aria-hidden="true"></i><span data-about-i18n="purposeOne">Bring the internship journey and its information together</span></li><li><i class="fa-solid fa-check" aria-hidden="true"></i><span data-about-i18n="purposeTwo">Clarify each role and stage requirements</span></li><li><i class="fa-solid fa-check" aria-hidden="true"></i><span data-about-i18n="purposeThree">Support guidance and constructive feedback</span></li></ul></aside>
        </section>

        <section class="about-roles" aria-labelledby="rolesTitle"><div class="about-section-heading about-reveal"><div><span class="about-section-kicker" data-about-i18n="rolesKicker">People in the journey</span><h2 id="rolesTitle" data-about-i18n="rolesTitle">Every role knows what comes next</h2></div><p data-about-i18n="rolesIntro">The system connects four key roles throughout one internship experience.</p></div>
            <div class="about-role-grid about-timeline">
                <article class="about-role-card about-reveal" data-tilt><span class="about-role-number">01</span><span class="about-role-icon role-green"><i class="fa-solid fa-user-graduate" aria-hidden="true"></i></span><h3 data-about-i18n="studentTitle">Student trainee</h3><p data-about-i18n="studentText">Explores opportunities, tracks tasks and hours, and follows guidance and evaluations during the placement.</p></article>
                <article class="about-role-card about-reveal" data-tilt><span class="about-role-number">02</span><span class="about-role-icon role-gold"><i class="fa-solid fa-building" aria-hidden="true"></i></span><h3 data-about-i18n="companyTitle">Training partner supervisor</h3><p data-about-i18n="companyText">Welcomes the trainee, follows attendance and tasks, and provides the workplace evaluation.</p></article>
                <article class="about-role-card about-reveal" data-tilt><span class="about-role-number">03</span><span class="about-role-icon role-blue"><i class="fa-solid fa-chalkboard-user" aria-hidden="true"></i></span><h3 data-about-i18n="academicTitle">Academic supervisor</h3><p data-about-i18n="academicText">Guides the student, follows progress and reports, and records the academic evaluation.</p></article>
                <article class="about-role-card about-reveal" data-tilt><span class="about-role-number">04</span><span class="about-role-icon role-purple"><i class="fa-solid fa-sitemap" aria-hidden="true"></i></span><h3 data-about-i18n="coordinatorTitle">Internship coordinator</h3><p data-about-i18n="coordinatorText">Organizes internship records, assignments, and follow-up, and reviews completion steps.</p></article>
            </div>
        </section>

        <section class="about-evaluation about-reveal" aria-labelledby="evaluationTitle"><div class="about-evaluation-heading"><span class="about-evaluation-icon"><i class="fa-solid fa-scale-balanced" aria-hidden="true"></i></span><div><span class="about-section-kicker" data-about-i18n="evaluationKicker">Evaluation framework</span><h2 id="evaluationTitle" data-about-i18n="evaluationTitle">Evaluation components</h2><p data-about-i18n="evaluationText">Evaluation follows department-approved forms and includes the supervisor and institution reports, plus the student’s detailed report and presentation.</p></div></div><div class="about-weight-list"><div class="about-weight"><div class="about-weight-head"><span data-about-i18n="companyWeight">Training supervisor report</span></div></div><div class="about-weight"><div class="about-weight-head"><span data-about-i18n="academicWeight">Training institution report</span></div></div><div class="about-weight"><div class="about-weight-head"><span data-about-i18n="reportWeight">Detailed report and presentation</span></div></div></div></section>

        <section class="about-bottom-cta about-reveal"><div><span class="about-section-kicker" data-about-i18n="ctaKicker">Need assistance?</span><h2 data-about-i18n="ctaTitle">We are here to answer your questions</h2></div><a class="about-button about-button-green about-magnetic" href='<%= ResolveUrl("~/PublicPages/ContactUs.aspx") %>'><span data-about-i18n="ctaButton">Go to contact page</span><i class="fa-solid fa-arrow-left" aria-hidden="true"></i></a></section>
    </div>
</asp:Content>
