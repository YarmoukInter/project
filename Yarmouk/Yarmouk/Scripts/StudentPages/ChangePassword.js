/* No credentials or authentication state are stored in the browser. */
(() => {
  document.querySelectorAll('[data-password]').forEach(button => {
    button.addEventListener('click', () => {
      const input = document.getElementById(button.dataset.password);
      const show = input.type === 'password';
      input.type = show ? 'text' : 'password';
      button.textContent = show ? 'Hide' : 'Show';
      button.setAttribute('aria-pressed', String(show));
      button.setAttribute('aria-label', `${show ? 'Hide' : 'Show'} ${document.querySelector(`label[for="${input.id}"]`).textContent.toLowerCase()}`);
    });
  });
})();
