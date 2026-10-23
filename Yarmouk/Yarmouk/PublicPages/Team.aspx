<%@ Page Title="Team" Language="C#" MasterPageFile="~/PublicPages/Section.Master" AutoEventWireup="true" CodeBehind="Team.aspx.cs" Inherits="Yarmouk.PublicPages.Team" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <meta name="description" content="Meet the team behind UIMS, the Field Training and Academic Assessment System at Yarmouk University." />
    <meta name="theme-color" content="#0C3026" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=Fraunces:opsz,wght@9..144,500;9..144,600;9..144,700&family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" crossorigin="anonymous" />
    <link href="<%= ResolveUrl("~/Content/PublicPages/Team.css?v=3") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/PublicPages/Team.js?v=4") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <a class="team-skip-link" href="#home">Skip to main content</a>
<section class="hero" id="home" aria-labelledby="hero-title">
            <div class="container hero-inner">
                <canvas class="hero-dataflow" id="heroDataflow" aria-hidden="true"></canvas>
                <h1 class="hero-title" id="hero-title">Field Training and Academic Assessment System <span class="hero-acronym">UIMS</span></h1>

                <div class="hero-lower">
                    <p class="hero-lead">
                        UIMS at Yarmouk University organizes field training procedures and connects <strong>students, training providers, academic supervisors, and training coordinators</strong>. The system is being built with <strong>ASP.NET C#</strong> and <strong>Firebase</strong> services.
                    </p>
                    <div class="hero-actions">
                        <a class="btn btn-primary" href="#team">
                            <span>Meet the project team</span>
                            <i class="fa-solid fa-arrow-down" aria-hidden="true"></i>
                        </a>
                        <a class="btn btn-ghost" href="#architecture">
                            <i class="fa-solid fa-diagram-project" aria-hidden="true"></i>
                            <span>View technical architecture</span>
                        </a>
                    </div>
                </div>

                <figure class="connection" data-loop>
                    <figcaption class="connection-caption" id="connectionCaption">Four roles, one shared system. Select a role to see how it connects.</figcaption>
                    <div class="connection-map" role="group" aria-labelledby="connectionCaption">
                        <svg class="connection-lines" viewBox="0 0 1200 420" preserveAspectRatio="none" aria-hidden="true" focusable="false">
                            <path class="route is-active" data-role-path="student" d="M200 118 C 360 118, 430 210, 600 210"/>
                            <path class="route" data-role-path="provider" d="M1000 118 C 840 118, 770 210, 600 210"/>
                            <path class="route" data-role-path="supervisor" d="M200 302 C 360 302, 430 210, 600 210"/>
                            <path class="route" data-role-path="coordinator" d="M1000 302 C 840 302, 770 210, 600 210"/>
                        </svg>

                        <div class="map-core">
                            <span class="core-ring" aria-hidden="true"></span>
                            <i class="fa-solid fa-cubes-stacked" aria-hidden="true"></i>
                            <strong>UIMS</strong>
                            <span>Yarmouk University</span>
                        </div>

                        <button class="role-node role-student is-active" type="button" data-role="student" data-title="Student" data-description="Training opportunities and task tracking" aria-pressed="true">
                            <span class="role-icon"><i class="fa-solid fa-user-graduate" aria-hidden="true"></i></span>
                            <span class="role-name">Student</span>
                        </button>
                        <button class="role-node role-provider" type="button" data-role="provider" data-title="Training provider" data-description="Training opportunities and field evaluation" aria-pressed="false">
                            <span class="role-icon"><i class="fa-solid fa-building" aria-hidden="true"></i></span>
                            <span class="role-name">Training provider</span>
                        </button>
                        <button class="role-node role-supervisor" type="button" data-role="supervisor" data-title="Academic supervisor" data-description="Progress tracking and academic assessment" aria-pressed="false">
                            <span class="role-icon"><i class="fa-solid fa-user-tie" aria-hidden="true"></i></span>
                            <span class="role-name">Academic supervisor</span>
                        </button>
                        <button class="role-node role-coordinator" type="button" data-role="coordinator" data-title="Training coordinator" data-description="User and permission management" aria-pressed="false">
                            <span class="role-icon"><i class="fa-solid fa-clipboard-check" aria-hidden="true"></i></span>
                            <span class="role-name">Training coordinator</span>
                        </button>
                    </div>
                    <p class="map-readout" aria-live="polite">
                        <strong id="readoutTitle">Student</strong>
                        <span id="readoutText">Training opportunities and task tracking</span>
                    </p>
                </figure>

                <ol class="hero-flow" aria-label="Process stages">
                    <li><span class="flow-num">01</span>Application</li>
                    <li><span class="flow-num">02</span>Training</li>
                    <li><span class="flow-num">03</span>Assessment</li>
                </ol>

                <ul class="capabilities" aria-label="System capabilities">
                    <li><i class="fa-solid fa-check" aria-hidden="true"></i><span>Managing field training opportunities</span></li>
                    <li><i class="fa-solid fa-check" aria-hidden="true"></i><span>Organizing training requests and placements</span></li>
                    <li><i class="fa-solid fa-check" aria-hidden="true"></i><span>Tracking tasks and academic progress</span></li>
                    <li><i class="fa-solid fa-check" aria-hidden="true"></i><span>Academic and field assessment</span></li>
                    <li><i class="fa-solid fa-check" aria-hidden="true"></i><span>Role-based access management</span></li>
                    <li><i class="fa-solid fa-check" aria-hidden="true"></i><span>Development with ASP.NET Web Forms and C#</span></li>
                </ul>
            </div>
        </section>
