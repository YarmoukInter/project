(() => {
  'use strict';
  // Keep the overlay outside animated/overflow-clipped cards while inside the WebForms form.
  const webFormsForm = document.querySelector('form');
  const dialogByTrigger = new WeakMap();
  const rows = [...document.querySelectorAll('.evaluation-card')];
  const filterButtons = [...document.querySelectorAll('[data-evaluation-filter]')];
  filterButtons.forEach(button => {
    const state = button.dataset.evaluationFilter;
    const count = button.querySelector('[data-evaluation-count]');
    if (count) count.textContent = String(state === 'all' ? rows.length : rows.filter(row=>row.dataset.status===state).length);
    button.addEventListener('click',()=>{
      filterButtons.forEach(item=>item.setAttribute('aria-pressed',String(item===button)));
      rows.forEach(row=>{row.hidden=state!=='all'&&row.dataset.status!==state;});
    });
  });
  const filters = document.querySelector('.evaluation-state-filters');
  if(filters) filters.hidden = !rows.length;
  function selectPanel(dialog, name, focusTab) {
    dialog.dataset.activePanel=name;
    dialog.querySelectorAll('[data-evaluation-tab]').forEach(button=>{
      const active=button.dataset.evaluationTab===name;
      button.setAttribute('aria-selected',String(active));
      button.tabIndex=active?0:-1;
      if(active&&focusTab)button.focus();
    });
    dialog.querySelectorAll('[data-evaluation-panel]').forEach(panel=>{panel.hidden=panel.dataset.evaluationPanel!==name;});
  }
  document.querySelectorAll('.evaluation-card').forEach(card => {
    const trigger = card.querySelector('[data-open-evaluation]');
    const dialog = card.querySelector('.evaluation-dialog');
    if (!trigger || !dialog) return;
    dialogByTrigger.set(trigger, dialog);
    if (webFormsForm) webFormsForm.appendChild(dialog);
    const academic=dialog.querySelector('.evaluation-form-grid');
    const meeting=dialog.querySelector('.zoom-review');
    if(academic&&meeting) {
      const tabs=document.createElement('div');
      tabs.className='evaluation-form-tabs';
      tabs.setAttribute('role','tablist');
      tabs.setAttribute('aria-label','Assessment sections');
      const prefix='evaluation-section-'+rows.indexOf(card);
      [['academic','Academic assessment',academic],['meeting','Meeting review',meeting]].forEach(([key,label,panel])=>{
        const button=document.createElement('button');
        button.type='button'; button.textContent=label; button.dataset.evaluationTab=key;
        button.id=prefix+'-'+key+'-tab'; panel.id=prefix+'-'+key;
        button.setAttribute('role','tab');button.setAttribute('aria-controls',panel.id);
        panel.setAttribute('role','tabpanel');panel.setAttribute('aria-labelledby',button.id);
        panel.dataset.evaluationPanel=key;
        button.addEventListener('click',()=>selectPanel(dialog,key,false));
        tabs.append(button);
      });
      tabs.addEventListener('keydown',event=>{
        if(!['ArrowLeft','ArrowRight','Home','End'].includes(event.key))return;
        event.preventDefault();
        const buttons=[...tabs.querySelectorAll('button')];
        const index=buttons.indexOf(document.activeElement);
        const next=event.key==='Home'?0:event.key==='End'?buttons.length-1:(index+(event.key==='ArrowRight'?1:-1)+buttons.length)%buttons.length;
        selectPanel(dialog,buttons[next].dataset.evaluationTab,true);
      });
      academic.before(tabs);
      selectPanel(dialog,'academic',false);
    }
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
    selectPanel(dialog,dialog.dataset.activePanel||'academic',false);
    const score = dialog.querySelector('[data-evaluation-panel]:not([hidden]) input:not(:disabled), [data-evaluation-panel]:not([hidden]) textarea:not(:disabled)');
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
    const saveButton = event.target.closest('.evaluation-dialog-actions input[type="submit"]');
    if (saveButton) {
      // Repeated dialogs share one WebForms form. Validate only the chosen
      // evaluation so invalid fields in hidden dialogs cannot block its postback.
      const dialog = saveButton.closest('.evaluation-dialog');
      const score = dialog.querySelector('input[type="number"]:not(:disabled)');
      if (score) score.setCustomValidity(score.value === '' ? 'Enter an academic score between 0 and 100.' : '');
      for (const field of dialog.querySelectorAll('input:not(:disabled), textarea:not(:disabled)')) {
        if (!field.checkValidity()) {
          const panel=field.closest('[data-evaluation-panel]');
          if(panel)selectPanel(dialog,panel.dataset.evaluationPanel,false);
          field.reportValidity();
          event.preventDefault();
          return;
        }
      }
      return;
    }
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
    const focusable = Array.from(dialog.querySelectorAll('button:not(:disabled), input:not(:disabled), textarea:not(:disabled)')).filter(field=>field.getClientRects().length>0 && field.tabIndex!==-1);
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
      score.setCustomValidity('');
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
