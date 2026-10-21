<%@ Page Title="Student Progress Monitor" Language="C#" MasterPageFile="~/AcademicPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="StudentProgress.aspx.cs" Inherits="Yarmouk.AcademicPages.StudentProgress" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/AcademicPages/StudentProgress.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/AcademicPages/StudentProgress.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="supervisor-workspace progress-workspace">
  <header class="workspace-heading">
    <div><span class="workspace-eyebrow">ACADEMIC SUPERVISOR</span><h1>Student Progress Monitor</h1><p>Track assigned placements, approved training hours, tasks, and company feedback.</p></div>
    <span class="live-indicator"><span></span> Firebase data</span>
  </header>
  <asp:Panel ID="ProgressError" runat="server" CssClass="workspace-alert workspace-alert--error" Visible="false" role="alert"><asp:Literal ID="ProgressErrorText" runat="server" /></asp:Panel>
  <div class="workspace-metrics" aria-label="Progress summary">
    <article class="workspace-metric"><span>Assigned students</span><strong><asp:Literal ID="AssignedStudentsCount" runat="server" /></strong><small>Active academic placements</small></article>
    <article class="workspace-metric"><span>Approved hours</span><strong><asp:Literal ID="ApprovedHoursTotal" runat="server" /></strong><small>Across your assigned students</small></article>
    <article class="workspace-metric"><span>Tasks completed</span><strong><asp:Literal ID="CompletedTasksTotal" runat="server" /></strong><small>Completed of all assigned tasks</small></article>
  </div>
  <div class="workspace-section-heading"><div><span class="workspace-eyebrow">YOUR CASELOAD</span><h2>Assigned placements</h2></div><div class="workspace-list-tools"><label class="visually-hidden" for="progressSearch">Search assigned students</label><input id="progressSearch" type="search" class="workspace-search" placeholder="Search students or companies" /><span class="workspace-count"><asp:Literal ID="CaseloadCount" runat="server" /> students</span></div></div>
  <asp:Panel ID="ProgressEmpty" runat="server" CssClass="workspace-empty" Visible="false"><span class="empty-icon" aria-hidden="true">◎</span><h3>No assigned placements yet</h3><p>Students will appear here when a coordinator assigns you as their academic supervisor.</p></asp:Panel>
  <asp:Repeater ID="AssignedStudents" runat="server">
    <HeaderTemplate><div class="progress-list"></HeaderTemplate>
    <ItemTemplate>
      <article class="student-progress-card">
        <div class="student-card-top"><div class="student-identity"><span class="student-avatar"><%#: Eval("Initials") %></span><div><h3><%#: Eval("StudentName") %></h3><p>Student ID <%#: Eval("StudentNumber") %> <span aria-hidden="true">·</span> <%#: Eval("CompanyName") %></p></div></div><span class="placement-status"><%#: Eval("PlacementStatus") %></span></div>
        <div class="progress-card-grid">
          <div class="progress-main"><div class="progress-label-row"><span>Training hours</span><strong><%#: Eval("HoursProgressLabel") %></strong></div><div class="progress-track" role="progressbar" aria-label="Approved training hours" aria-valuemin="0" aria-valuemax="100" aria-valuenow='<%# Eval("ProgressPercent") %>'><span style='width:<%# Eval("ProgressPercent") %>%'></span></div><div class="progress-meta"><span><%#: Eval("ProgressPercent") %>% of required hours approved</span><span><%#: Eval("TaskProgressLabel") %></span></div></div>
          <div class="progress-detail"><span class="detail-label">Training period</span><strong><%#: Eval("TrainingPeriodLabel") %></strong><span class="detail-label detail-label--spaced">Task activity</span><strong><%#: Eval("SubmittedTasks") %> awaiting review</strong></div>
        </div>
        <div class="company-feedback"><span class="feedback-mark" aria-hidden="true">↗</span><div><span class="detail-label">Company feedback</span><p><%#: Eval("FeedbackLabel") %></p></div></div>
      </article>
    </ItemTemplate>
    <FooterTemplate></div></FooterTemplate>
  </asp:Repeater>
  <p id="progressSearchEmpty" class="search-empty" hidden>No students match your search.</p>
</section>
</asp:Content>
