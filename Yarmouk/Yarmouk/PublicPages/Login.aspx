<%@ Page Title="Sign In" Language="C#" Async="true" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Yarmouk.PublicPages.Login" %>
<!DOCTYPE html><html lang="en"><head runat="server"><meta charset="utf-8" /><meta name="viewport" content="width=device-width, initial-scale=1" /><title>Sign In — Yarmouk University</title>
<link href="<%= ResolveUrl("~/Content/bootstrap.min.css") %>" rel="stylesheet" />
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
<link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap" rel="stylesheet" />
<link href="<%= ResolveUrl("~/Content/PublicPages/Login.css") %>?v=oauth-google-github-3" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/PublicPages/Login.js") %>?v=oauth-google-github-3" defer></script>
<script src="<%= ResolveUrl("~/Scripts/PublicPages/EmblemMorph.js") %>" defer></script>
<script src="https://www.gstatic.com/firebasejs/10.14.1/firebase-app-compat.js"></script>
<script src="https://www.gstatic.com/firebasejs/10.14.1/firebase-auth-compat.js"></script>
<script>
  window.yarmoukFirebaseConfig = {
    apiKey: '<%= System.Web.Configuration.WebConfigurationManager.AppSettings["FirebaseWebApiKey"] %>',
    authDomain: 'yarmouk-uims.firebaseapp.com',
    projectId: 'yarmouk-uims',
    appId: '1:930938489436:web:70e4d7616b8886446c7499'
  };
