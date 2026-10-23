// Client-side validation only. Firebase reads and writes are performed by the C# page.
(function () {
  'use strict';

  var reviewModal = document.getElementById('task-review-modal');
  var reviewDialog = reviewModal && reviewModal.querySelector('.task-review-dialog');
  var reviewScore = document.getElementById('task-review-score');
  var reviewFeedback = document.getElementById('task-review-feedback');
  var reviewDeadline = document.getElementById('task-review-deadline');
  var reviewApprove = document.getElementById('task-review-approve');
  var reviewRevision = document.getElementById('task-review-revision');
  var reviewFocus = null;

  function closeReviewModal() {
    if (!reviewModal) return;
    reviewModal.hidden = true;
    document.body.classList.remove('task-review-open');
    if (reviewFocus) reviewFocus.focus();
  }

  document.addEventListener('click', function (event) {
    var trigger = event.target.closest('[data-review-id]');
    if (!trigger || !reviewModal) return;
    reviewFocus = trigger;
    var id = trigger.dataset.reviewId;
    var studentSelector = document.getElementById('StudentSelector');
    if (studentSelector && trigger.dataset.reviewStudentUid) studentSelector.value = trigger.dataset.reviewStudentUid;
    var solution = trigger.dataset.reviewSolution || '';
    document.getElementById('task-review-title').textContent = trigger.dataset.reviewTitle || 'Review task';
    document.getElementById('task-review-status').textContent = trigger.dataset.reviewStatusLabel || '';
    document.getElementById('task-review-notes').textContent = trigger.dataset.reviewNotes || 'No notes were submitted.';
    var link = document.getElementById('task-review-link');
    link.hidden = !/^https:\/\/(www\.)?(github\.com|gist\.github\.com)\//i.test(solution);
    link.href = link.hidden ? '#' : solution;
    document.getElementById('task-review-no-link').hidden = !link.hidden;
    reviewScore.value = trigger.dataset.reviewGrade || '';
    reviewFeedback.value = '';
    reviewDeadline.value = '';
    reviewApprove.value = 'complete|' + id;
    reviewRevision.value = 'revision|' + id;
    document.getElementById('task-review-actions').hidden = trigger.dataset.reviewStatus !== 'done';
    reviewModal.hidden = false;
    document.body.classList.add('task-review-open');
    reviewDialog.focus();
  });

  if (reviewModal) reviewModal.addEventListener('click', function (event) {
    if (event.target === reviewModal || event.target.closest('#close-task-review')) closeReviewModal();
  });
  document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape' && reviewModal && !reviewModal.hidden) closeReviewModal();
  });
  if (reviewApprove) reviewApprove.addEventListener('click', function (event) {
    var score = reviewScore.value.trim();
    if (score === '' || !/^\d+$/.test(score) || Number(score) < 0 || Number(score) > 10) {
      event.preventDefault();
      reviewScore.setCustomValidity('Enter a whole-number evaluation from 0 to 10.');
      reviewScore.reportValidity();
    }
  });
  if (reviewScore) reviewScore.addEventListener('input', function () { reviewScore.setCustomValidity(''); });
  if (reviewRevision) reviewRevision.addEventListener('click', function (event) {
    var validNotes = reviewFeedback.value.trim().length >= 2;
    var validDeadline = !!reviewDeadline.value && new Date(reviewDeadline.value + 'T23:59:59').getTime() > Date.now();
    reviewFeedback.setCustomValidity(validNotes ? '' : 'Enter revision notes with at least 2 characters.');
    reviewDeadline.setCustomValidity(validDeadline ? '' : 'Choose a future deadline.');
    if (!validNotes || !validDeadline) {
      event.preventDefault();
      (validNotes ? reviewDeadline : reviewFeedback).reportValidity();
    }
  });
  if (reviewFeedback) reviewFeedback.addEventListener('input', function () { reviewFeedback.setCustomValidity(''); });
  if (reviewDeadline) reviewDeadline.addEventListener('input', function () { reviewDeadline.setCustomValidity(''); });

  var addModal = document.getElementById('add-task-modal');
  var openAddTask = document.getElementById('open-add-task');
  var closeAddTask = document.getElementById('close-add-task');
  var cancelAddTask = document.getElementById('cancel-add-task');
  var deadlineInput = document.getElementById('new-task-deadline');

  function setMinimumDeadline() {
    if (!deadlineInput) return;
    var tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    var month = String(tomorrow.getMonth() + 1).padStart(2, '0');
    var day = String(tomorrow.getDate()).padStart(2, '0');
    deadlineInput.min = tomorrow.getFullYear() + '-' + month + '-' + day;
  }

  function closeAddTaskModal() {
    if (addModal) addModal.hidden = true;
  }

  if (openAddTask && addModal) {
    openAddTask.addEventListener('click', function () {
      setMinimumDeadline();
      addModal.hidden = false;
      var firstField = document.getElementById('new-task-number');
      if (firstField) firstField.focus();
    });
  }
  if (closeAddTask) closeAddTask.addEventListener('click', closeAddTaskModal);
  if (cancelAddTask) cancelAddTask.addEventListener('click', closeAddTaskModal);
  if (addModal) addModal.addEventListener('click', function (event) {
    if (event.target === addModal) closeAddTaskModal();
  });

  window.validateNewTask = function () {
    var fields = ['new-task-number', 'new-task-title', 'new-task-deadline', 'new-task-description', 'new-task-url'];
    for (var i = 0; i < fields.length; i++) {
      var field = document.getElementById(fields[i]);
      if (field && !field.checkValidity()) { field.reportValidity(); return false; }
    }
    var selected = document.querySelectorAll('input[name="targetStudentUid"]:checked');
    if (!selected.length) {
      window.alert('Select at least one trainee student.');
      return false;
    }
    if (deadlineInput) {
      var deadline = new Date(deadlineInput.value + 'T00:00:00');
      var today = new Date();
      today.setHours(0, 0, 0, 0);
      if (!deadlineInput.value || deadline <= today) {
        deadlineInput.setCustomValidity('Choose a future deadline.');
        deadlineInput.reportValidity();
        return false;
      }
      deadlineInput.setCustomValidity('');
    }
    return true;
  };

  document.querySelectorAll('input[name="targetStudentUid"]').forEach(function (checkbox) {
    checkbox.addEventListener('change', function () {
      var summary = document.querySelector('.trainee-picker summary');
      if (!summary) return;
      var count = document.querySelectorAll('input[name="targetStudentUid"]:checked').length;
      summary.textContent = count ? count + (count === 1 ? ' student selected' : ' students selected') : 'Choose one or more students';
    });
  });
})();
