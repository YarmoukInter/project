/* Plain browser JavaScript for ASP.NET Web Forms. No C# required. */
(() => {
  'use strict';
  let gateComplete = document.body?.dataset.gateComplete === 'true';
  let refresh = () => {};

  // Call this from the gate animation with its real completion state.
  // Passing false expands the navbar again when scrolling backwards.
  window.PublicNavbar = {
    setGateComplete(complete) {
      gateComplete = Boolean(complete);
      refresh();
    }
  };

  function init() {
    const header = document.querySelector('[data-public-navbar]');
    if (!header) return;
    const toggle = header.querySelector('.yu-nav-toggle');
    const links = header.querySelector('.yu-nav-links');
    const mobile = matchMedia('(max-width: 700px)');
    header.classList.add('is-enhanced');
    toggle.hidden = false;

    // Automatic active-page indicator; works with virtual application folders.
    const currentPath = location.pathname.toLowerCase();
    links.querySelectorAll('a').forEach(link => {
      const active = new URL(link.href).pathname.toLowerCase() === currentPath;
      if (active) link.setAttribute('aria-current', 'page');
      else link.removeAttribute('aria-current');
    });

    function close(restoreFocus = false) {
      header.classList.remove('is-open');
      toggle.setAttribute('aria-expanded', 'false');
      toggle.setAttribute('aria-label', 'Open navigation');
      if (restoreFocus) toggle.focus();
    }

    refresh = () => {
      const compact = header.dataset.resize === 'gate'
        ? gateComplete
        : Math.max(window.scrollY, document.querySelector('[data-navbar-scroll]')?.scrollTop || 0) > 100;
      header.classList.toggle('is-compact', compact);
    };

    toggle.addEventListener('click', () => {
      const open = !header.classList.contains('is-open');
      header.classList.toggle('is-open', open);
      toggle.setAttribute('aria-expanded', String(open));
      toggle.setAttribute('aria-label', open ? 'Close navigation' : 'Open navigation');
    });
    links.addEventListener('click', event => {
      if (event.target.closest('a')) close();
    });
    document.addEventListener('keydown', event => {
      if (event.key === 'Escape' && header.classList.contains('is-open')) close(true);
    });
    document.addEventListener('pointerdown', event => {
      if (!header.contains(event.target)) close();
    });
    header.addEventListener('focusout', event => {
      if (!header.contains(event.relatedTarget)) close();
    });
    mobile.addEventListener('change', () => close());
    window.addEventListener('scroll', refresh, { passive: true });
    window.addEventListener('resize', refresh, { passive: true });
    window.addEventListener('pageshow', refresh);
    document.querySelector('[data-navbar-scroll]')?.addEventListener('scroll', refresh, { passive: true });

    // Also supports the existing Yarmouk gate animation's completion event.
    window.addEventListener('yarmouk:gate-assembly', () => {
      window.PublicNavbar.setGateComplete(document.body.dataset.gateComplete === 'true');
    });
    refresh();
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init, { once: true });
  else init();
})();