</script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
</head><body id="page-login"><form id="AuthForm" runat="server" novalidate>
    <div class="login-page">
      <div class="lb-ambience" aria-hidden="true"></div>

      <div class="lb-container">
        <!-- ================================================================
             UNIVERSITY EMBLEM AND GOLDEN JUBILEE TRANSFORMATION
             ================================================================ -->
        <section
          class="gate-panel"
          id="gatePanel"
          aria-label="Yarmouk University emblem"
        >
          <!-- Faint gold radial light following cursor on desktop -->
          <div class="gate-cursor-light" id="gateCursorLight" aria-hidden="true"></div>

          <!-- Official Header Order: Logo on dark backing, Title, Faculty -->
          <header class="gate-header">
            <a
              href="<%= ResolveUrl("~/PublicPages/Home.aspx") %>"
              class="gate-brand-link"
              title="Yarmouk University Home"
            >
              <div class="gate-logo-mount">
                <img
                  src="<%= ResolveUrl("~/Content/Images/yarmouk-college-logo.png") %>"
                  alt="Yarmouk University"
                  class="gate-logo-img"
                  width="150"
                  height="54"
                />
              </div>
              <div class="gate-brand-meta">
                <span class="gate-gateway-title">Internship Management System</span>
                <span class="gate-dept-name">Faculty of Information Technology and Computer Science</span>
              </div>
            </a>
          </header>

          <!-- Persistent emblem geometry morphs into the Golden Jubilee mark. -->
          <figure class="gate-stage-area emblem-stage" id="gateStageArea">
            <img class="gate-emblem" src="<%= ResolveUrl("~/Content/Images/yarmouk-emblem-assembly.svg") %>" width="500" height="640" alt="Yarmouk University emblem transforming into the Golden Jubilee emblem." />
          </figure>
          <footer class="gate-footer">
            <span class="gate-footer-line"></span>
            <p class="gate-caption-text">Irbid, Jordan <span aria-hidden="true">·</span> Since 1976</p>
          </footer>
        </section>

        <!-- ================================================================
             LOGIN FORM PANEL (Right Side Panel)
             ================================================================ -->
        <section class="lb-form-panel" aria-label="Portal Form Panel">
          <div class="lb-form-container">
            <div class="lb-top-nav-bar">
              <div class="lb-portal-pill">
                <span class="lb-portal-pill-dot"></span>
                <span>Internship Portal</span>
              </div>

              <div class="d-flex align-items-center gap-2">
                <button
                  class="lb-theme-btn"
                  id="loginThemeToggle"
                  aria-label="Toggle dark mode"
                  title="Toggle dark or light theme"
                  type="button"
                >
                  <span id="loginThemeIcon" aria-hidden="true">◐</span>
                </button>

                <a
                  href="<%= ResolveUrl("~/PublicPages/Home.aspx") %>"
                  class="lb-home-btn"
                  title="Yarmouk University Home"
                >
                  <span aria-hidden="true">◇</span>
                  <span>Home</span>
                </a>
              </div>
            </div>

            <div class="lb-header-block">
              <span class="lb-section-label">YARMOUK INTERNSHIP PORTAL</span>
              <h1 class="lb-title">Welcome back<span class="lb-title-dot">.</span></h1>
              <p class="lb-subtitle">Sign in to continue your internship journey.</p>
            </div>

            <div class="lb-auth-card">
              <div id="hrLoginForm">
                <asp:HiddenField ID="SocialIdToken" runat="server" ClientIDMode="Static" />
                <div class="lb-field">
                  <label for="loginEmail">Email address</label>
                  <asp:TextBox runat="server" ID="loginEmail" ClientIDMode="Static" TextMode="Email" CssClass="form-control lb-form-control" placeholder="Enter your registered email" autocomplete="username" MaxLength="254" />
                </div>
                <div class="lb-field">
                  <div class="lb-field-heading">
                    <label for="loginPassword">Password</label>
                    <a href="#recovery" id="forgotPasswordLink" class="lb-link-forgot">Forgot password?</a>
                  </div>
                  <div class="lb-password-wrap">
                    <asp:TextBox runat="server" ID="loginPassword" ClientIDMode="Static" TextMode="Password" CssClass="form-control lb-form-control" placeholder="Enter your password" autocomplete="current-password" MaxLength="128" />
                    <button class="lb-pwd-toggle" type="button" id="togglePasswordBtn" aria-label="Toggle password visibility"><span id="toggleIcon">Show</span></button>
                  </div>
                </div>
                <p class="auth-message" role="alert"><asp:Literal ID="Message" runat="server" /></p>
                <asp:Button runat="server" ID="SignInButton" Text="Sign in" CssClass="btn btn-primary w-100 lb-submit-btn" OnClick="SignIn_Click" />
                <div class="login-divider"><span>or continue with</span></div>
                <div class="social-login" aria-label="Sign in with a provider">
                  <button type="button" class="social-login-btn" id="googleSignIn" data-provider="google" aria-label="Continue with Google">
                    <svg class="social-provider-mark" viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path fill="#4285F4" d="M21.6 12.23c0-.71-.06-1.42-.18-2.1H12v3.98h5.38a4.6 4.6 0 0 1-2 3.01v2.5h3.24c1.9-1.75 2.98-4.34 2.98-7.39z"/><path fill="#34A853" d="M12 22c2.7 0 4.96-.9 6.62-2.38l-3.23-2.5c-.9.6-2.04.96-3.39.96-2.6 0-4.8-1.76-5.58-4.12H3.1v2.58A10 10 0 0 0 12 22z"/><path fill="#FBBC05" d="M6.42 13.84a6 6 0 0 1 0-3.68V7.58H3.1a10 10 0 0 0 0 8.84l3.32-2.58z"/><path fill="#EA4335" d="M12 5.96c1.43 0 2.71.49 3.72 1.46l2.79-2.79A10 10 0 0 0 3.1 7.58l3.32 2.58C7.2 7.72 9.4 5.96 12 5.96z"/></svg>
                    <span>Continue with Google</span>
                  </button>
                  <button type="button" class="social-login-btn" id="githubSignIn" data-provider="github" aria-label="Continue with GitHub">
                    <svg class="social-provider-mark social-github-mark" viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61-.546-1.387-1.333-1.757-1.333-1.757-1.09-.745.083-.73.083-.73 1.205.085 1.84 1.237 1.84 1.237 1.07 1.834 2.807 1.304 3.492.997.108-.775.42-1.305.762-1.605-2.665-.3-5.466-1.333-5.466-5.93 0-1.31.467-2.38 1.235-3.22-.135-.303-.54-1.523.115-3.176 0 0 1.005-.322 3.3 1.23a11.5 11.5 0 0 1 6.005 0c2.295-1.552 3.3-1.23 3.3-1.23.655 1.653.25 2.873.12 3.176.77.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.43.37.81 1.096.81 2.21 0 1.595-.015 2.88-.015 3.27 0 .32.21.695.825.575C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12z"/></svg>
                    <span>Continue with GitHub</span>
                  </button>
                </div>
                <asp:Button runat="server" ID="SocialSignInButton" Text="Continue" CssClass="social-postback" OnClick="SocialSignIn_Click" UseSubmitBehavior="false" />
              </div>
              <div class="lb-card-footer-note">
                <span class="lb-note-mark" aria-hidden="true">✓</span>
                <span>Accounts are managed by the University Internship Coordinator.</span>
              </div>
            </div>
          </div>
        </section>
      </div>

      <div class="auth-recovery-backdrop" id="recoveryBackdrop" aria-hidden="true">
        <section class="auth-recovery" id="recovery" role="dialog" aria-modal="true" aria-labelledby="recoveryTitle" tabindex="-1">
          <button class="auth-recovery-close" id="recoveryClose" type="button" aria-label="Close password recovery">&times;</button>
          <span class="auth-recovery-eyebrow">ACCOUNT ACCESS</span>
          <h2 id="recoveryTitle">Reset your password</h2>
          <p class="auth-recovery-copy">Enter the email address registered with the internship coordinator. We’ll send a secure reset link if the account is eligible.</p>
          <label for="resetEmail">Email address</label>
          <asp:TextBox ID="resetEmail" runat="server" ClientIDMode="Static" TextMode="Email" CssClass="form-control auth-recovery-input" autocomplete="email" MaxLength="254" placeholder="name@yu.edu.jo" />
          <p role="status" aria-live="polite" class="auth-message"><asp:Literal ID="RecoveryMessage" runat="server" /></p>
          <div class="auth-recovery-actions">
            <asp:Button ID="RecoverButton" runat="server" Text="Send reset link" CssClass="btn auth-recovery-submit" OnClick="Recover_Click" />
            <button class="auth-recovery-back" id="recoveryBack" type="button">Back to sign in</button>
          </div>
        </section>
      </div>
    </div>
  </form></body></html>