<section class="section team" id="team" aria-labelledby="team-title">
            <div class="team-ambient-glow" aria-hidden="true"></div>
            <div class="container">
                <header class="section-head team-head" data-reveal>
                    <div>
                        <p class="kicker">The Architects of UIMS</p>
                        <h2 id="team-title">Interactive Team Pantheon</h2>
                    </div>
                    <div class="team-controls-wrap">
                        <p class="team-intro">Seven specialists engineering the future of academic field training. Explore the 3D gallery or use keyboard arrows to inspect each member's architectural dossier.</p>
                        <div class="pantheon-nav" aria-label="Team navigation controls">
                            <button class="control-orb" id="prevMember" type="button" aria-label="Previous team member">
                                <i class="fa-solid fa-arrow-left" aria-hidden="true"></i>
                            </button>
                            <div class="pantheon-counter" aria-live="polite">
                                <strong id="activeCounter">01</strong>
                                <span>/ 07</span>
                            </div>
                            <button class="control-orb" id="nextMember" type="button" aria-label="Next team member">
                                <i class="fa-solid fa-arrow-right" aria-hidden="true"></i>
                            </button>
                        </div>
                    </div>
                </header>

                <div class="pantheon-experience" id="dossier" data-reveal>
                    
                    <!-- The selected member's information -->
                    <div class="dossier-stage" id="dossier-panel" role="tabpanel" tabindex="0" aria-labelledby="tab-0">
                        <div class="stage-bg-watermark" id="stageWatermark" aria-hidden="true">01</div>

                        <!-- Left Column: 3D Multi-Layer Monolith Portrait -->
                        <div class="monolith-visual" id="monolithVisual">
                            <!-- Rotating Academic Astrolabe Ring -->
                            <svg class="astrolabe-ring" viewBox="0 0 500 500" aria-hidden="true">
                                <defs>
                                    <linearGradient id="goldOrbit" x1="0%" y1="0%" x2="100%" y2="100%">
                                        <stop offset="0%" stop-color="#C9A964" stop-opacity="0.6"/>
                                        <stop offset="50%" stop-color="#E8D9AE" stop-opacity="0.15"/>
                                        <stop offset="100%" stop-color="#8A6A2E" stop-opacity="0.5"/>
                                    </linearGradient>
                                </defs>
                                <circle cx="250" cy="250" r="228" fill="none" stroke="url(#goldOrbit)" stroke-width="1" stroke-dasharray="6 10" />
                                <circle cx="250" cy="250" r="196" fill="none" stroke="url(#goldOrbit)" stroke-width="1.2" />
                                <path d="M250 10 L250 34 M250 466 L250 490 M10 250 L34 250 M466 250 L490 250" stroke="#C9A964" stroke-width="1.5" stroke-opacity="0.5"/>
                                <circle cx="250" cy="22" r="4" fill="#E8D9AE" />
                                <circle cx="478" cy="250" r="4" fill="#C9A964" />
                                <circle cx="22" cy="250" r="4" fill="#C9A964" />
                            </svg>

                            <!-- Secondary Offset Architectural Frame -->
                            <div class="monolith-backdrop-arch" aria-hidden="true"></div>

                            <!-- Main Portrait Archway -->
                            <div class="dossier-frame" id="dossierFrame">
                                <div class="portrait-aura" id="portraitAura" aria-hidden="true"></div>
                                <img id="dossierImg" src="<%= ResolveUrl("~/Content/Images/Team/wessam_khraisat-transparent.png") %>" alt="Portrait of Wissam Khraisat" width="380" height="475">
                                <div class="portrait-glass-sheen" aria-hidden="true"></div>
                                
                                <!-- Animated Golden Edge Tracer -->
                                <svg class="portrait-edge-effect" viewBox="0 0 340 425" preserveAspectRatio="none" aria-hidden="true" focusable="false">
                                    <defs>
                                        <linearGradient id="portraitEdgeLight" x1="0" y1="0" x2="1" y2="1">
                                            <stop offset="0" stop-color="#94783E" stop-opacity=".18" />
                                            <stop offset=".38" stop-color="#E5CA82" />
                                            <stop offset=".64" stop-color="#FFF9E7" />
                                            <stop offset="1" stop-color="#B08C43" stop-opacity=".28" />
                                        </linearGradient>
                                    </defs>
                                    <path class="portrait-edge-track" pathLength="1000" d="M170 1 C84 1 1 58 1 128 V402 Q1 424 23 424 H317 Q339 424 339 402 V128 C339 58 256 1 170 1 Z" />
                                    <path class="portrait-edge-tracer" pathLength="1000" d="M170 1 C84 1 1 58 1 128 V402 Q1 424 23 424 H317 Q339 424 339 402 V128 C339 58 256 1 170 1 Z" />
                                    <path class="portrait-edge-glint" pathLength="1000" d="M170 1 C84 1 1 58 1 128 V402 Q1 424 23 424 H317 Q339 424 339 402 V128 C339 58 256 1 170 1 Z" />
                                </svg>
                            </div>

                            <div class="dossier-socials" aria-label="Team social profiles">
                                <a class="dossier-social-button" data-platform="linkedin" aria-disabled="true" tabindex="-1" aria-label="LinkedIn profile coming soon">
                                    <i class="fa-brands fa-linkedin-in" aria-hidden="true"></i><span>LinkedIn</span><span class="social-pending">Coming soon</span>
                                </a>
                                <a class="dossier-social-button" data-platform="github" aria-disabled="true" tabindex="-1" aria-label="GitHub profile coming soon">
                                    <i class="fa-brands fa-github" aria-hidden="true"></i><span>GitHub</span><span class="social-pending">Coming soon</span>
                                </a>
                            </div>
                            <!-- Floating 3D Parallax Badges -->
                            <div class="floating-badge badge-id" aria-hidden="true">
                                <span class="badge-label" id="dossierBadgeRole"></span>
                                <strong class="dossier-number" id="dossierNum">01</strong>
                            </div>

                            <div class="floating-badge badge-Verify" id="floatingSpecialty" aria-hidden="true">
                                <span class="pulse-dot"></span>
                                <i class="fa-solid fa-compass-drafting" id="floatingIcon"></i>
                                <span id="floatingTagText">UIMS Core Team</span>
                            </div>
                        </div>

                        <!-- Right Column: Editorial & Technical Dossier -->
                        <div class="dossier-info">
                            <div class="dossier-meta-bar">
                                <p class="dossier-role" id="dossierRole"></p>
                                <span class="yu-verified-pill"><i class="fa-solid fa-certificate"></i> YU × Orange Academy</span>
                            </div>

                            <h3 class="dossier-name" id="dossierName"></h3>
