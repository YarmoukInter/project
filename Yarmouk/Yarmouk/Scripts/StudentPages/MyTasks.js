// Student task list, task details, and PDF submission modal.
(function () {
  'use strict';

  var dataNode = document.getElementById('task-data');
  var board = document.getElementById('task-board');
  if (!dataNode || !board) return;

  var tasks = [];
  try { tasks = JSON.parse(dataNode.textContent || '[]'); }
  catch (error) { tasks = []; }
  var boardSearch = document.getElementById('task-board-search');
  var boardSort = document.getElementById('task-board-sort');
  var visibleLimits = {};

  var backdrop = document.getElementById('task-modal-backdrop');
  var modal = document.getElementById('task-modal');
  var previousSubmission = document.getElementById('previous-submission');
  var githubSolutionUrlInput = document.getElementById('GitHubSolutionUrl');
  var submissionPanel = document.getElementById('submission-panel');
  var completedPanel = document.getElementById('completed-panel');
  var taskReferencePdf = document.getElementById('task-reference-pdf');
  var taskReferencePdfLink = document.getElementById('task-reference-pdf-link');
  var closeButton = document.getElementById('close-task-modal');
  var lastFocusedElement = null;

  function text(value) { return value == null ? '' : String(value); }
  function escapeHtml(value) {
    return text(value).replace(/[&<>"']/g, function (character) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character];
    });
  }
  function safeTaskPdfUrl(value) {
    if (!value) return '';
    try {
      var url = new URL(String(value), window.location.href);
      return url.protocol === 'https:' || url.origin === window.location.origin ? url.href : '';
    } catch (error) { return ''; }
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
    if (status === 'completed' || status === 'complete' || status === 'approved') return 'completed';
    if (status === 'done' || status === 'submitted' || status === 'underreview' || status === 'inreview' || status === 'pendingreview') return 'done';
    if (status === 'inprogress' || status === 'progress') return 'inProgress';
    if (status === 'revision' || status === 'revisionrequested' || status === 'needsrevision' || status === 'resubmit' || status === 'todo' || status === 'assigned' || status === 'pending') return 'todo';
    return 'todo';
  }
  function statusLabel(task) {
    var status = normalizedStatus(task.status);
    if (status === 'completed') return 'Completed';
    if (status === 'todo' && task.reviewNotes) return 'Revision requested';
    if (status === 'todo') return 'To do';
    if (status === 'done') return 'Done · Under review';
    if (status === 'inProgress') return 'In Progress';
    return 'To do';
  }
  function statusClass(task) { return 'task-status--' + normalizedStatus(task.status); }
  function statusMarkup(task) { return '<span class="task-status ' + statusClass(task) + '">' + escapeHtml(statusLabel(task)) + '</span>'; }

  function renderTasks() {
    var query = boardSearch ? boardSearch.value.trim().toLocaleLowerCase() : '';
    var matching = tasks.filter(function (task) { return !query || (text(task.title) + ' ' + text(task.description)).toLocaleLowerCase().includes(query); });
    var count = document.getElementById('task-count');
    if (count) count.textContent = query ? matching.length + ' of ' + tasks.length + ' tasks match' : tasks.length + (tasks.length === 1 ? ' assigned task' : ' assigned tasks');
    var columns = Array.prototype.slice.call(board.querySelectorAll('.task-column'));
    var grouped = { todo: [], inProgress: [], done: [], completed: [] };
    matching.forEach(function (task) { grouped[normalizedStatus(task.status)].push(task); });
    columns.forEach(function (column) {
      var status = column.getAttribute('data-drop-status');
      var items = grouped[status] || [];
      items.sort(function (a, b) {
        if (boardSort && boardSort.value === 'title') return text(a.title).localeCompare(text(b.title), undefined, {numeric:true,sensitivity:'base'});
        if (boardSort && boardSort.value === 'newest') return Number(b.createdAtUnixMs || 0) - Number(a.createdAtUnixMs || 0);
        return (Number(a.dueAtUnixMs) || Infinity) - (Number(b.dueAtUnixMs) || Infinity);
      });
      var columnList = column.querySelector('.task-column-list');
      column.querySelector('.column-count').textContent = String(items.length);
      if (!items.length) {
        var emptyText = query ? 'No matching tasks in this status' : status === 'done' ? 'Submitted tasks appear here' : (status === 'completed' ? 'Approved tasks appear here' : 'Drop a task here');
        columnList.innerHTML = '<div class="task-column-empty">' + emptyText + '</div>';
        return;
      }
      var limit = visibleLimits[status] || 5;
      columnList.innerHTML = items.slice(0, limit).map(function (task) {
      var index = tasks.indexOf(task);
      var title = text(task.title) || 'Untitled task';
      var id = text(task.id) || ('task-' + (index + 1));
      var statusName = normalizedStatus(task.status);
      var canDrag = statusName === 'todo' || statusName === 'inProgress';
      return '<article class="task-card' + (canDrag ? ' task-card--draggable' : '') + '"' + (canDrag ? ' draggable="true"' : '') + ' data-task-id="' + escapeHtml(id) + '">' +
        '<div class="task-card-top"><span class="task-card-id">INTERNSHIP TASK</span>' + statusMarkup(task) + '</div>' +
        '<h3>' + escapeHtml(title) + '</h3>' +
        '<p class="task-card-description">' + escapeHtml(task.description || 'No task details were provided.') + '</p>' +
        '<div class="task-card-meta"><span>Due <strong>' + escapeHtml(dateFromUnix(task.dueAtUnixMs)) + '</strong></span>' +
        '<span>Submitted <strong>' + escapeHtml(task.submittedAtUnixMs ? dateFromUnix(task.submittedAtUnixMs) : 'Not submitted') + '</strong></span>' +

        '</div>' +
        '<div class="task-card-bottom"><button class="task-open-button" type="button" data-task-index="' + index + '">View task →</button></div>' +
        '</article>';
      }).join('');
      if (items.length > limit) columnList.insertAdjacentHTML('beforeend', '<button type="button" class="task-board-more" data-more-status="' + status + '">Show ' + Math.min(5,items.length-limit) + ' more <span>(' + (items.length-limit) + ' remaining)</span></button>');
    });
  }

  function openTask(task) {
    lastFocusedElement = document.activeElement;
    document.getElementById('modal-task-id').textContent = 'INTERNSHIP TASK';
    document.getElementById('modal-title').textContent = task.title || 'Untitled task';
    var modalStatus = document.getElementById('modal-status');
    modalStatus.className = 'task-status ' + statusClass(task);
    modalStatus.textContent = statusLabel(task);
    document.getElementById('modal-deadline').textContent = dateFromUnix(task.dueAtUnixMs);
    document.getElementById('modal-submitted').textContent = task.submittedAtUnixMs ? dateFromUnix(task.submittedAtUnixMs) : 'Not submitted';
    document.getElementById('modal-created').textContent = dateFromUnix(task.createdAtUnixMs);
    document.getElementById('modal-file-name').textContent = task.taskPdfUrl ? (task.taskPdfFileName || 'Task requirements') : 'No file attached';
    document.getElementById('modal-description').textContent = task.description || 'No task details were provided.';
    document.getElementById('modal-submission-notes').textContent = task.notes || 'No notes submitted.';
    var solutionLink = document.getElementById('modal-submission-link');
    var solutionUrl = safeTaskPdfUrl(task.submissionUrl);
    solutionLink.innerHTML = solutionUrl ? '<a href="' + escapeHtml(solutionUrl) + '" target="_blank" rel="noopener noreferrer">Open submitted link ↗</a>' : 'No link submitted.';
    var taskPdfUrl = safeTaskPdfUrl(task.taskPdfUrl);
    if (taskPdfUrl && taskReferencePdf && taskReferencePdfLink) {
      taskReferencePdfLink.href = taskPdfUrl;
      taskReferencePdfLink.textContent = task.taskPdfFileName || 'Open task requirements PDF';
      taskReferencePdf.hidden = false;
    } else if (taskReferencePdf) { taskReferencePdf.hidden = true; }
    document.getElementById('SelectedTaskId').value = task.id || '';
    if (githubSolutionUrlInput) githubSolutionUrlInput.value = task.submissionUrl || '';

    var status = normalizedStatus(task.status);
    var completed = status === 'completed';
    var revisionRequested = status === 'todo' && !!task.reviewNotes;
    var startButton = document.getElementById('StartTaskButton');
    startButton.hidden = completed || status === 'inProgress' || status === 'done' || revisionRequested;
    startButton.disabled = completed;

    submissionPanel.hidden = completed || status === 'done' || (status === 'todo' && !revisionRequested);
    completedPanel.hidden = !completed;
    if (status === 'todo' && task.reviewNotes) {
      document.getElementById('submission-heading').textContent = 'Resubmit your work';
      document.getElementById('submission-help').textContent = 'Your supervisor requested changes. Update your GitHub solution, then resubmit the link and a note.';
      document.getElementById('SubmitTaskButton').value = 'Resubmit task';
    } else {
      document.getElementById('submission-heading').textContent = 'Submit your work';
      document.getElementById('submission-help').textContent = 'Add your GitHub solution link and a note for your supervisor before the deadline.';
      document.getElementById('SubmitTaskButton').value = 'Submit task';
    }

    var feedback = document.getElementById('review-feedback');
    var feedbackText = document.getElementById('review-feedback-text');
    feedbackText.textContent = task.reviewNotes || 'No supervisor feedback yet.';
    feedback.hidden = false;

    if (task.submittedAtUnixMs) {
      var previousMarkup = 'Latest submission: ' + escapeHtml(dateFromUnix(task.submittedAtUnixMs));
      var submittedUrl = solutionUrl;
      if (task.submissionId && !submittedUrl) previousMarkup += ' · <a href="' + escapeHtml(window.location.pathname + '?download=' + encodeURIComponent(task.submissionId)) + '">Download submitted PDF</a>';
      previousSubmission.innerHTML = previousMarkup;
      previousSubmission.hidden = false;
    } else {
      previousSubmission.textContent = '';
      previousSubmission.hidden = true;
    }
    backdrop.hidden = false;
    document.body.classList.add('task-modal-open');
    modal.focus();
  }

  function closeModal() {
    backdrop.hidden = true;
    document.body.classList.remove('task-modal-open');
    if (lastFocusedElement && typeof lastFocusedElement.focus === 'function') lastFocusedElement.focus();
  }

  board.addEventListener('click', function (event) {
    var more = event.target.closest('[data-more-status]');
    if (more) {
      var moreStatus = more.getAttribute('data-more-status');
      var previousLimit = visibleLimits[moreStatus] || 5;
      visibleLimits[moreStatus] = previousLimit + 5;
      renderTasks();
      var column = board.querySelector('[data-drop-status="' + moreStatus + '"]');
      var newTaskButton = column && column.querySelectorAll('[data-task-index]')[previousLimit];
      if (newTaskButton) newTaskButton.focus({preventScroll:true});
      return;
    }
    var button = event.target.closest('[data-task-index]');
    if (!button) return;
    var task = tasks[Number(button.getAttribute('data-task-index'))];
    if (task) openTask(task);
  });
  var draggedTaskId = '';
  if (boardSearch) boardSearch.addEventListener('input', function () { visibleLimits = {}; renderTasks(); });
  if (boardSearch) boardSearch.addEventListener('keydown', function (event) { if (event.key === 'Enter') event.preventDefault(); });
  if (boardSort) boardSort.addEventListener('change', function () { visibleLimits = {}; renderTasks(); });
  board.addEventListener('dragstart', function (event) {
    var card = event.target.closest('[data-task-id][draggable="true"]');
    if (!card) return;
    draggedTaskId = card.getAttribute('data-task-id');
    event.dataTransfer.effectAllowed = 'move';
    event.dataTransfer.setData('text/plain', draggedTaskId);
    window.setTimeout(function () { card.classList.add('task-card--dragging'); }, 0);
  });
  board.addEventListener('dragend', function (event) {
    var card = event.target.closest('[data-task-id]');
    if (card) card.classList.remove('task-card--dragging');
    board.querySelectorAll('.task-column--drag-over').forEach(function (column) { column.classList.remove('task-column--drag-over'); });
    draggedTaskId = '';
  });
  board.addEventListener('dragover', function (event) {
    var column = event.target.closest('[data-drop-status]');
    if (!column || !draggedTaskId) return;
    event.preventDefault();
    column.classList.add('task-column--drag-over');
    event.dataTransfer.dropEffect = column.getAttribute('data-drop-status') === 'todo' || column.getAttribute('data-drop-status') === 'inProgress' ? 'move' : 'none';
  });
  board.addEventListener('dragleave', function (event) {
    var column = event.target.closest('[data-drop-status]');
    if (column && !column.contains(event.relatedTarget)) column.classList.remove('task-column--drag-over');
  });
  board.addEventListener('drop', function (event) {
    var column = event.target.closest('[data-drop-status]');
    if (!column || !draggedTaskId) return;
    event.preventDefault();
    var targetStatus = column.getAttribute('data-drop-status');
    column.classList.remove('task-column--drag-over');
    if (targetStatus !== 'todo' && targetStatus !== 'inProgress') {
      window.alert('Submit your GitHub solution and note to move a task to Done. Only the supervisor can mark it Complete.');
      return;
    }
    var task = tasks.find(function (item) { return text(item.id) === draggedTaskId; });
    if (!task || normalizedStatus(task.status) === targetStatus) return;
    document.getElementById('SelectedTaskId').value = draggedTaskId;
    document.getElementById('TargetTaskStatus').value = targetStatus;
    document.getElementById('MoveTaskButton').click();
  });
  closeButton.addEventListener('click', closeModal);
  backdrop.addEventListener('click', function (event) { if (event.target === backdrop) closeModal(); });
  document.addEventListener('keydown', function (event) { if (event.key === 'Escape' && !backdrop.hidden) closeModal(); });

  window.setInterval(function () {
    if (document.visibilityState === 'hidden') return;
    fetch(window.location.pathname + '?poll=1', { credentials: 'same-origin', headers: { 'Accept': 'application/json' } })
      .then(function (response) { if (!response.ok) throw new Error('Task refresh failed'); return response.json(); })
      .then(function (latest) {
        if (!Array.isArray(latest)) return;
        tasks = latest;
        renderTasks();
      })
      .catch(function () { /* Keep the last successfully loaded task list visible. */ });
  }, 15000);

  renderTasks();
})();
