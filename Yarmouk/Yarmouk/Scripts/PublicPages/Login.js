/* Presentation only. Authentication and redirects are handled by the server. */
(() => {
  const gate = document.getElementById('gatePanel');
  const email = document.getElementById('loginEmail');
  const password = document.getElementById('loginPassword');
  const toggle = document.getElementById('togglePasswordBtn');
  email.addEventListener('focus', () => gate.classList.add('is-email-focus'));
  email.addEventListener('blur', () => gate.classList.remove('is-email-focus'));
  password.addEventListener('focus', () => gate.classList.add('is-doors-closed'));
  password.addEventListener('input', () => {
    gate.classList.toggle('is-doors-closed', !!password.value);
    document.querySelectorAll('.gate-carved-dot').forEach((dot, i) => dot.classList.toggle('is-active', i < password.value.length));
  });
  toggle.addEventListener('click', () => {
    const show = password.type === 'password';
    password.type = show ? 'text' : 'password';
    toggle.setAttribute('aria-pressed', String(show));
    toggle.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
    document.getElementById('toggleIcon').textContent = show ? 'Hide' : 'Show';
    gate.classList.toggle('is-password-revealed', show);
  });
  const cards = [...document.querySelectorAll('.lb-role-card')];
  const selectedRole = document.getElementById('SelectedRole');
  function choose(card) {
    cards.forEach(item => { const active = item === card; item.classList.toggle('is-active', active); item.setAttribute('aria-checked', String(active)); item.tabIndex = active ? 0 : -1; });
    selectedRole.value = card.dataset.role;
  }
  cards.forEach((card, index) => {
    card.tabIndex = index ? -1 : 0;
    card.addEventListener('click', () => choose(card));
    card.addEventListener('keydown', event => {
      if (!['ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(event.key)) return;
      event.preventDefault();
      const next = cards[(index + (event.key === 'ArrowLeft' || event.key === 'ArrowUp' ? -1 : 1) + cards.length) % cards.length];
      choose(next); next.focus();
    });
  });
  choose(cards.find(card => card.dataset.role === selectedRole.value) || cards[0]);
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
      gate.style.setProperty('--tilt-y', `${((event.clientX - rect.left) / rect.width - .5) * 7}deg`);
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
