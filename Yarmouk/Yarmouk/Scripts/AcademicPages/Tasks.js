(() => {
  const search = document.getElementById('academicTaskSearch');
  const status = document.getElementById('academicTaskStatus');
  const cards = [...document.querySelectorAll('.academic-task-card')];
  const empty = document.getElementById('academicTaskNoMatch');
  if (!search || !status) return;
  const filter = () => {
    const query = search.value.trim().toLocaleLowerCase();
    let visible = 0;
    cards.forEach(card => {
      card.hidden = !(card.textContent.toLocaleLowerCase().includes(query) && (status.value === 'all' || card.dataset.status === status.value));
      if (!card.hidden) visible++;
    });
    if (empty) empty.hidden = visible > 0 || cards.length === 0;
  };
  search.addEventListener('input', filter);
  status.addEventListener('change', filter);
})();
