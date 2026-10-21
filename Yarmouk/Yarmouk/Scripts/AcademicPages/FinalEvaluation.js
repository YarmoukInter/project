(() => {
  'use strict';
  document.querySelectorAll('[data-evaluation-feedback]').forEach(field => {
    const counter = field.parentElement.querySelector('[data-char-count]');
    if (!counter) return;
    const updateCount = () => {
      counter.textContent = `${field.value.length} / 2000`;
    };
    field.addEventListener('input', updateCount);
    updateCount();
  });

  document.querySelectorAll('.evaluation-card').forEach(card => {
    const score = card.querySelector('input[type="number"]');
    if (!score) return;
    score.addEventListener('input', () => {
      if (score.value !== '' && Number(score.value) > 100) score.value = '100';
      if (Number(score.value) < 0) score.value = '0';
    });
  });
})();
