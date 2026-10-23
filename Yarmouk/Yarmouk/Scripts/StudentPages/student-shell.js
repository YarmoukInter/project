/* Shared/sidebar.js owns student navigation. Keep only student content enhancements here. */
(() => {
  'use strict';
  document.querySelectorAll('[data-student-year]').forEach(year => { year.textContent = String(new Date().getFullYear()); });
})();
