(() => {
  'use strict';
  // Keep the overlay outside animated/overflow-clipped cards while inside the WebForms form.
  const webFormsForm = document.querySelector('form');
  const dialogByTrigger = new WeakMap();
  document.querySelectorAll('.evaluation-card').forEach(card => {
    const trigger = card.querySelector('[data-open-evaluation]');
    const dialog = card.querySelector('.evaluation-dialog');
    if (!trigger || !dialog) return;
    dialogByTrigger.set(trigger, dialog);
    if (webFormsForm) webFormsForm.appendChild(dialog);
  });

  document.querySelectorAll('[data-evaluation-feedback]').forEach(field => {
    const counter = field.parentElement.querySelector('[data-char-count]');
    if (!counter) return;
    const updateCount = () => {
      counter.textContent = `${field.value.length} / 2000`;
    };
    field.addEventListener('input', updateCount);
    updateCount();
  });

  const openDialog = dialog => {
    if (!dialog) return;
    dialog.__returnFocus = document.activeElement;
    dialog.hidden = false;
    dialog.classList.add('is-open');
    document.body.classList.add('evaluation-modal-open');
    const score = dialog.querySelector('input[type="number"]:not(:disabled)');
    if (score) score.focus();
    else dialog.focus();
  };
  const closeDialog = dialog => {
    if (!dialog) return;
    dialog.classList.remove('is-open');
    dialog.hidden = true;
    document.body.classList.remove('evaluation-modal-open');
    if (dialog.__returnFocus && typeof dialog.__returnFocus.focus === 'function') dialog.__returnFocus.focus();
  };

  document.addEventListener('click', event => {
    if (!(event.target instanceof Element)) return;
    const openButton = event.target.closest('[data-open-evaluation]');
    if (openButton) {
      event.preventDefault();
      openDialog(dialogByTrigger.get(openButton));
      return;
    }
    const closeButton = event.target.closest('[data-close-evaluation]');
    if (closeButton) {
      event.preventDefault();
      closeDialog(closeButton.closest('.evaluation-dialog'));
      return;
    }
    if (event.target.matches('.evaluation-dialog.is-open')) closeDialog(event.target);
  });
  document.addEventListener('keydown', event => {
    const dialog = document.querySelector('.evaluation-dialog.is-open');
    if (!dialog) return;
    if (event.key === 'Escape') {
      closeDialog(dialog);
      return;
    }
    if (event.key !== 'Tab') return;
    const focusable = Array.from(dialog.querySelectorAll('button:not(:disabled), input:not(:disabled), textarea:not(:disabled)'));
    if (!focusable.length) return;
    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    if (event.shiftKey && document.activeElement === first) {
      event.preventDefault();
      last.focus();
    } else if (!event.shiftKey && document.activeElement === last) {
      event.preventDefault();
      first.focus();
    }
  });

  document.querySelectorAll('.evaluation-dialog input[type="number"]').forEach(score => {
    score.addEventListener('input', () => {
      if (score.value !== '' && Number(score.value) > 100) score.value = '100';
      if (Number(score.value) < 0) score.value = '0';
    });
  });

  if (window.__reopenEvaluationPlacementId) {
    const dialog = Array.from(document.querySelectorAll('.evaluation-dialog'))
      .find(item => item.dataset.placementId === window.__reopenEvaluationPlacementId);
    if (dialog) openDialog(dialog);
  }
})();
