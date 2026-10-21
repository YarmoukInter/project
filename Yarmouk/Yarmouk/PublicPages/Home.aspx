<%@ Page Language="C#" %>
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
  <link rel="stylesheet" runat="server" href="~/Content/PublicPages/Home.css" />
  <script src='<%= ResolveUrl("~/Scripts/PublicPages/Home.js") %>' defer></script>
</head>
<body data-scene="discover" data-home-root='<%= ResolveUrl("~/") %>'>
  <form id="form1" runat="server">
  <a class="skip-link" href="#chapter-discover">Skip to main content</a>
  <header class="yu-public-navbar" data-public-navbar="true">
    <div class="yu-nav-body">
      <a class="yu-nav-brand" href="Home.aspx" data-replay="true" aria-label="Yarmouk University home"><img src='<%= ResolveUrl("~/assets/yarmouk-logo-on-light.svg") %>' width="140" height="51" alt="Yarmouk University"></a>
      <button class="yu-nav-toggle" type="button" aria-expanded="false" aria-controls="yu-public-links" aria-label="Open navigation" hidden><span></span><span></span></button>
      <nav class="yu-nav-links" id="yu-public-links" aria-label="Main navigation">
        <a href="Home.aspx" aria-current="page" data-replay="true">Home</a><a href="AboutUs.aspx"  >About Us</a><a href="ContactUs.aspx"  >Contact Us</a>

      </nav>
      <a class="yu-nav-login" href="Login.aspx" >Log in <span aria-hidden="true">↗</span></a>
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

      <div class="static-introduction"><svg viewBox="0 0 500 550" role="img" aria-label="Pointed gate inspired by the supplied university emblem"><path stroke-width="10" d="M250 26C231 70 176 76 122 111C77 140 57 191 57 247V425C57 463 48 485 28 501M472 501C452 485 443 463 443 425V247C443 191 423 140 378 111C324 76 269 70 250 26"/><path stroke-width="4" d="M250 46C226 81 181 88 128 122C87 149 71 195 71 248V425C71 455 64 480 53 490M447 490C436 480 429 455 429 425V248C429 195 413 149 372 122C319 88 274 81 250 46"/></svg><p>Your future. In the making.</p></div>
      <div class="chapter-container">
        <article class="chapter-card is-active" id="chapter-discover" data-chapter-index="0" aria-labelledby="discover-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M25 18h70v86H25zM39 37h42M39 49h30M39 66h42M39 78h35"/></svg>
          <h1 class="chapter-title" id="discover-heading" tabindex="-1">Your future.<br>In the making.</h1>
          <p class="chapter-desc">Turn what you know into what you can do.<br class="desktop-break"> Your practical training journey starts at Yarmouk.</p><p class="record-transcript a11y-sr-only">Browse public opportunity summaries. Log in to view full details and apply for a role suited to your major.</p>
          <div class="chapter-actions"><a class="primary-btn" href="Login.aspx">Explore internships <span aria-hidden="true">↗</span></a><button type="button" class="text-btn" data-open-dialog="opportunities-dialog">View open opportunities</button></div>
        </article>
        <article class="chapter-card" id="chapter-apply" data-chapter-index="1" aria-labelledby="apply-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M25 18h70v86H25zM39 37h42M39 49h30M39 74l14 13 28-31"/></svg>
          <h2 class="chapter-title" id="apply-heading" tabindex="-1">A place to begin.</h2>
          <p class="chapter-desc">Find a role that fits your major. The company reviews your application; you confirm one accepted offer to make it your placement.</p><p class="record-transcript a11y-sr-only">Applications begin Pending. The company accepts or rejects them. Confirm one accepted offer to begin your placement.</p>
          <p class="scene-detail">Apply. Receive an offer. Make your choice.</p>
          <div class="chapter-actions"><a class="primary-btn" href="Login.aspx">Find your opportunity <span aria-hidden="true">↗</span></a></div>
        </article>
        <article class="chapter-card" id="chapter-train" data-chapter-index="2" aria-labelledby="train-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M25 18h70v86H25zM39 36l5 5 8-10m8 6h20M39 57l5 5 8-10m8 6h20M39 81h42m-42 8h24"/></svg>
          <h2 class="chapter-title" id="train-heading" tabindex="-1">Build your ability.</h2>
          <p class="chapter-desc">Take on real tasks, learn from company feedback and track the hours approved by your academic supervisor.</p><p class="record-transcript a11y-sr-only">Company supervisors review your submitted work and give feedback. Your academic supervisor approves training hours independently.</p>
          <p class="scene-detail">Every task. Every revision. Every hour.</p>
          <div class="chapter-actions"><a class="primary-btn" href="Login.aspx">Continue your training <span aria-hidden="true">↗</span></a></div>
        </article>
        <article class="chapter-card" id="chapter-complete" data-chapter-index="3" aria-labelledby="complete-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M13 25h94v70H13zM21 33h78v54H21zM36 48h48M29 60h62M47 75h26"/></svg>
          <h2 class="chapter-title" id="complete-heading" tabindex="-1">Your next<br>beginning.</h2>
          <p class="chapter-desc">Complete your training. Receive coordinator approval. Take your certificate into what comes next.</p><p class="record-transcript a11y-sr-only">Completion requires reviewed work and a final report, approved hours, staff evaluations, an ended training period and coordinator approval.</p>
          <div class="chapter-actions"><a class="primary-btn" href="Login.aspx">View your progress <span aria-hidden="true">↗</span></a><button type="button" class="text-btn" data-open-dialog="requirements-dialog">Completion requirements</button></div>
        </article>
        <article class="chapter-card" id="chapter-faculty" data-chapter-index="4" aria-labelledby="faculty-heading"><svg class="record-illustration" viewBox="0 0 120 120" aria-hidden="true"><path d="M20 92h80M27 43v44m16-44v44m17-44v44m17-44v44m16-44v44M20 37l40-19 40 19z"/></svg>
          <img class="final-logo" src='<%= ResolveUrl("~/assets/yarmouk-logo-on-dark.svg") %>' alt="Yarmouk University" width="210" height="73"><h2 class="chapter-title" id="faculty-heading" tabindex="-1">Your journey.<br>One Yarmouk.</h2>
          <p class="chapter-desc">Let your next chapter begin at Yarmouk. Explore practical training at the Faculty of Information Technology and Computer Science.</p><p class="record-transcript a11y-sr-only">Practical training connects students, company supervisors, academic supervisors and the university internship coordinator. Explore the faculty’s six majors.</p>
          <div class="chapter-actions"><a class="primary-btn" href="Login.aspx">Start your journey <span aria-hidden="true">↗</span></a><button type="button" id="open-majors-modal" class="text-btn" aria-haspopup="dialog">Explore our six majors</button></div>
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
    <section class="home-section about-section" id="about-training" aria-labelledby="about-training-title">
      <div class="section-label"><span>From knowledge to practice</span><span>Yarmouk University · Irbid</span></div>
      <div class="about-grid"><h2 id="about-training-title">What you learn.<br><em>What you become.</em></h2><div class="about-copy"><p class="section-lead">Your first professional experience starts with the skills you’re building today.</p><p>Practical training brings students, companies and academic supervisors together. Find a role, take on meaningful work and build a record of your progress—with guidance along the way.</p><a class="section-link" href="AboutUs.aspx">Get to know the programme <span aria-hidden="true">↗</span></a></div></div>
      <div class="about-footnote"><span>Faculty of Information Technology and Computer Science</span><a href="#how-to-apply">See how it works <span aria-hidden="true">↓</span></a></div>
    </section>

    <section class="home-section opportunities-section" id="opportunities" aria-labelledby="opportunities-heading">
      <div class="section-heading"><div><p class="section-eyebrow">Make your first move</p><h2 id="opportunities-heading">An opening<br>for your ambition.</h2></div><div class="section-heading-aside"><p>Explore a few of the roles available to your faculty. Full details and applications are available after login.</p><a class="section-link" href="Login.aspx">Explore opportunities <span aria-hidden="true">↗</span></a></div></div>
      <div class="opportunity-filters"><div class="opportunity-search"><label for="public-search">Search opportunities</label><input id="public-search" type="search" placeholder="Role, company or location" autocomplete="off" aria-controls="home-opportunities"></div><div class="opportunity-major-filter"><label for="public-major">Your field</label><select id="public-major"><option value="">All six majors</option><option value="cs">Computer Science</option><option value="cis">Computer Information Systems</option><option value="bit">Business Information Technology</option><option value="cys">Cybersecurity</option><option value="dai">Data Science and Artificial Intelligence</option><option value="drg">Digital Reality and Game Development</option></select></div><span id="public-opportunity-count" role="status" aria-live="polite"></span></div>
      <div class="opportunity-carousel-controls"><p id="opportunity-carousel-hint">Browse at your pace. Hover or focus on a card to pause.</p><div><button type="button" id="opportunity-previous" aria-label="Previous opportunities">←</button><button type="button" id="opportunity-motion" aria-pressed="false">Pause motion</button><button type="button" id="opportunity-next" aria-label="Next opportunities">→</button></div></div><div id="home-opportunities" class="opportunity-carousel" role="region" aria-label="Internship opportunity cards" aria-describedby="opportunity-carousel-hint" tabindex="0" aria-busy="true"><p>Loading opportunity summaries…</p></div>
    </section>

    <section class="home-section fields-section" id="fields" aria-labelledby="fields-heading">
      <div class="section-heading"><div><p class="section-eyebrow">Six disciplines. Shared possibility.</p><h2 id="fields-heading">Find your field.</h2></div><p class="section-heading-aside">Different ways of thinking. A common purpose: putting technology to work in the world.</p></div>
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
      <div class="section-heading"><div><p class="section-eyebrow">A clear place to start</p><h2 id="apply-guide-heading">Your application,<br>step by step.</h2></div><p class="section-heading-aside">From finding a role to confirming your placement. Follow the guide at your own pace.</p></div>
      <div class="application-grid"><div class="application-media" id="application-media"><video id="application-video" controls playsinline preload="none" poster='<%= ResolveUrl("~/assets/how-to-apply-poster.png") %>' aria-describedby="video-guide-caption"><source src='<%= ResolveUrl("~/assets/how-to-apply.webm") %>' type="video/webm"><track kind="captions" src='<%= ResolveUrl("~/assets/how-to-apply.vtt") %>' srclang="en" label="English" default>Your browser cannot play this video. Follow the four written steps beside it.</video><p class="video-caption" id="video-guide-caption">A short application walkthrough. No sound required. Captions and a written guide are included.</p></div><ol class="application-steps"><li><span>01</span><div><h3>Log in to your student account</h3><p>Your university internship coordinator creates your account and assigns your academic group.</p></div></li><li><span>02</span><div><h3>Find a role that fits</h3><p>Browse opportunities, check the requirements and submit your application.</p></div></li><li><span>03</span><div><h3>Wait for the company’s decision</h3><p>The company supervisor reviews your application and accepts or rejects it.</p></div></li><li><span>04</span><div><h3>Confirm one accepted offer</h3><p>Your confirmation creates your placement. Other outstanding offers and applications become Not selected.</p></div></li></ol></div>
    </section>

    <section class="home-section faq-section" id="faq" aria-labelledby="faq-heading"><div class="faq-intro"><p class="section-eyebrow">Before you begin</p><h2 id="faq-heading">Good questions.<br>Clear answers.</h2><p>Something else on your mind?</p><a class="section-link" href="ContactUs.aspx">Contact the university <span aria-hidden="true">↗</span></a></div><div class="faq-list">
      <details><summary>How do I get a student account?</summary><p>The university internship coordinator creates your account and assigns an academic group with an active supervisor. There is no public sign-up.</p></details>
      <details><summary>Can I explore opportunities before logging in?</summary><p>Yes. Public summaries show the role, company and location. Log in to view the full requirements and apply.</p></details>
      <details><summary>Who decides whether my application is accepted?</summary><p>The company supervisor reviews applications and accepts or rejects them. Academic supervisors do not make hiring decisions.</p></details>
      <details><summary>What happens after I receive an offer?</summary><p>Confirm one accepted offer to create your placement. Other outstanding applications and offers become Not selected.</p></details>
      <details><summary>How are my tasks and training hours reviewed?</summary><p>Your company supervisor reviews your work and provides feedback or requests revisions. Your academic supervisor approves or returns your recorded training hours independently.</p></details>
      <details><summary>When can I receive my completion certificate?</summary><p>After your training ends and all completion requirements are met, the university internship coordinator reviews and approves completion. Approval releases your success message and certificate.</p></details>
    </div></section>

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
          <a href="AboutUs.aspx">About us</a>
          <a href="ContactUs.aspx">Contact us</a>
          <a href="https://it.yu.edu.jo/" target="_blank" rel="noopener noreferrer">Faculty website <span aria-hidden="true">↗</span></a>
          <a class="footer-login" href="Login.aspx">Start your journey <span aria-hidden="true">↗</span></a>
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
    <a class="primary-btn" href="Login.aspx">Log in to explore <span aria-hidden="true">↗</span></a>
  </dialog>
  <dialog id="requirements-dialog" class="journey-dialog" aria-labelledby="requirements-title">
    <div class="dialog-header"><h2 id="requirements-title">Ready to complete.</h2><button type="button" class="dialog-close-btn" data-close-dialog="true" aria-label="Close requirements">×</button></div>
    <p class="dialog-intro">The university internship coordinator reviews your record before approving completion.</p>
    <ul class="requirements-list"><li>Company and academic final evaluations received.</li><li>Required training hours approved by your academic supervisor.</li><li>Assigned tasks completed and reviewed; final report reviewed.</li><li>A passing calculated result and the end of the training period.</li></ul>
    <p class="dialog-note">Approval releases your successful-completion message and certificate. Final evaluation forms and results remain private to staff.</p>
    <a class="primary-btn" href="Login.aspx">View your training record <span aria-hidden="true">↗</span></a>
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