<p class="dossier-bio" id="dossierBio"></p>

                            <div class="dossier-grid-blocks">
                                <div class="dossier-block">
                                    <h4><i class="fa-solid fa-list-check" aria-hidden="true"></i> Key Duties & Responsibilities</h4>
                                    <ul class="duties" id="dossierDuties"></ul>
                                </div>

                                <div class="dossier-block">
                                    <h4><i class="fa-solid fa-microchip" aria-hidden="true"></i> Technical Focus Areas</h4>
                                    <ul class="chips" id="dossierAreas"></ul>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Team navigation gallery, shown beneath the selected member's information -->
                    <div class="kinetic-deck" id="roster" role="tablist" aria-label="Team members"></div>
                </div>
            </div>
        </section>
<section class="section architecture" id="architecture" aria-labelledby="arch-title" data-loop>
            <div class="container">
                <header class="section-head" data-reveal>
                    <div>
                        <p class="kicker">System overview</p>
                        <h2 id="arch-title">Technical architecture and data flow</h2>
                    </div>
                    <p class="section-lead">UIMS combines ASP.NET C# application logic, Firebase services, and a responsive interface built for the four user roles.</p>
                </header>

                <div class="arch-grid">
                    <article class="arch-card arch-a" data-reveal>
                        <span class="arch-index" aria-hidden="true">01</span>
                        <span class="arch-icon"><i class="fa-brands fa-microsoft" aria-hidden="true"></i></span>
                        <h3>ASP.NET Web Forms and C#</h3>
                        <p>Used to build the system's pages and to handle the training and assessment procedures defined in the project requirements.</p>
                    </article>

                    <article class="arch-card arch-b" data-reveal>
                        <span class="arch-index" aria-hidden="true">02</span>
                        <span class="arch-icon"><i class="fa-solid fa-user-shield" aria-hidden="true"></i></span>
                        <h3>Firebase Authentication</h3>
                        <p>Verifies user identity and manages permissions for the four roles: student, training provider, academic supervisor, and training coordinator.</p>
                    </article>

                    <div class="arch-core" data-reveal>
                        <span class="core-ring" aria-hidden="true"></span>
                        <i class="fa-solid fa-cubes-stacked" aria-hidden="true"></i>
                        <strong>UIMS</strong>
                        <span>Yarmouk University</span>
                    </div>

                    <article class="arch-card arch-c" data-reveal>
                        <span class="arch-index" aria-hidden="true">03</span>
                        <span class="arch-icon"><i class="fa-solid fa-bolt-lightning" aria-hidden="true"></i></span>
                        <h3>Firebase Realtime Database</h3>
                        <p>Stores and synchronizes training opportunities, requests, tasks, and assessments in real time.</p>
                    </article>

                    <article class="arch-card arch-d" data-reveal>
                        <span class="arch-index" aria-hidden="true">04</span>
                        <span class="arch-icon"><i class="fa-solid fa-mobile-screen-button" aria-hidden="true"></i></span>
                        <h3>Responsive Arabic interface</h3>
                        <p>Designed in Arabic with a right-to-left layout, with components adapted to suit different screen sizes.</p>
                    </article>
                </div>
            </div>
        </section>
