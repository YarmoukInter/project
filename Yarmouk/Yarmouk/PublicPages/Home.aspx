<%@ Page Title="Home" Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="Yarmouk.PublicPages.Home" %>
<!doctype html>
<html lang="en">
<head runat="server">
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="theme-color" content="#ffffff">
  <title>Yarmouk University — Your practical training journey</title>
  <meta name="description" content="Explore practical training at Yarmouk University. Discover an opportunity, confirm your placement, develop your skills and complete your internship.">
  <link rel="icon" runat="server" href="~/assets/yarmouk-mark.svg" type="image/svg+xml">
  <link rel="preconnect" href="https://fonts.googleapis.com">
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous">
  <link href="https://fonts.googleapis.com/css2?family=DM+Sans:opsz,wght@9..40,400;9..40,500;9..40,600;9..40,700&display=swap" rel="stylesheet">
  <link rel="stylesheet" runat="server" href="~/Content/PublicPages/Home.css?v=emblem-reveal-21" />
  <script src='<%= ResolveUrl("~/Scripts/PublicPages/Home.js") %>?v=emblem-reveal-21' defer></script>
</head>
<body data-scene="discover" data-home-root='<%= ResolveUrl("~/") %>'>
  <form id="form1" runat="server">
  <a class="skip-link" href="#chapter-discover">Skip to main content</a>
  <header class="yu-public-navbar" data-public-navbar="true">
    <div class="yu-nav-body">
      <a class="yu-nav-brand" href="Home.aspx" data-replay="true" aria-label="Yarmouk University home"><img src='<%= ResolveUrl("~/assets/yarmouk-logo-on-light.svg") %>' width="140" height="51" alt="Yarmouk University"></a>
      <button class="yu-nav-toggle" type="button" aria-expanded="false" aria-controls="yu-public-links" aria-label="Open navigation" hidden><span></span><span></span></button>
      <nav class="yu-nav-links" id="yu-public-links" aria-label="Main navigation">
        <a href='<%= ResolveUrl("~/PublicPages/Home.aspx") %>' aria-current="page" data-replay="true">Home</a><a href='<%= ResolveUrl("~/PublicPages/AboutUs.aspx") %>'  >About Us</a><a href='<%= ResolveUrl("~/PublicPages/ContactUs.aspx") %>'  >Contact Us</a>

      </nav>
      <a class="yu-nav-login" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>' >Log in <span aria-hidden="true">↗</span></a>
    </div>
  </header>

  <div class="scene-viewport" aria-hidden="true">
    <canvas id="experience-canvas"></canvas>
    <div class="static-fallback" id="static-fallback">
      <svg class="arch-svg" viewBox="0 0 500 550" fill="none" aria-hidden="true"><path d="M250 26C231 70 176 76 122 111C77 140 57 191 57 247V425C57 463 48 485 28 501M472 501C452 485 443 463 443 425V247C443 191 423 140 378 111C324 76 269 70 250 26" stroke="currentColor" stroke-width="9"/><path d="M250 46C226 81 181 88 128 122C87 149 71 195 71 248V425C71 455 64 480 53 490M447 490C436 480 429 455 429 425V248C429 195 413 149 372 122C319 88 274 81 250 46" stroke="currentColor" stroke-width="3"/></svg>
    </div>
  </div>

  <main id="main"><div id="experience-runway" class="experience-runway" aria-label="Practical training journey">
    <div class="sticky-stage">
      <div class="scene-caption"><span>Yarmouk University</span><span>Practical training · Irbid</span></div>

      <div class="static-introduction">
        <svg viewBox="0 0 500 550" role="img" aria-label="Completed Yarmouk University emblem">
          <path class="crest-field" stroke-width="10" d="M250 26C231 70 176 76 122 111C77 140 57 191 57 247V425C57 463 48 485 28 501H472C452 485 443 463 443 425V247C443 191 423 140 378 111C324 76 269 70 250 26Z"/>
          <path stroke-width="4" d="M250 46C226 81 181 88 128 122C87 149 71 195 71 248V425C71 455 64 480 53 490M447 490C436 480 429 455 429 425V248C429 195 413 149 372 122C319 88 274 81 250 46"/>
          <path class="crest-motif" fill-rule="evenodd" d="M185 106L228 230H235L240 198H261L267 230H274L315 106L266 365L250 478L234 365ZM214 254L250 448L286 254Z"/>
          <g class="crest-orbits" stroke-width="4.5"><ellipse cx="250" cy="337" rx="58" ry="109" transform="rotate(-43 250 337)"/><ellipse cx="250" cy="337" rx="58" ry="109" transform="rotate(43 250 337)"/></g>
          <g class="crest-wheat"><path d="M250 430V303" stroke-width="2.2"/><path d="M250 313C240 300 247 285 250 279C256 289 258 302 250 313ZM249 336C235 332 232 319 234 310C244 313 250 325 249 336ZM251 336C265 332 268 319 266 310C256 313 250 325 251 336ZM249 368C232 365 230 350 232 340C244 345 251 357 249 368ZM251 368C268 365 270 350 268 340C256 345 249 357 251 368ZM249 399C232 397 231 383 233 374C245 378 250 388 249 399ZM251 399C268 397 269 383 267 374C255 378 250 388 251 399Z"/></g>
        </svg>
        <p>Yarmouk University <span lang="ar" dir="rtl">جامعة اليرموك</span></p>
      </div>
      <div class="chapter-container">
        <article class="chapter-card is-active" id="chapter-discover" data-chapter-index="0" aria-labelledby="discover-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M25 18h70v86H25zM39 37h42M39 49h30M39 66h42M39 78h35"/></svg>
          <div class="hero-university"><img src='<%= ResolveUrl("~/assets/yarmouk-mark.svg") %>' width="43" height="50" alt=""><span><span class="hero-university-ar" lang="ar" dir="rtl">جامعة اليرموك</span><span class="hero-university-en">Yarmouk University</span></span></div>
          <h1 class="chapter-title" id="discover-heading" tabindex="-1">Your future.<br>In the making.</h1>
          <p class="chapter-desc">Turn what you know into what you can do.<br class="desktop-break"> Your practical training journey starts at Yarmouk.</p><p class="record-transcript a11y-sr-only">Browse public opportunity summaries. Log in to view full details and apply for a role suited to your major.</p>
          <div class="chapter-actions"><a class="primary-btn" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>Explore internships <span aria-hidden="true">↗</span></a><button type="button" class="text-btn" data-open-dialog="opportunities-dialog">View open opportunities</button></div>
        </article>
        <article class="chapter-card" id="chapter-apply" data-chapter-index="1" aria-labelledby="apply-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M25 18h70v86H25zM39 37h42M39 49h30M39 74l14 13 28-31"/></svg>
          <h2 class="chapter-title" id="apply-heading" tabindex="-1">A place to begin.</h2>
          <p class="chapter-desc">Find a role that fits your major. The company reviews your application; you confirm one accepted offer to make it your placement.</p><p class="record-transcript a11y-sr-only">Applications begin Pending. The company accepts or rejects them. Confirm one accepted offer to begin your placement.</p>
          <p class="scene-detail">Apply. Receive an offer. Make your choice.</p>
          <div class="chapter-actions"><a class="primary-btn" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>Find your opportunity <span aria-hidden="true">↗</span></a></div>
        </article>
        <article class="chapter-card" id="chapter-train" data-chapter-index="2" aria-labelledby="train-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M25 18h70v86H25zM39 36l5 5 8-10m8 6h20M39 57l5 5 8-10m8 6h20M39 81h42m-42 8h24"/></svg>
          <h2 class="chapter-title" id="train-heading" tabindex="-1">Build your ability.</h2>
          <p class="chapter-desc">Take on real tasks, learn from company feedback and track the hours approved by your academic supervisor.</p><p class="record-transcript a11y-sr-only">Company supervisors review your submitted work and give feedback. Your academic supervisor approves training hours independently.</p>
          <p class="scene-detail">Every task. Every revision. Every hour.</p>
          <div class="chapter-actions"><a class="primary-btn" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>Continue your training <span aria-hidden="true">↗</span></a></div>
        </article>
        <article class="chapter-card" id="chapter-complete" data-chapter-index="3" aria-labelledby="complete-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M13 25h94v70H13zM21 33h78v54H21zM36 48h48M29 60h62M47 75h26"/></svg>
          <h2 class="chapter-title" id="complete-heading" tabindex="-1">Your next<br>beginning.</h2>
          <p class="chapter-desc">Complete your training. Receive coordinator approval. Take your certificate into what comes next.</p><p class="record-transcript a11y-sr-only">Completion requires reviewed work and a final report, approved hours, staff evaluations, an ended training period and coordinator approval.</p>
          <div class="chapter-actions"><a class="primary-btn" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>View your progress <span aria-hidden="true">↗</span></a><button type="button" class="text-btn" data-open-dialog="requirements-dialog">Completion requirements</button></div>
        </article>
        <article class="chapter-card" id="chapter-faculty" data-chapter-index="4" aria-labelledby="faculty-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M20 92h80M27 43v44m16-44v44m17-44v44m17-44v44m16-44v44M20 37l40-19 40 19z"/></svg>
          <img class="final-logo" src='<%= ResolveUrl("~/assets/yarmouk-logo-on-dark.svg") %>' alt="Yarmouk University" width="210" height="73"><h2 class="chapter-title" id="faculty-heading" tabindex="-1">Your journey.<br>One Yarmouk.</h2>
          <p class="chapter-desc">Let your next chapter begin at Yarmouk. Explore practical training at the Faculty of Information Technology and Computer Science.</p><p class="record-transcript a11y-sr-only">Practical training connects students, company supervisors, academic supervisors and the university internship coordinator. Explore the faculty’s six majors.</p>
          <div class="chapter-actions"><a class="primary-btn" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>Start your journey <span aria-hidden="true">↗</span></a><button type="button" id="open-majors-modal" class="text-btn" aria-haspopup="dialog">Explore our six majors</button></div>
        </article>
      </div>

      <footer class="stage-footer">
        <div class="journey-intro"><span class="scroll-mark" aria-hidden="true">↓</span><span id="cue-text">Scroll to shape your journey</span></div>
        <nav class="chapter-stepper" aria-label="Internship journey stages">
          <button type="button" class="stepper-btn is-active" data-stage-jump="0" aria-current="step">Discover</button>
          <button type="button" class="stepper-btn" data-stage-jump="1">Apply</button>
          <button type="button" class="stepper-btn" data-stage-jump="2">Train</button>
          <button type="button" class="stepper-btn" data-stage-jump="3">Complete</button>
          <button type="button" class="stepper-btn" data-stage-jump="4">Our faculty</button>
        </nav>
        <nav class="journey-pagination" aria-label="Journey navigation"><button id="journey-previous" type="button" aria-label="Previous journey stage" disabled>←</button><button id="journey-next" type="button" aria-label="Next journey stage">→</button></nav>
        <div class="counter-track" id="progress-bar-el" role="progressbar" aria-label="Journey progress" aria-valuemin="0" aria-valuemax="100" aria-valuenow="0"><div id="progress-fill" class="counter-fill"></div></div>
      </footer>
    </div>
  </div>
  <!-- Public homepage sections follow the completed gate. -->
  <div class="home-content" id="home-content">
    <nav class="home-section-nav" aria-label="Explore this page"><span>Practical training</span><a href="#opportunities">Opportunities</a><a href="#fields">Our majors</a><a href="#how-to-apply">How to apply</a><a href="#faq">Questions &amp; answers</a></nav>
    <div class="stone-story" id="stone-story" data-stone-story>
      <aside class="stone-book-rail" aria-label="The stone book">
        <div class="stone-book-stage">
          <canvas id="stone-book-canvas" aria-hidden="true"></canvas>
          <svg class="stone-book-fallback" viewBox="0 0 500 400" aria-hidden="true">
            <g transform="translate(-30 0) scale(1.12 1)">
            <ellipse cx="252" cy="313" rx="208" ry="31" fill="#203125" opacity=".13"/>
            <path d="M31 263 246 345 470 267 449 239 247 299 49 239Z" fill="#4a5546"/>
            <path d="M45 248C117 218 184 233 246 283 304 224 374 218 449 237L451 268C372 250 304 264 246 324 182 274 114 260 45 288Z" fill="#868778"/>
            <path d="M45 237C117 207 184 222 246 272 304 213 374 207 449 226L450 250C372 232 304 246 246 306 182 256 114 242 45 270Z" fill="#b3b2a2"/>
            <path d="M47 225C119 195 184 210 246 260 304 201 374 195 446 214L448 235C372 217 304 231 246 291 182 241 114 227 47 255Z" fill="#929687"/>
            <path d="M48 130C110 97 182 82 246 134L246 269C182 220 114 207 48 238Z" fill="#bcbcae"/>
            <path d="M246 134C285 58 363 75 441 104L446 218C365 195 299 211 246 269Z" fill="#a3a797"/>
            <path d="M246 135V271M47 263C112 236 180 248 246 299 304 240 373 225 449 249" fill="none" stroke="#575f50" stroke-width="4"/>
            <path d="m99 135 24 9 13 22-10 22m204-69 22 20-8 24 25 21m-230 28 22-20 14 5" fill="none" stroke="#747c69" stroke-width="2" opacity=".4"/>
            <g fill="none" stroke="#50604a" stroke-width="1.4" opacity=".55">
              <path d="M85 154q6-5 12 0t12 0t12 0t12 0t12 0t12 0M88 175q6-5 12 0t12 0t12 0t12 0t12 0t12 0M91 196q6-5 12 0t12 0t12 0t12 0"/>
              <path d="M292 135q6-5 12 0t12 0t12 0t12 0t12 0t12 0M290 156q6-5 12 0t12 0t12 0t12 0t12 0t12 0M288 177q6-5 12 0t12 0t12 0t12 0"/>
            </g>
            </g>
          </svg>
          <div class="stone-book-controls"><span>Your next chapter.</span><button type="button" id="book-motion" aria-pressed="false" hidden>Pause page turns</button></div>
        </div>
      </aside>
      <div class="stone-story-chapters">
    <section class="home-section about-section" id="about-training" aria-labelledby="about-training-title">
      <div class="about-grid"><h2 id="about-training-title">Knowledge<br>into practice.</h2><div class="about-copy"><p class="section-lead">Your first professional experience starts with the skills you’re building today.</p><p>Practical training brings students, companies and academic supervisors together. Find a role, take on meaningful work and build a record of your progress—with guidance along the way.</p><a class="section-link" href='<%= ResolveUrl("~/PublicPages/AboutUs.aspx") %>'>Get to know the programme <span aria-hidden="true">↗</span></a></div></div>
      <div class="about-footnote"><span>Faculty of Information Technology and Computer Science</span><a href="#how-to-apply">See how it works <span aria-hidden="true">↓</span></a></div>
    </section>

    <section class="home-section opportunities-section" id="opportunities" aria-labelledby="opportunities-heading">
      <div class="section-heading"><div><h2 id="opportunities-heading">Find your<br> first opening.</h2></div><div class="section-heading-aside"><p>Explore a few of the roles available to your faculty. Full details and applications are available after login.</p><a class="section-link" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>Explore opportunities <span aria-hidden="true">↗</span></a></div></div>
      <div class="opportunity-filters"><div class="opportunity-search"><label for="public-search">Search opportunities</label><input id="public-search" type="search" placeholder="Role, company or location" autocomplete="off" aria-controls="home-opportunities"></div><div class="opportunity-major-filter"><label for="public-major">Your field</label><select id="public-major"><option value="">All six majors</option><option value="cs">Computer Science</option><option value="cis">Computer Information Systems</option><option value="bit">Business Information Technology</option><option value="cys">Cybersecurity</option><option value="dai">Data Science and Artificial Intelligence</option><option value="drg">Digital Reality and Game Development</option></select></div><span id="public-opportunity-count" role="status" aria-live="polite"></span></div>
      <div class="opportunity-carousel-controls"><p id="opportunity-carousel-hint">Browse at your pace. Hover or focus on a card to pause.</p><div><button type="button" id="opportunity-previous" aria-label="Previous opportunities">←</button><button type="button" id="opportunity-motion" aria-pressed="false">Pause motion</button><button type="button" id="opportunity-next" aria-label="Next opportunities">→</button></div></div><div id="home-opportunities" class="opportunity-carousel" role="region" aria-label="Internship opportunity cards" aria-describedby="opportunity-carousel-hint" tabindex="0" aria-busy="true"><p>Loading opportunity summaries…</p></div>
    </section>

    <section class="home-section fields-section" id="fields" aria-labelledby="fields-heading">
      <div class="section-heading"><div><h2 id="fields-heading">Find your field.</h2></div><p class="section-heading-aside">Different ways of thinking. A common purpose: putting technology to work in the world.</p></div>
      <div class="fields-grid">
        <article><span class="field-code">CS</span><h3>Computer Science</h3><p>Build software, solve problems and turn ideas into working systems.</p><a href="#opportunities" data-major="cs" aria-label="Explore opportunities for Computer Science">Explore this field <span aria-hidden="true">↗</span></a></article>
        <article><span class="field-code">CIS</span><h3>Computer Information Systems</h3><p>Connect people, information and the systems organisations depend on.</p><a href="#opportunities" data-major="cis" aria-label="Explore opportunities for Computer Information Systems">Explore this field <span aria-hidden="true">↗</span></a></article>
        <article><span class="field-code">BIT</span><h3>Business Information Technology</h3><p>Bring technical skills and business thinking to real organisational challenges.</p><a href="#opportunities" data-major="bit" aria-label="Explore opportunities for Business Information Technology">Explore this field <span aria-hidden="true">↗</span></a></article>
        <article><span class="field-code">CYS</span><h3>Cybersecurity</h3><p>Help protect information, investigate risks and strengthen digital systems.</p><a href="#opportunities" data-major="cys" aria-label="Explore opportunities for Cybersecurity">Explore this field <span aria-hidden="true">↗</span></a></article>
        <article><span class="field-code">DAI</span><h3>Data Science and Artificial Intelligence</h3><p>Explore data, develop models and turn evidence into useful decisions.</p><a href="#opportunities" data-major="dai" aria-label="Explore opportunities for Data Science and Artificial Intelligence">Explore this field <span aria-hidden="true">↗</span></a></article>
        <article><span class="field-code">DRG</span><h3>Digital Reality and Game Development</h3><p>Create interactive experiences through immersive technology and game design.</p><a href="#opportunities" data-major="drg" aria-label="Explore opportunities for Digital Reality and Game Development">Explore this field <span aria-hidden="true">↗</span></a></article>
      </div>
    </section>

    <section class="home-section application-section" id="how-to-apply" aria-labelledby="apply-guide-heading">
      <div class="section-heading"><div><h2 id="apply-guide-heading">Make your<br>first move.</h2></div><p class="section-heading-aside">From finding a role to confirming your placement. Follow the guide at your own pace.</p></div>
      <div class="application-grid"><div class="application-media" id="application-media"><video id="application-video" controls playsinline preload="none" poster='<%= ResolveUrl("~/assets/how-to-apply-poster.png") %>' aria-describedby="video-guide-caption"><source src='<%= ResolveUrl("~/assets/how-to-apply.webm") %>' type="video/webm"><track kind="captions" src='data:text/vtt;charset=utf-8,WEBVTT%0D%0A%0D%0A00%3A00.000%20--%3E%2000%3A06.000%0D%0A1.%20Log%20in.%20The%20university%20internship%20coordinator%20creates%20your%20student%20account%20and%20assigns%20your%20academic%20group.%0D%0A%0D%0A00%3A06.000%20--%3E%2000%3A12.000%0D%0A2.%20Explore%20opportunities.%20Review%20the%20requirements%20and%20submit%20your%20application.%0D%0A%0D%0A00%3A12.000%20--%3E%2000%3A18.000%0D%0A3.%20The%20company%20supervisor%20reviews%20your%20application%20and%20accepts%20or%20rejects%20it.%0D%0A%0D%0A00%3A18.000%20--%3E%2000%3A24.000%0D%0A4.%20Confirm%20one%20accepted%20offer%20to%20create%20your%20placement.%20Other%20outstanding%20applications%20become%20Not%20selected.%0D%0A' srclang="en" label="English" default>Your browser cannot play this video. Follow the four written steps beside it.</video><p class="video-caption" id="video-guide-caption">A short application walkthrough. No sound required. Captions and a written guide are included.</p></div><ol class="application-steps"><li><span>01</span><div><h3>Log in to your student account</h3><p>Your university internship coordinator creates your account and assigns your academic group.</p></div></li><li><span>02</span><div><h3>Find a role that fits</h3><p>Browse opportunities, check the requirements and submit your application.</p></div></li><li><span>03</span><div><h3>Wait for the company’s decision</h3><p>The company supervisor reviews your application and accepts or rejects it.</p></div></li><li><span>04</span><div><h3>Confirm one accepted offer</h3><p>Your confirmation creates your placement. Other outstanding offers and applications become Not selected.</p></div></li></ol></div>
    </section>

    <section class="home-section faq-section" id="faq" aria-labelledby="faq-heading"><div class="faq-intro"><h2 id="faq-heading">Good questions.<br>Clear answers.</h2><p>Something else on your mind?</p><a class="section-link" href='<%= ResolveUrl("~/PublicPages/ContactUs.aspx") %>'>Contact the university <span aria-hidden="true">↗</span></a></div><div class="faq-list">
      <details name="home-questions"><summary>How do I get a student account?</summary><p>The university internship coordinator creates your account and assigns an academic group with an active supervisor. There is no public sign-up.</p></details>
      <details name="home-questions"><summary>Can I explore opportunities before logging in?</summary><p>Yes. Public summaries show the role, company and location. Log in to view the full requirements and apply.</p></details>
      <details name="home-questions"><summary>Who decides whether my application is accepted?</summary><p>The company supervisor reviews applications and accepts or rejects them. Academic supervisors do not make hiring decisions.</p></details>
      <details name="home-questions"><summary>What happens after I receive an offer?</summary><p>Confirm one accepted offer to create your placement. Other outstanding applications and offers become Not selected.</p></details>
      <details name="home-questions"><summary>How are my tasks and training hours reviewed?</summary><p>Your company supervisor reviews your work and provides feedback or requests revisions. Your academic supervisor approves or returns your recorded training hours independently.</p></details>
      <details name="home-questions"><summary>When can I receive my completion certificate?</summary><p>After your training ends and all completion requirements are met, the university internship coordinator reviews and approves completion. Approval releases your success message and certificate.</p></details>
    </div></section>

      </div>
    </div>
    <footer class="home-footer" id="contact-location">
      <div class="footer-main">
        <div class="footer-identity">
          <a class="footer-brand" href="#chapter-discover" data-replay="true" aria-label="Yarmouk University home"><img src='<%= ResolveUrl("~/assets/yarmouk-logo-on-dark.svg") %>' alt="Yarmouk University" width="150" height="54" loading="lazy"></a>
          <h2>Faculty of Information Technology<br>and Computer Science</h2>
          <p>Yarmouk University · Irbid, Jordan</p>
          <a class="footer-email" href="mailto:it.fac@yu.edu.jo">it.fac@yu.edu.jo <span aria-hidden="true">↗</span></a>
        </div>
        <nav class="footer-explore" aria-label="University links">
          <h3>Explore</h3>
          <a href='<%= ResolveUrl("~/PublicPages/AboutUs.aspx") %>'>About us</a>
          <a href='<%= ResolveUrl("~/PublicPages/ContactUs.aspx") %>'>Contact us</a>
          <a href="https://it.yu.edu.jo/" target="_blank" rel="noopener noreferrer">Faculty website <span aria-hidden="true">↗</span></a>
          <a class="footer-login" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>Start your journey <span aria-hidden="true">↗</span></a>
        </nav>
        <div class="footer-map">
          <div class="footer-map-heading"><h3>Find the faculty</h3><a href="https://www.google.com/maps/search/?api=1&amp;query=Faculty+of+Information+Technology+and+Computer+Science+Yarmouk+University+Irbid" target="_blank" rel="noopener noreferrer">Directions <span aria-hidden="true">↗</span></a></div>
          <iframe title="Google Maps: Faculty of Information Technology and Computer Science, Yarmouk University, Irbid" src="https://maps.google.com/maps?q=Faculty%20of%20Information%20Technology%20and%20Computer%20Science%20Yarmouk%20University%20Irbid&amp;output=embed" width="480" height="176" loading="lazy" referrerpolicy="no-referrer-when-downgrade" allowfullscreen="allowfullscreen"></iframe>
        </div>
      </div>
      <div class="footer-bottom"><span>© <span id="footer-year">2026</span> Yarmouk University</span><nav aria-label="Footer utilities"><a href="#faq">FAQ</a><a href="#chapter-discover" data-replay="true">Back to top <span aria-hidden="true">↑</span></a></nav><span class="footer-powered">Powered by</span></div>
    </footer>

  </div></main>

  <dialog id="opportunities-dialog" class="journey-dialog" aria-labelledby="opportunities-title">
    <div class="dialog-header"><h2 id="opportunities-title">Find your opening.</h2><button type="button" class="dialog-close-btn" data-close-dialog="true" aria-label="Close opportunities">×</button></div>
    <p class="dialog-intro">A glimpse of available training. Log in to view full details and apply.</p>
    <div id="featured-opportunities-list" class="public-opportunities"><p>Loading public opportunity summaries…</p></div>
    <a class="primary-btn" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>Log in to explore <span aria-hidden="true">↗</span></a>
  </dialog>
  <dialog id="requirements-dialog" class="journey-dialog" aria-labelledby="requirements-title">
    <div class="dialog-header"><h2 id="requirements-title">Ready to complete.</h2><button type="button" class="dialog-close-btn" data-close-dialog="true" aria-label="Close requirements">×</button></div>
    <p class="dialog-intro">The university internship coordinator reviews your record before approving completion.</p>
    <ul class="requirements-list"><li>Company and academic final evaluations received.</li><li>Required training hours approved by your academic supervisor.</li><li>Assigned tasks completed and reviewed; final report reviewed.</li><li>A passing calculated result and the end of the training period.</li></ul>
    <p class="dialog-note">Approval releases your successful-completion message and certificate. Final evaluation forms and results remain private to staff.</p>
    <a class="primary-btn" href='<%= ResolveUrl("~/PublicPages/Login.aspx") %>'>View your training record <span aria-hidden="true">↗</span></a>
  </dialog>
  <dialog id="majors-dialog" class="journey-dialog" aria-labelledby="dialog-title">
    <div class="dialog-header"><h2 id="dialog-title">Six majors. Your path.</h2><button type="button" class="dialog-close-btn" id="close-majors-modal" aria-label="Close academic majors dialog">×</button></div>
    <p class="dialog-intro">Faculty of Information Technology and Computer Science · Yarmouk University, Irbid.</p>
    <ul class="dialog-majors-list"><li>Computer Science</li><li>Computer Information Systems</li><li>Business Information Technology</li><li>Cybersecurity</li><li>Data Science and Artificial Intelligence</li><li>Digital Reality and Game Development</li></ul>
    <a class="text-btn" href="https://it.yu.edu.jo" target="_blank" rel="noopener noreferrer">Visit the faculty website <span aria-hidden="true">↗</span></a>
  </dialog>
  <p id="a11y-announcer" class="a11y-sr-only" aria-live="polite" aria-atomic="true"></p>
  <noscript><p class="noscript-banner">Use the links below to explore your training journey. The interactive scene requires JavaScript.</p></noscript>
  </form>
</body>
</html>

<!-- H -->
