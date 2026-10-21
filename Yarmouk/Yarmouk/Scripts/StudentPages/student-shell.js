(() => {
  'use strict';
  const header = document.querySelector('[data-student-nav]');
  if (!header) return;

  const toggle = header.querySelector('.student-nav-toggle');
  const navigation = header.querySelector('.student-navigation');
  const mobile = window.matchMedia('(max-width: 760px)');
  toggle.hidden = false;

  const currentPath = window.location.pathname.toLowerCase();
  navigation.querySelectorAll('a').forEach(link => {
    const linkPath = new URL(link.href, window.location.href).pathname.toLowerCase();
    if (linkPath === currentPath) link.setAttribute('aria-current', 'page');
  });

  const closeMenu = (restoreFocus = false) => {
    header.classList.remove('is-open');
    toggle.setAttribute('aria-expanded', 'false');
    toggle.setAttribute('aria-label', 'Open student navigation');
    if (restoreFocus) toggle.focus();
  };

  toggle.addEventListener('click', () => {
    const open = !header.classList.contains('is-open');
    header.classList.toggle('is-open', open);
    toggle.setAttribute('aria-expanded', String(open));
    toggle.setAttribute('aria-label', open ? 'Close student navigation' : 'Open student navigation');
  });
  navigation.addEventListener('click', event => {
    if (event.target.closest('a')) closeMenu();
  });
  document.addEventListener('pointerdown', event => {
    if (!header.contains(event.target)) closeMenu();
  });
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape') closeMenu(true);
  });
  mobile.addEventListener('change', () => closeMenu());
  document.querySelectorAll('[data-student-year]').forEach(year => {
    year.textContent = String(new Date().getFullYear());
  });
})();
