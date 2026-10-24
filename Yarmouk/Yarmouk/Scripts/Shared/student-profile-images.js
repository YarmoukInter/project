(() => {
  'use strict';

  function enhance(root) {
    (root || document).querySelectorAll('[data-student-photo-url]:not([data-student-photo-ready])').forEach(avatar => {
      avatar.dataset.studentPhotoReady = 'true';
      const url = (avatar.dataset.studentPhotoUrl || '').trim();
      if (!url) return;

      let imageUrl;
      try {
        imageUrl = new URL(url, document.baseURI);
        if (imageUrl.origin !== location.origin) return;
      } catch (_) { return; }

      const image = document.createElement('img');
      image.className = 'uims-student-photo-image';
      image.alt = '';
      image.decoding = 'async';
      image.addEventListener('error', () => { image.remove(); avatar.classList.add('uims-student-photo-failed'); }, { once: true });
      image.src = imageUrl.href;
      avatar.append(image);
    });
  }

  window.UimsStudentPhotos = { enhance };
  if (document.readyState === 'loading')
    document.addEventListener('DOMContentLoaded', () => enhance(document), { once: true });
  else enhance(document);
})();
