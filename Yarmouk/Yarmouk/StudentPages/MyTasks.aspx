<%@ Page Title="My Tasks" Language="C#" MasterPageFile="~/StudentPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="MyTasks.aspx.cs" Inherits="Yarmouk.StudentPages.MyTasks" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/StudentPages/MyTasks.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/MyTasks.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="my-tasks-page" aria-labelledby="tasks-heading">
  <div class="tasks-heading-row">
    <div>
      <p class="tasks-eyebrow">Your internship workspace</p>
      <h1 id="tasks-heading">My Tasks</h1>
      <p class="tasks-intro">Review your assigned work, deadlines and feedback, and submit your work here.</p>
    </div>
  </div>

  <asp:Literal ID="PageMessage" runat="server" />
  <div class="tasks-section-heading"><h2>Your task board</h2><span id="task-count" class="task-count"></span></div>
  <div id="task-board" class="task-board" aria-live="polite">
    <section class="task-column task-column--todo" data-drop-status="todo" aria-labelledby="todo-heading"><header><h3 id="todo-heading">To do</h3><span class="column-count">0</span></header><div class="task-column-list"></div></section>
    <section class="task-column task-column--progress" data-drop-status="inProgress" aria-labelledby="progress-heading"><header><h3 id="progress-heading">In Progress</h3><span class="column-count">0</span></header><div class="task-column-list"></div></section>
    <section class="task-column task-column--done" data-drop-status="done" aria-labelledby="done-heading"><header><h3 id="done-heading">Done</h3><span class="column-count">0</span></header><div class="task-column-list"></div></section>
    <section class="task-column task-column--complete" data-drop-status="completed" aria-labelledby="complete-heading"><header><h3 id="complete-heading">Complete</h3><span class="column-count">0</span></header><div class="task-column-list"></div></section>
  </div>
  <div id="task-data" hidden><asp:Literal ID="TaskData" runat="server" /></div>
</section>

<div id="task-modal-backdrop" class="task-modal-backdrop" hidden>
  <section id="task-modal" class="task-modal" role="dialog" aria-modal="true" aria-labelledby="modal-title" tabindex="-1">
    <button id="close-task-modal" class="task-modal-close" type="button" aria-label="Close task details">×</button>
    <div class="task-modal-content">
      <p id="modal-task-id" class="tasks-eyebrow"></p>
      <h2 id="modal-title"></h2>
      <div id="modal-status" class="task-status"></div>
      <div class="task-meta-grid">
        <div><span>Deadline</span><strong id="modal-deadline"></strong></div>
        <div><span>Submitted</span><strong id="modal-submitted"></strong></div>
      </div>
      <div class="task-description"><span>Task details</span><p id="modal-description"></p></div>
      <div id="task-reference-pdf" class="task-reference-pdf" hidden><strong>Task requirements PDF</strong><a id="task-reference-pdf-link" href="#" target="_blank" rel="noopener">Open PDF</a></div>
      <div id="previous-submission" class="previous-submission" hidden></div>
      <div id="review-feedback" class="review-feedback" hidden><strong>Supervisor feedback</strong><p id="review-feedback-text"></p></div>
      <button id="StartTaskButton" name="StartTaskAction" value="1" type="submit" class="submit-task-button start-task-button">Start task</button>
      <div id="submission-panel" class="submission-panel">
        <h3 id="submission-heading">Submit your work</h3>
        <p id="submission-help"></p>
        <asp:HiddenField ID="SelectedTaskId" runat="server" ClientIDMode="Static" />
        <asp:HiddenField ID="TargetTaskStatus" runat="server" ClientIDMode="Static" />
        <button id="MoveTaskButton" name="MoveTaskAction" value="1" type="submit" class="task-move-postback" style="position:absolute;left:-10000px;width:1px;height:1px;">Move task</button>
        <label class="notes-label" for="GitHubSolutionUrl">GitHub solution URL <span>(required)</span></label>
        <asp:TextBox ID="GitHubSolutionUrl" runat="server" ClientIDMode="Static" TextMode="Url" CssClass="submission-url" placeholder="https://github.com/username/repository" MaxLength="2048" required="required" />
        <p class="submission-field-help">Paste the GitHub link to your solution.</p>
        <label class="notes-label" for="SubmissionNotes">Note to your supervisor <span>(required)</span></label>
        <asp:TextBox ID="SubmissionNotes" runat="server" ClientIDMode="Static" TextMode="MultiLine" Rows="3" CssClass="submission-notes" placeholder="Add a note about your submission..." MaxLength="1000" required="required" />
        <asp:Button ID="SubmitTaskButton" runat="server" ClientIDMode="Static" CssClass="submit-task-button" Text="Submit task" OnClick="SubmitTask_Click" />
      </div>
      <div id="completed-panel" class="completed-panel" hidden><strong>Task completed</strong><p>This task has been marked complete. No further submission is needed.</p></div>
    </div>
  </section>
</div>

</asp:Content>
