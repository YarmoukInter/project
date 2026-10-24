(() => {
  'use strict';
  const search = document.getElementById('progressSearch');
  const cards = [...document.querySelectorAll('.student-progress-card')];
  const noMatches = document.getElementById('progressSearchEmpty');
  const infoModal = document.getElementById('studentInfoModal');
  if (infoModal) {
    let infoOpener = null;
    const closeInfo = () => { infoModal.hidden = true; document.body.classList.remove('progress-note-open'); if (infoOpener) infoOpener.focus(); };
    document.querySelectorAll('[data-open-student-info]').forEach(button => button.addEventListener('click', () => {
      infoOpener = button;
      const photo = infoModal.querySelector('[data-progress-dialog-photo]');
      if (photo) {
        photo.dataset.studentPhotoUrl = button.dataset.profileImageUrl || '';
        photo.removeAttribute('data-student-photo-ready');
        photo.querySelectorAll('.uims-student-photo-image').forEach(image => image.remove());
        const initials = photo.querySelector('[data-progress-dialog-initials]');
        if (initials) initials.textContent = button.dataset.initials || 'ST';
        window.UimsStudentPhotos && window.UimsStudentPhotos.enhance(infoModal);
      }
      infoModal.querySelectorAll('[data-info-field]').forEach(field => { field.textContent = button.dataset[field.dataset.infoField] || 'Not provided'; });
      infoModal.hidden = false;
      document.body.classList.add('progress-note-open');
      infoModal.querySelector('[data-close-student-info]').focus();
    }));
    infoModal.addEventListener('click', event => { if (event.target === infoModal || event.target.closest('[data-close-student-info]')) closeInfo(); });
    document.addEventListener('keydown', event => { if (event.key === 'Escape' && !infoModal.hidden) closeInfo(); });
  }
  if (search && cards.length) {
    const filter = () => {
      const query = search.value.trim().toLocaleLowerCase();
      let visible = 0;
      cards.forEach(card => {
        const match = !query || card.textContent.toLocaleLowerCase().includes(query);
        card.hidden = !match;
        if (match) visible += 1;
      });
      if (noMatches) noMatches.hidden = visible !== 0;
    };
    search.addEventListener('input', filter);
  }

  const hoursModal = document.getElementById('approvedHoursModal');
  const hoursPlacementField = document.getElementById('HoursPlacementId');
  const hoursField = document.getElementById('ApprovedHoursInput');
  const hoursNoteField = document.getElementById('ApprovedHoursNote');
  const hoursInlineError = document.getElementById('approvedHoursInlineError');
  if (hoursModal && hoursPlacementField && hoursField) {
    let hoursOpener = null;
    const closeHours = () => {
      hoursModal.hidden = true;
      document.body.classList.remove('progress-note-open');
      if (hoursOpener) hoursOpener.focus();
    };
    document.querySelectorAll('[data-open-approved-hours]').forEach(button => {
      button.addEventListener('click', () => {
        hoursOpener = button;
        hoursPlacementField.value = button.dataset.placementId || '';
        hoursField.value = '';
        if (hoursNoteField) hoursNoteField.value = '';
        if (hoursInlineError) hoursInlineError.hidden = true;
        const current = button.dataset.currentHours || '0';
        const required = button.dataset.requiredHours || 'not set';
        document.getElementById('approvedHoursStudent').textContent =
          'For ' + (button.dataset.studentName || 'this student') + ' · currently ' + current + ' of ' + required + ' approved hours.';
        hoursModal.hidden = false;
        document.body.classList.add('progress-note-open');
        hoursField.focus();
      });
    });
    hoursModal.addEventListener('click', event => {
      if (event.target === hoursModal || event.target.closest('[data-close-approved-hours]')) closeHours();
    });
    document.addEventListener('keydown', event => {
      if (event.key === 'Escape' && !hoursModal.hidden) closeHours();
    });
    if (hoursPlacementField.value) {
      const pageError = document.querySelector('.progress-workspace .workspace-alert--error');
      if (hoursInlineError && pageError) { hoursInlineError.textContent = pageError.textContent.trim(); hoursInlineError.hidden = false; }
      hoursModal.hidden = false;
      document.body.classList.add('progress-note-open');
    }
  }

  const modal = document.getElementById('progressNoteModal');
  const placementField = document.getElementById('NotePlacementId');
  const bodyField = document.getElementById('NoteBody');
  const inlineError = document.getElementById('progressNoteInlineError');
  if (!modal || !placementField || !bodyField) return;
  let opener = null;
  const close = () => {
    modal.hidden = true;
    document.body.classList.remove('progress-note-open');
    if (opener) opener.focus();
  };
  document.querySelectorAll('[data-open-progress-note]').forEach(button => {
    button.addEventListener('click', () => {
      opener = button;
      placementField.value = button.dataset.placementId || '';
      bodyField.value = '';
      if (inlineError) inlineError.hidden = true;
      document.getElementById('progressNoteStudent').textContent = 'For ' + (button.dataset.studentName || 'this student');
      modal.hidden = false;
      document.body.classList.add('progress-note-open');
      bodyField.focus();
    });
  });
  modal.addEventListener('click', event => {
    if (event.target === modal || event.target.closest('[data-close-progress-note]')) close();
  });
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape' && !modal.hidden) close();
  });
  if (placementField.value) {
    const pageError = document.querySelector('.progress-workspace .workspace-alert--error');
    if (inlineError && pageError) { inlineError.textContent = pageError.textContent.trim(); inlineError.hidden = false; }
    modal.hidden = false;
    document.body.classList.add('progress-note-open');
  }
})();
