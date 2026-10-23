(() => {
  'use strict';

  const search = document.getElementById('traineeSearch');
  const statusFilter = document.getElementById('traineeStatusFilter');
  const cards = [...document.querySelectorAll('.trainee-card')];
  const noMatches = document.getElementById('traineeSearchEmpty');
  const visibleCount = document.getElementById('traineeVisibleCount');
  const dialog = document.getElementById('traineeInfoDialog');
  const detailFields = dialog ? [...dialog.querySelectorAll('[data-profile-field]')] : [];

  const matchesStatus = (row, filter) => {
    if (filter === 'all') return true;
    const status = (row.dataset.traineeStatus || '').replace(/[ _-]/g, '').toLocaleLowerCase();
    const isCompleted = status === 'completed' || status === 'complete' || status === 'closed';
    return filter === 'completed' ? isCompleted : !isCompleted;
  };

  const normalize = value => (value || '').toLocaleLowerCase().normalize('NFKD').replace(/[\u0300-\u036f]/g, '').trim();
  const filterRows = () => {
    const query = search ? normalize(search.value) : '';
    const filter = statusFilter ? statusFilter.value : 'all';
    let visible = 0;
    cards.forEach(card => {
      const matchesText = !query || normalize(card.dataset.search || card.textContent).includes(query);
      const matchesSelectedStatus = matchesStatus(card, filter);
      card.hidden = !(matchesText && matchesSelectedStatus);
      if (!card.hidden) visible += 1;
    });
    if (noMatches) noMatches.hidden = visible !== 0 || cards.length === 0;
    if (visibleCount) visibleCount.textContent = String(visible);
  };

  if (search) search.addEventListener('input', filterRows);
  const initialQuery = new URLSearchParams(location.search).get('search') || new URLSearchParams(location.search).get('q');
  if (search && initialQuery) search.value = initialQuery;
  if (statusFilter) statusFilter.addEventListener('change', filterRows);
  filterRows();

  if (!dialog) return;

  document.querySelectorAll('[data-info-trigger]').forEach(button => {
    button.addEventListener('click', () => {
      const values = {
        name: button.dataset.name,
        number: button.dataset.number,
        email: button.dataset.email,
        phone: button.dataset.phone,
        major: button.dataset.major,
        academicStatus: button.dataset.academicStatus,
        gpa: button.dataset.gpa
      };
      detailFields.forEach(field => {
        field.textContent = values[field.dataset.profileField] || 'Not provided';
      });
      if (typeof dialog.showModal === 'function') dialog.showModal();
      else dialog.setAttribute('open', 'open');
    });
  });

  dialog.querySelectorAll('[data-dialog-close]').forEach(button => {
    button.addEventListener('click', () => {
      if (typeof dialog.close === 'function') dialog.close();
      else dialog.removeAttribute('open');
    });
  });

  dialog.addEventListener('click', event => {
    if (event.target === dialog) {
      if (typeof dialog.close === 'function') dialog.close();
      else dialog.removeAttribute('open');
    }
  });
})();
