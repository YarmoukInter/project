/* Plain JavaScript; safe with Web Forms client IDs and partial postbacks. */
(function () {
  "use strict";
  function initializeFooter() {
    document.querySelectorAll(".yu-footer [data-footer-year]").forEach(function (element) {
      element.textContent = String(new Date().getFullYear());
    });
  }
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", initializeFooter, { once: true });
  else initializeFooter();
  // A ScriptManager/UpdatePanel can replace the footer after initial page load.
  if (window.Sys && window.Sys.Application) window.Sys.Application.add_load(initializeFooter);
})();
