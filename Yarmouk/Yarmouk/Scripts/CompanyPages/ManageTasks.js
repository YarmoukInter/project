// Client-side validation only. Firebase reads and writes are performed by the C# page.
(function () {
  'use strict';

  window.confirmTaskApproval = function () {
    return window.confirm('Accept this submission and mark the task Completed?');
  };

  window.validateRevision = function (button) {
    var row = button.closest('tr');
    if (!row) return false;
    var notes = row.querySelector('textarea');
    var deadline = row.querySelector('input[type="date"]');
    if (!notes || !deadline) return false;

    var validNotes = notes.value.trim().length >= 2;
    var validDeadline = !!deadline.value && new Date(deadline.value + 'T23:59:59').getTime() > Date.now();
    notes.setCustomValidity(validNotes ? '' : 'Enter the requested changes (at least 2 characters).');
    deadline.setCustomValidity(validDeadline ? '' : 'Choose a future deadline.');
    if (!validNotes) { notes.reportValidity(); return false; }
    if (!validDeadline) { deadline.reportValidity(); return false; }
    return window.confirm('Return this task to the student with the new deadline?');
  };

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
