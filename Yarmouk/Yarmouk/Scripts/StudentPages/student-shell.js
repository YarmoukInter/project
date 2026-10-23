(() => {
  'use strict';
  const root = document.querySelector('.student-layout');
  const rail = document.querySelector('[data-student-nav]');
  if (!root || !rail) return;
  const sidebar = rail.querySelector('.student-sidebar');
  const navigation = sidebar.querySelector('.student-navigation');
  const profile = sidebar.querySelector('.student-profile-link');
  const toggle = sidebar.querySelector('.student-sidebar-toggle');
  const pin = sidebar.querySelector('.student-sidebar-pin');
  const menu = document.querySelector('.student-nav-toggle');
  const backdrop = document.querySelector('.student-nav-backdrop');
  const small = matchMedia('(max-width:1080px)');
  const storageKey = 'yarmouk:student:sidebar:pinned';
  let pinned = false, hovering = false, focused = false, manual = false, mobileOpen = false, suppressed = false, leaveTimer;
  try { pinned = sessionStorage.getItem(storageKey) === 'true'; } catch (_) {}

  const currentPath = location.pathname.replace(/\.aspx$/i, '').toLowerCase();
  [...navigation.querySelectorAll('a'), profile].forEach(link => {
    if (new URL(link.href, location.href).pathname.replace(/\.aspx$/i, '').toLowerCase() === currentPath) {
      link.setAttribute('aria-current', 'page');
    }
  });
  root.classList.add('is-enhanced');

  function render() {
    const open = small.matches ? mobileOpen : pinned || manual || (!suppressed && (hovering || focused));
    rail.classList.toggle('is-open', open && !small.matches);
    sidebar.classList.toggle('is-collapsed', !open && !small.matches);
    sidebar.classList.toggle('is-mobile-open', small.matches && mobileOpen);
    sidebar.inert = small.matches && !mobileOpen;
    toggle.setAttribute('aria-expanded', String(open));
    toggle.setAttribute('aria-label', open ? 'Collapse navigation' : 'Expand navigation');
    pin.setAttribute('aria-checked', String(pinned));
    menu.setAttribute('aria-expanded', String(small.matches && mobileOpen));
    backdrop.hidden = !(small.matches && mobileOpen);
  }
  function savePin() { try { sessionStorage.setItem(storageKey, String(pinned)); } catch (_) {} }
  function closeMobile(restoreFocus = false) {
    mobileOpen = false;
    render();
    if (restoreFocus) menu.focus();
  }

  sidebar.addEventListener('pointerenter', event => {
    if (small.matches || event.pointerType === 'touch') return;
    clearTimeout(leaveTimer); hovering = true; suppressed = false; render();
  });
  sidebar.addEventListener('pointerleave', () => {
    if (small.matches) return;
    leaveTimer = setTimeout(() => { hovering = false; manual = false; suppressed = false; render(); }, 220);
  });
  sidebar.addEventListener('focusin', event => {
    if (!small.matches && event.target.matches(':focus-visible')) { focused = true; suppressed = false; render(); }
  });
  sidebar.addEventListener('focusout', event => {
    if (!sidebar.contains(event.relatedTarget)) { focused = false; if (small.matches) closeMobile(); else render(); }
  });
  toggle.addEventListener('click', () => {
    if (small.matches) { closeMobile(true); return; }
    if (rail.classList.contains('is-open')) {
      pinned = false; manual = false; hovering = false; focused = false; suppressed = true; savePin();
    } else { manual = true; suppressed = false; }
    render();
  });
  pin.addEventListener('click', () => { pinned = !pinned; manual = false; savePin(); render(); });
  menu.addEventListener('click', () => {
    mobileOpen = !mobileOpen; render();
    if (mobileOpen) navigation.querySelector('a').focus();
  });
  backdrop.addEventListener('click', () => closeMobile(true));
  navigation.addEventListener('click', event => { if (event.target.closest('a')) closeMobile(); });
  document.addEventListener('keydown', event => {
    if (event.key !== 'Escape') return;
    if (mobileOpen) { closeMobile(true); return; }
    if (!small.matches && sidebar.contains(document.activeElement)) {
      manual = false; hovering = false; focused = false; suppressed = true; render(); toggle.focus();
    }
  });
  small.addEventListener('change', () => { mobileOpen = false; hovering = false; focused = false; manual = false; render(); });
  document.querySelectorAll('[data-student-year]').forEach(year => { year.textContent = String(new Date().getFullYear()); });
  render();
})();
