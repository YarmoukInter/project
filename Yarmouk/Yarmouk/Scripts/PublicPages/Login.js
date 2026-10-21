/* Presentation only. Authentication and redirects are handled by the server. */
(() => {
  const gate = document.getElementById('gatePanel');
  const socialButtons = [...document.querySelectorAll('.social-login-btn[data-provider]')];
  const email = document.getElementById('loginEmail');
  const password = document.getElementById('loginPassword');
  const toggle = document.getElementById('togglePasswordBtn');
  socialButtons.forEach(button => {
    button.addEventListener('click', () => startSocialSignIn(button.dataset.provider));
  });
  async function startSocialSignIn(providerName) {
    const createProvider = {
      google: () => new firebase.auth.GoogleAuthProvider(),
      github: () => new firebase.auth.GithubAuthProvider()
    }[providerName];
    if (!createProvider) return;
    const providerLabel = { google: 'Google', github: 'GitHub' }[providerName];
    socialButtons.forEach(button => { button.disabled = true; });
    const message = document.querySelector('.auth-message');
    message.textContent = '';
    try {
      if (!window.firebase || !window.yarmoukFirebaseConfig) throw new Error('Firebase setup is unavailable.');
      if (!firebase.apps.length) firebase.initializeApp(window.yarmoukFirebaseConfig);
      const auth = firebase.auth();
      const provider = createProvider();
      if (providerName === 'google')
        provider.setCustomParameters({ prompt: 'select_account' });
      if (providerName === 'github') provider.addScope('user:email');
      const result = await auth.signInWithPopup(provider);
      document.getElementById('SocialIdToken').value = await result.user.getIdToken();
      document.getElementById('SocialSignInButton').click();
    } catch (error) {
      gate.classList.add('is-error');
      const messages = {
        'auth/operation-not-allowed': `${providerLabel} sign-in is not enabled in Firebase Authentication yet.`,
        'auth/popup-closed-by-user': `The ${providerLabel} sign-in window was closed before completion.`,
        'auth/popup-blocked': `Your browser blocked the ${providerLabel} sign-in window. Allow pop-ups and try again.`,
        'auth/unauthorized-domain': 'This website domain is not authorized for sign-in. Add localhost in Firebase Authentication settings for local development.',
        'auth/account-exists-with-different-credential': 'This email already uses a different sign-in method. Contact the coordinator to link your account.'
      };
      message.textContent = messages[error.code] || `${providerLabel} sign-in could not be completed. Check that the provider is enabled and its OAuth settings are complete in Firebase.`;
    } finally {
      socialButtons.forEach(button => { button.disabled = false; });
    }
  }
  email.addEventListener('focus', () => gate.classList.add('is-email-focus'));
  email.addEventListener('blur', () => gate.classList.remove('is-email-focus'));
  toggle.addEventListener('click', () => {
    const show = password.type === 'password';
    password.type = show ? 'text' : 'password';
    toggle.setAttribute('aria-pressed', String(show));
    toggle.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
    document.getElementById('toggleIcon').textContent = show ? 'Hide' : 'Show';
  });
  document.getElementById('SignInButton').addEventListener('click', () => {
    if (!email.value.trim() || !password.value) return;
    gate.classList.remove('is-error');
  });
  const authMessage = document.querySelector('#hrLoginForm .auth-message');
  if (authMessage && authMessage.textContent.trim()) gate.classList.add('is-error');
  document.getElementById('loginThemeToggle').addEventListener('click', () => {
    const dark = document.documentElement.dataset.theme !== 'dark';
    document.documentElement.dataset.theme = dark ? 'dark' : 'light';
    document.getElementById('loginThemeToggle').setAttribute('aria-pressed', String(dark));
  });
  if (!matchMedia('(prefers-reduced-motion: reduce)').matches && matchMedia('(pointer: fine)').matches) {
    gate.addEventListener('pointermove', event => {
      const rect = gate.getBoundingClientRect();
      gate.classList.add('has-pointer');
      gate.style.setProperty('--mouse-x', `${event.clientX - rect.left}px`);
      gate.style.setProperty('--mouse-y', `${event.clientY - rect.top}px`);
      gate.style.setProperty('--tilt-y', `${((event.clientX - rect.left) / rect.width - .5) * 3}deg`);
    });
    gate.addEventListener('pointerleave', () => { gate.classList.remove('has-pointer'); gate.style.setProperty('--tilt-y', '0deg'); });
  }

  const recoveryLink = document.getElementById('forgotPasswordLink');
  const recoveryBackdrop = document.getElementById('recoveryBackdrop');
  const recoveryDialog = document.getElementById('recovery');
  const recoveryEmail = document.getElementById('resetEmail');
  const recoveryClose = document.getElementById('recoveryClose');
  const recoveryBack = document.getElementById('recoveryBack');
  let recoveryReturnFocus = null;

  function openRecovery() {
    recoveryReturnFocus = document.activeElement;
    recoveryBackdrop.classList.add('is-open');
    recoveryBackdrop.setAttribute('aria-hidden', 'false');
    document.body.classList.add('recovery-open');
    requestAnimationFrame(() => recoveryEmail.focus());
  }

  function closeRecovery() {
    recoveryBackdrop.classList.remove('is-open');
    recoveryBackdrop.setAttribute('aria-hidden', 'true');
    document.body.classList.remove('recovery-open');
    if (recoveryReturnFocus) recoveryReturnFocus.focus();
  }

  recoveryLink.addEventListener('click', event => { event.preventDefault(); openRecovery(); });
  recoveryClose.addEventListener('click', closeRecovery);
  recoveryBack.addEventListener('click', closeRecovery);
  recoveryBackdrop.addEventListener('click', event => {
    if (event.target === recoveryBackdrop) closeRecovery();
  });
  document.addEventListener('keydown', event => {
    if (!recoveryBackdrop.classList.contains('is-open')) return;
    if (event.key === 'Escape') { closeRecovery(); return; }
    if (event.key !== 'Tab') return;
    const focusable = [...recoveryDialog.querySelectorAll('button, input, a[href]')]
      .filter(element => !element.disabled && element.offsetParent !== null);
    if (!focusable.length) return;
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
    else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
  });

  // Keep the dialog open after the server posts a recovery result or validation message.
  if (document.querySelector('#recovery .auth-message').textContent.trim()) openRecovery();
})();