<section class="section partner" id="partner" aria-labelledby="partner-title">
            <div class="container">
                <div class="partner-panel" data-reveal>
                    <div class="partner-copy">
                        <p class="partner-badge"><i class="fa-solid fa-fire-flame-curved" aria-hidden="true"></i>Training partner</p>
                        <h2 id="partner-title">Applied training at <span>Orange Coding Academy</span></h2>
                        <p>UIMS is being developed through applied training at Orange Coding Academy. The project lets the team apply their skills in software development, interface and user experience design, and teamwork within Agile teams.</p>
                        <ul class="partner-tags">
                            <li>Web application development</li>
                            <li>Agile and Scrum teamwork</li>
                            <li>Interface and user experience design</li>
                            <li>Digital applications for academia</li>
                        </ul>
                    </div>

                    <aside class="partner-board" aria-label="Project figures">
                        <div class="orange-mark">
                            <img src="<%= ResolveUrl("~/Content/Images/Team/orange-official.jpg") %>" alt="Orange" width="120" height="120" loading="lazy" decoding="async">
                        </div>
                        <h3>Orange Coding Academy</h3>
                        <p class="board-sub">Coding and professional training</p>
                        <div class="figures">
                            <div class="figure"><strong>07</strong><span>Team members</span></div>
                            <div class="figure"><strong>04</strong><span>User roles</span></div>
                            <div class="figure figure-tech"><strong>ASP.NET</strong><span>System development technologies</span></div>
                            <div class="figure figure-tech"><strong>Firebase</strong><span>Database and synchronization services</span></div>
                        </div>
                    </aside>
                </div>
            </div>
        </section>
</asp:Content>
