<%@ Page Title="Student Progress Monitor" Language="C#" MasterPageFile="~/AcademicPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="StudentProgress.aspx.cs" Inherits="Yarmouk.AcademicPages.StudentProgress" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/AcademicPages/StudentProgress.css?v=progress-notes-1") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/AcademicPages/StudentProgress.js?v=progress-notes-1") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="supervisor-workspace progress-workspace">
  <header class="workspace-heading">
    <div><span class="workspace-eyebrow">ACADEMIC SUPERVISOR</span><h1>Student Progress Monitor</h1><p>Track assigned placements, approved training hours, tasks, and company feedback.</p></div>
    <span class="live-indicator"><span></span> Firebase data</span>
  </header>
  <asp:Panel ID="ProgressError" runat="server" CssClass="workspace-alert workspace-alert--error" Visible="false" role="alert"><asp:Literal ID="ProgressErrorText" runat="server" /></asp:Panel>
  <asp:Panel ID="ProgressSuccess" runat="server" CssClass="workspace-alert workspace-alert--success" Visible="false" role="status">Your progress note was saved and is visible to the student.</asp:Panel>
  <div class="workspace-metrics" aria-label="Progress summary">
    <article class="workspace-metric"><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><circle cx="9" cy="8" r="3"/><path d="M3.5 19v-1.3A4.7 4.7 0 0 1 8.2 13h1.6a4.7 4.7 0 0 1 4.7 4.7V19M16 5.3a3 3 0 0 1 0 5.8m1.1 2.1a4.5 4.5 0 0 1 3.4 4.4V19"/></svg></span><span>Assigned students</span><strong><asp:Literal ID="AssignedStudentsCount" runat="server" /></strong><small>Active academic placements</small></article>
    <article class="workspace-metric"><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="8.5"/><path d="M12 7v5l3.4 2M7 3.8 5.2 5.6M17 3.8l1.8 1.8"/></svg></span><span>Approved hours</span><strong><asp:Literal ID="ApprovedHoursTotal" runat="server" /></strong><small>Across your assigned students</small></article>
    <article class="workspace-metric"><span class="metric-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="m5 12.5 4.2 4.2L19 7"/><path d="M20 12a8 8 0 1 1-4-6.9"/></svg></span><span>Tasks completed</span><strong><asp:Literal ID="CompletedTasksTotal" runat="server" /></strong><small>Completed of all assigned tasks</small></article>
  </div>
  <div class="workspace-section-heading"><div><span class="workspace-eyebrow">YOUR CASELOAD</span><h2>Assigned placements</h2></div><div class="workspace-list-tools"><label class="visually-hidden" for="progressSearch">Search assigned students</label><div class="workspace-search-wrap"><svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="10.8" cy="10.8" r="6.4"/><path d="m16 16 4.5 4.5"/></svg><input id="progressSearch" type="search" class="workspace-search" placeholder="Search students or companies" /></div><span class="workspace-count"><asp:Literal ID="CaseloadCount" runat="server" /> students</span></div></div>
  <asp:Panel ID="ProgressEmpty" runat="server" CssClass="workspace-empty" Visible="false"><span class="empty-icon" aria-hidden="true">◎</span><h3>No assigned placements yet</h3><p>Students will appear here when a coordinator assigns you as their academic supervisor.</p></asp:Panel>
  <asp:Repeater ID="AssignedStudents" runat="server">
    <HeaderTemplate><div class="progress-list"></HeaderTemplate>
    <ItemTemplate>
      <article class="student-progress-card">
        <div class="student-card-top"><div class="student-identity"><span class="student-avatar"><%#: Eval("Initials") %></span><div><h3><%#: Eval("StudentName") %></h3><p>Student ID <%#: Eval("StudentNumber") %> <span aria-hidden="true">·</span> <%#: Eval("CompanyName") %></p></div></div><span class="placement-status"><%#: Eval("PlacementStatus") %></span></div>
        <div class="progress-card-grid">
          <div class="progress-main"><div class="progress-label-row"><div><span class="detail-label">APPROVED TRAINING HOURS</span><strong><%#: Eval("HoursProgressLabel") %></strong></div><span class="progress-percent"><%#: Eval("ProgressPercent") %>%</span></div><div class="progress-track" role="progressbar" aria-label="Approved training hours" aria-valuemin="0" aria-valuemax="100" aria-valuenow='<%# Eval("ProgressPercent") %>'><span style='width:<%# Eval("ProgressPercent") %>%'></span></div><div class="progress-meta"><span><%#: Eval("ProgressPercent") %>% of required hours approved</span><span><%#: Eval("TaskProgressLabel") %></span></div></div>
          <div class="progress-detail"><div class="detail-item"><span class="detail-label">TRAINING PERIOD</span><strong><%#: Eval("TrainingPeriodLabel") %></strong></div><div class="detail-item"><span class="detail-label">AWAITING REVIEW</span><strong><%#: Eval("SubmittedTasks") %> tasks</strong></div></div>
        </div>
        <div class="company-feedback"><span class="feedback-mark" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M4 5.5h16v11H9l-5 3z"/><path d="M8 10h8M8 13h5"/></svg></span><div><span class="detail-label">COMPANY FEEDBACK</span><p><%#: Eval("FeedbackLabel") %></p></div></div>
        <div class="progress-card-actions"><button type="button" class="progress-note-button progress-info-button" data-open-student-info data-name='<%#: Eval("StudentName") %>' data-number='<%#: Eval("StudentNumber") %>' data-email='<%#: Eval("StudentEmail") %>' data-phone='<%#: Eval("StudentPhone") %>' data-major='<%#: Eval("StudentMajor") %>' data-status='<%#: Eval("StudentAcademicStatus") %>' data-gpa='<%#: Eval("StudentGpa") %>'>View student info <span aria-hidden="true">→</span></button><button type="button" class="progress-note-button" data-open-progress-note data-placement-id='<%#: Eval("PlacementId") %>' data-student-name='<%#: Eval("StudentName") %>'>Add note / feedback <span aria-hidden="true">→</span></button></div>
      </article>
    </ItemTemplate>
    <FooterTemplate></div></FooterTemplate>
  </asp:Repeater>
  <p id="progressSearchEmpty" class="search-empty" hidden>No students match your search.</p>
  <div class="progress-note-modal" id="studentInfoModal" hidden role="dialog" aria-modal="true" aria-labelledby="studentInfoTitle"><div class="progress-note-dialog"><button type="button" class="progress-note-close" data-close-student-info aria-label="Close">×</button><span class="workspace-eyebrow">STUDENT PROFILE</span><h2 id="studentInfoTitle">Student information</h2><dl class="progress-info-grid"><div><dt>Name</dt><dd data-info-field="name"></dd></div><div><dt>Student number</dt><dd data-info-field="number"></dd></div><div><dt>Email</dt><dd data-info-field="email"></dd></div><div><dt>Phone</dt><dd data-info-field="phone"></dd></div><div><dt>Major</dt><dd data-info-field="major"></dd></div><div><dt>Academic status</dt><dd data-info-field="status"></dd></div><div><dt>GPA</dt><dd data-info-field="gpa"></dd></div></dl><div class="progress-note-actions"><button type="button" data-close-student-info>Close</button></div></div></div>
  <div class="progress-note-modal" id="progressNoteModal" hidden role="dialog" aria-modal="true" aria-labelledby="progressNoteTitle">
    <div class="progress-note-dialog">
      <button type="button" class="progress-note-close" data-close-progress-note aria-label="Close">×</button>
      <span class="workspace-eyebrow">STUDENT PROGRESS</span>
      <h2 id="progressNoteTitle">Add progress note</h2>
      <p id="progressNoteStudent"></p>
      <p class="progress-note-inline-error" id="progressNoteInlineError" role="alert" hidden></p>
      <asp:HiddenField ID="NotePlacementId" runat="server" ClientIDMode="Static" />
      <asp:Label ID="NoteBodyLabel" runat="server" AssociatedControlID="NoteBody" Text="Your note for the student" />
      <asp:TextBox ID="NoteBody" runat="server" ClientIDMode="Static" TextMode="MultiLine" Rows="5" MaxLength="2000" placeholder="Describe progress, strengths, or what to improve next..." />
      <small>This note will appear on the student's Progress page. Maximum 2,000 characters.</small>
      <div class="progress-note-actions"><button type="button" data-close-progress-note>Cancel</button><asp:Button ID="SaveProgressNoteButton" runat="server" Text="Save note" OnClick="SaveProgressNote_Click" CausesValidation="false" formnovalidate="formnovalidate" /></div>
    </div>
  </div>
</section>
</asp:Content>
