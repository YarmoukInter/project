// Student task list, task details, and PDF submission modal.
(function () {
  'use strict';

  var dataNode = document.getElementById('task-data');
  var list = document.getElementById('task-list');
  if (!dataNode || !list) return;

  var tasks = [];
  try { tasks = JSON.parse(dataNode.textContent || '[]'); }
  catch (error) { tasks = []; }

  var backdrop = document.getElementById('task-modal-backdrop');
  var modal = document.getElementById('task-modal');
  var fileInput = document.getElementById('PdfUpload');
  var fileName = document.getElementById('selected-file-name');
  var previousSubmission = document.getElementById('previous-submission');
  var submissionPanel = document.getElementById('submission-panel');
  var completedPanel = document.getElementById('completed-panel');
  var closeButton = document.getElementById('close-task-modal');
  var lastFocusedElement = null;

  function text(value) { return value == null ? '' : String(value); }
  function escapeHtml(value) {
    return text(value).replace(/[&<>"']/g, function (character) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character];
    });
  }
  function dateFromUnix(value) {
    var number = Number(value || 0);
    if (!number) return '—';
    var date = new Date(number);
    if (isNaN(date.getTime())) return '—';
    return new Intl.DateTimeFormat(undefined, { day: 'numeric', month: 'short', year: 'numeric' }).format(date);
  }
  function normalizedStatus(value) {
    var status = text(value).replace(/[_\-\s]/g, '').toLowerCase();
    if (status === 'completed' || status === 'done' || status === 'approved') return 'completed';
    if (status === 'revisionrequested' || status === 'needsrevision' || status === 'resubmit') return 'revisionRequested';
    if (status === 'submitted' || status === 'underreview' || status === 'inreview' || status === 'pendingreview') return 'inReview';
    return 'assigned';
  }
  function statusLabel(task) {
    var status = normalizedStatus(task.status);
    if (status === 'completed') return 'Completed';
    if (status === 'revisionRequested') return 'Revision requested';
    if (status === 'inReview') return 'Under review';
    return 'Awaiting submission';
  }
  function statusClass(task) { return 'task-status--' + normalizedStatus(task.status); }
  function statusMarkup(task) { return '<span class="task-status ' + statusClass(task) + '">' + escapeHtml(statusLabel(task)) + '</span>'; }

  function renderTasks() {
    var count = document.getElementById('task-count');
    if (count) count.textContent = tasks.length + (tasks.length === 1 ? ' assigned task' : ' assigned tasks');
    if (!tasks.length) {
      list.innerHTML = '<div class="tasks-empty"><span class="empty-icon" aria-hidden="true">✓</span><h2>No tasks assigned yet</h2><p>New tasks from your supervisor will appear here.</p></div>';
      return;
    }
    list.innerHTML = tasks.map(function (task, index) {
      var title = text(task.title) || 'Untitled task';
      var id = text(task.id) || ('task-' + (index + 1));
      return '<article class="task-card">' +
        '<div class="task-card-top"><span class="task-card-id">' + escapeHtml(id) + '</span>' + statusMarkup(task) + '</div>' +
        '<h3>' + escapeHtml(title) + '</h3>' +
        '<p class="task-card-description">' + escapeHtml(task.description || 'No task details were provided.') + '</p>' +
        '<div class="task-card-meta"><span>Due <strong>' + escapeHtml(dateFromUnix(task.dueAtUnixMs)) + '</strong></span>' +
        '<span>Submitted <strong>' + escapeHtml(task.submittedAtUnixMs ? dateFromUnix(task.submittedAtUnixMs) : 'Not submitted') + '</strong></span>' +
        '<span>Student <strong>' + escapeHtml(task.studentName || '—') + '</strong></span>' +
        '<span>Student no. <strong>' + escapeHtml(task.studentNumber || '—') + '</strong></span></div>' +
        '<div class="task-card-bottom"><span></span><button class="task-open-button" type="button" data-task-index="' + index + '">View task →</button></div>' +
        '</article>';
    }).join('');
  }

  function openTask(task) {
    lastFocusedElement = document.activeElement;
    document.getElementById('modal-task-id').textContent = 'TASK · ' + (task.id || '');
    document.getElementById('modal-title').textContent = task.title || 'Untitled task';
    var modalStatus = document.getElementById('modal-status');
    modalStatus.className = 'task-status ' + statusClass(task);
    modalStatus.textContent = statusLabel(task);
    document.getElementById('modal-deadline').textContent = dateFromUnix(task.dueAtUnixMs);
    document.getElementById('modal-submitted').textContent = task.submittedAtUnixMs ? dateFromUnix(task.submittedAtUnixMs) : 'Not submitted';
    document.getElementById('modal-description').textContent = task.description || 'No task details were provided.';
    document.getElementById('modal-student-name').textContent = task.studentName || '—';
    document.getElementById('modal-student-number').textContent = task.studentNumber || '—';
    document.getElementById('SelectedTaskId').value = task.id || '';

    var status = normalizedStatus(task.status);
    var completed = status === 'completed';
    submissionPanel.hidden = completed;
    completedPanel.hidden = !completed;
    if (status === 'inReview') {
      document.getElementById('submission-heading').textContent = 'Send an updated submission';
      document.getElementById('submission-help').textContent = 'Your work is under review. You can upload a revised PDF if you want to update it.';
      document.getElementById('SubmitTaskButton').value = 'Send updated PDF';
    } else if (status === 'revisionRequested') {
      document.getElementById('submission-heading').textContent = 'Resubmit your work';
      document.getElementById('submission-help').textContent = 'Your supervisor requested a revision. Upload your updated PDF and add a note if needed.';
      document.getElementById('SubmitTaskButton').value = 'Resubmit PDF';
    } else {
      document.getElementById('submission-heading').textContent = 'Submit your work';
      document.getElementById('submission-help').textContent = 'Upload your completed task as a PDF and add an optional note to your supervisor.';
      document.getElementById('SubmitTaskButton').value = 'Submit PDF';
    }

    if (task.submittedAtUnixMs) {
      var previousMarkup = 'Latest submission: ' + escapeHtml(dateFromUnix(task.submittedAtUnixMs));
      if (task.notes) previousMarkup += ' · Note: ' + escapeHtml(task.notes);
      if (task.submissionId) previousMarkup += ' · <a href="' + escapeHtml(window.location.pathname + '?download=' + encodeURIComponent(task.submissionId)) + '">Download submitted PDF</a>';
      previousSubmission.innerHTML = previousMarkup;
      previousSubmission.hidden = false;
    } else {
      previousSubmission.textContent = '';
      previousSubmission.hidden = true;
    }
    if (fileInput) fileInput.value = '';
    if (fileName) fileName.textContent = 'No file selected';
    backdrop.hidden = false;
    document.body.classList.add('task-modal-open');
    modal.focus();
  }

  function closeModal() {
    backdrop.hidden = true;
    document.body.classList.remove('task-modal-open');
    if (lastFocusedElement && typeof lastFocusedElement.focus === 'function') lastFocusedElement.focus();
  }

  list.addEventListener('click', function (event) {
    var button = event.target.closest('[data-task-index]');
    if (!button) return;
    var task = tasks[Number(button.getAttribute('data-task-index'))];
    if (task) openTask(task);
  });
  closeButton.addEventListener('click', closeModal);
  backdrop.addEventListener('click', function (event) { if (event.target === backdrop) closeModal(); });
  document.addEventListener('keydown', function (event) { if (event.key === 'Escape' && !backdrop.hidden) closeModal(); });
  if (fileInput) fileInput.addEventListener('change', function () {
    if (fileName) fileName.textContent = fileInput.files && fileInput.files.length ? fileInput.files[0].name : 'No file selected';
  });

  renderTasks();
})();
