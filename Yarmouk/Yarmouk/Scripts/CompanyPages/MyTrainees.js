(() => {
  'use strict';
  const search = document.getElementById('traineeSearch');
  const cards = [...document.querySelectorAll('.trainee-card')];
  const noMatches = document.getElementById('traineeSearchEmpty');
  if (!search || !cards.length) return;

  search.addEventListener('input', () => {
    const query = search.value.trim().toLocaleLowerCase();
    let visible = 0;
    cards.forEach(card => {
      const match = !query || card.textContent.toLocaleLowerCase().includes(query);
      card.hidden = !match;
      if (match) visible += 1;
    });
    if (noMatches) noMatches.hidden = visible !== 0;
  });
})();
