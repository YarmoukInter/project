<%@ Page Title="Manage Tasks" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="ManageTasks.aspx.cs" Inherits="Yarmouk.CompanyPages.ManageTasks" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<script src="../Scripts/CompanyPages/ManageTasks.js?v=review-modal-1" defer></script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<script src="../Scripts/Shared/record-lists.js?v=record-cards-3" defer></script>
<link href="../Content/CompanyPages/ManageTasks.css?v=filter-icon-center-1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<main class="manage-tasks-page">
  <header class="manage-tasks-heading">
    <p class="eyebrow">Company workspace</p>
    <h1>Manage trainee tasks</h1>
    <p>View every trainee task, then choose a student to narrow the list.</p>
  </header>
  <asp:Literal ID="PageMessage" runat="server" />
  <div class="add-task-toolbar"><button id="open-add-task" class="add-task-button" type="button">+ Add task</button></div>
  <section class="student-filter task-filter-panel" aria-labelledby="student-filter-heading">
    <div class="filter-heading"><div><span class="filter-icon" aria-hidden="true">⌕</span><div><strong id="student-filter-heading">Filter company tasks</strong><span>Search across every trainee task or narrow the list by student and status.</span></div></div><a href='<%= ResolveUrl("~/CompanyPages/ManageTasks") %>' class="clear-task-filters">Clear all</a></div>
    <div class="task-filter-grid">
      <label>Search tasks<asp:TextBox ID="TaskSearchTextBox" runat="server" TextMode="Search" placeholder="Student, task number, or title" /></label>
      <label>Student<asp:DropDownList ID="StudentSelector" runat="server" ClientIDMode="Static" /></label>
      <label>Status<asp:DropDownList ID="TaskStatusSelector" runat="server"><asp:ListItem Value="">All statuses</asp:ListItem><asp:ListItem Value="todo">To do</asp:ListItem><asp:ListItem Value="inProgress">In progress</asp:ListItem><asp:ListItem Value="done">Awaiting review</asp:ListItem><asp:ListItem Value="completed">Completed</asp:ListItem></asp:DropDownList></label>
    </div>
    <div class="task-filter-actions"><span>Filters are applied to the complete company task list.</span><button type="submit" class="apply-task-filters" name="filterTasksAction" value="1" formnovalidate>Apply filters</button></div>
  </section>
  <section class="tasks-table-panel" aria-labelledby="task-list-heading">
    <div class="table-heading">
      <div><h2 id="task-list-heading">Trainee tasks</h2><p><asp:Literal ID="TaskListContext" runat="server" /></p></div>
      <asp:Literal ID="TaskCount" runat="server" />
    </div>
    <asp:Literal ID="TaskTable" runat="server" />
  </section>
</main>

<div id="add-task-modal" class="add-task-modal" hidden>
  <section class="add-task-dialog" role="dialog" aria-modal="true" aria-labelledby="add-task-title" tabindex="-1">
    <button id="close-add-task" class="close-add-task" type="button" aria-label="Close">×</button>
    <p class="eyebrow">New assignment</p>
    <h2 id="add-task-title">Add task</h2>
    <p class="dialog-intro">The task will be added to each selected student's To do column.</p>
    <div class="add-task-form">
      <label for="new-task-number">Task number</label>
      <input id="new-task-number" name="newTaskNumber" maxlength="40" pattern="[A-Za-z0-9_-]+" placeholder="TASK003" required />
      <label for="new-task-title">Task title</label>
      <input id="new-task-title" name="newTaskTitle" maxlength="160" placeholder="Task title" required />
      <label for="new-task-deadline">Deadline</label>
      <input id="new-task-deadline" name="newTaskDeadline" type="date" required />
      <label for="new-task-description">What should the student do?</label>
      <textarea id="new-task-description" name="newTaskDescription" maxlength="4000" rows="4" placeholder="Describe the task requirements" required></textarea>
      <label for="new-task-url">Task link (PDF or instructions)</label>
      <input id="new-task-url" name="newTaskUrl" type="url" maxlength="2048" placeholder="https://..." required />
      <label>Select trainee students</label>
      <details class="trainee-picker">
        <summary>Choose one or more students</summary>
        <div class="trainee-picker-list"><asp:Literal ID="StudentChoices" runat="server" /></div>
      </details>
      <div class="dialog-actions">
        <button class="cancel-add-task" id="cancel-add-task" type="button">Cancel</button>
        <button class="save-add-task" type="submit" name="addTaskAction" value="1" onclick="return validateNewTask()">Add task</button>
      </div>
    </div>
  </section>
</div>
<div id="task-review-modal" class="task-review-modal" hidden>
  <section class="task-review-dialog" role="dialog" aria-modal="true" aria-labelledby="task-review-title" tabindex="-1">
    <button id="close-task-review" class="close-task-review" type="button" aria-label="Close review">×</button>
    <p class="eyebrow">Submission review</p>
    <h2 id="task-review-title">Review task</h2>
    <p id="task-review-status" class="task-review-status"></p>
    <div class="task-review-info"><strong>Student solution</strong><a id="task-review-link" target="_blank" rel="noopener noreferrer" hidden>Open GitHub solution ↗</a><span id="task-review-no-link">No GitHub link saved</span></div>
    <div class="task-review-info"><strong>Student notes</strong><p id="task-review-notes"></p></div>
    <div id="task-review-actions" class="task-review-actions">
      <label for="task-review-score">Task evaluation <span>(out of 10)</span></label>
      <input id="task-review-score" name="taskScore" type="number" min="0" max="10" step="1" inputmode="numeric" placeholder="0–10" />
      <button id="task-review-approve" class="approve-button" type="submit" name="taskAction" value="" formnovalidate>Approve and complete</button>
      <div class="task-review-divider"><span>or request changes</span></div>
      <label for="task-review-feedback">What should the student change?</label>
      <textarea id="task-review-feedback" name="reviewFeedback" maxlength="2000" rows="3" placeholder="Write clear revision notes"></textarea>
      <label for="task-review-deadline">New deadline</label>
      <input id="task-review-deadline" name="reviewDeadline" type="date" />
      <button id="task-review-revision" class="revision-button" type="submit" name="taskAction" value="" formnovalidate>Request changes</button>
    </div>
  </section>
</div>
</asp:Content>

