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
    <div class="tasks-student-card" aria-label="Student information">
      <span class="tasks-student-avatar" aria-hidden="true">ST</span>
      <span><strong><asp:Literal ID="StudentName" runat="server" /></strong><small>Student no. <asp:Literal ID="StudentNumber" runat="server" /></small></span>
    </div>
  </div>

  <asp:Literal ID="PageMessage" runat="server" />
  <div class="tasks-section-heading"><h2>Your assigned work</h2><span id="task-count" class="task-count"></span></div>
  <div id="task-list" class="task-list" aria-live="polite"></div>
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
        <div><span>Student</span><strong id="modal-student-name"></strong></div>
        <div><span>Student number</span><strong id="modal-student-number"></strong></div>
      </div>
      <div class="task-description"><span>Task details</span><p id="modal-description"></p></div>
      <div id="previous-submission" class="previous-submission" hidden></div>
      <div id="submission-panel" class="submission-panel">
        <h3 id="submission-heading">Submit your work</h3>
        <p id="submission-help"></p>
        <asp:HiddenField ID="SelectedTaskId" runat="server" ClientIDMode="Static" />
        <label class="pdf-upload" for="PdfUpload">
          <span class="upload-icon" aria-hidden="true">↑</span>
          <span><strong>Choose a PDF file</strong><small>PDF only · up to 3 MB</small></span>
        </label>
        <asp:FileUpload ID="PdfUpload" runat="server" ClientIDMode="Static" CssClass="pdf-input" accept="application/pdf,.pdf" />
        <p id="selected-file-name" class="selected-file-name">No file selected</p>
        <label class="notes-label" for="SubmissionNotes">Note to your supervisor <span>(optional)</span></label>
        <asp:TextBox ID="SubmissionNotes" runat="server" ClientIDMode="Static" TextMode="MultiLine" Rows="3" CssClass="submission-notes" placeholder="Add a note about your submission..." MaxLength="1000" />
        <asp:Button ID="SubmitTaskButton" runat="server" ClientIDMode="Static" CssClass="submit-task-button" Text="Submit PDF" OnClick="SubmitTask_Click" />
      </div>
      <div id="completed-panel" class="completed-panel" hidden><strong>Task completed</strong><p>This task has been marked complete. No further submission is needed.</p></div>
    </div>
  </section>
</div>

</asp:Content>
