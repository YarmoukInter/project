<%@ Page Title="Manage Tasks" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="ManageTasks.aspx.cs" Inherits="Yarmouk.CompanyPages.ManageTasks" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/CompanyPages/ManageTasks.css" rel="stylesheet" />
<script src="../Scripts/CompanyPages/ManageTasks.js" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<main class="manage-tasks-page">
  <header class="manage-tasks-heading">
    <p class="eyebrow">Company workspace</p>
    <h1>Manage trainee tasks</h1>
    <p>Choose a student to review their tasks, submissions, and deadlines.</p>
  </header>
  <asp:Literal ID="PageMessage" runat="server" />
  <div class="add-task-toolbar"><button id="open-add-task" class="add-task-button" type="button">+ Add task</button></div>
  <section class="student-filter" aria-labelledby="student-filter-heading">
    <label id="student-filter-heading" for="StudentSelector">Student</label>
    <asp:DropDownList ID="StudentSelector" runat="server" ClientIDMode="Static" AutoPostBack="true" />
  </section>
  <section class="tasks-table-panel" aria-labelledby="task-list-heading">
    <div class="table-heading">
      <div><h2 id="task-list-heading">Student tasks</h2><p>Tasks are ordered from oldest to newest.</p></div>
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
</asp:Content>
