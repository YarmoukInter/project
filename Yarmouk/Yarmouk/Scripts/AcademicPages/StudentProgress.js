(() => {
  'use strict';
  const search = document.getElementById('progressSearch');
  const cards = [...document.querySelectorAll('.student-progress-card')];
  const noMatches = document.getElementById('progressSearchEmpty');
  if (!search || !cards.length) return;

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
})();
