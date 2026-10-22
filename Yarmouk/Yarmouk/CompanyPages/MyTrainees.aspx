<%@ Page Title="My Trainees" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="MyTrainees.aspx.cs" Inherits="Yarmouk.CompanyPages.MyTrainees" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CompanyPages/MyTrainees.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/MyTrainees.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="company-trainees-workspace">
  <header class="trainees-heading"><div><span class="trainees-eyebrow">COMPANY WORKSPACE</span><h1>My Trainees</h1><p>Track your confirmed placements, review progress, and open each trainee’s work.</p></div><span class="trainees-live"><i aria-hidden="true"></i> Placement overview</span></header>
  <asp:Panel ID="TraineesError" runat="server" CssClass="trainees-alert" Visible="false" role="alert"><asp:Literal ID="TraineesErrorText" runat="server" /></asp:Panel>
  <div class="trainees-summary" aria-label="Trainee summary">
    <article><span class="summary-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M16 20v-1.7a3.3 3.3 0 0 0-3.3-3.3H6.3A3.3 3.3 0 0 0 3 18.3V20m13-10a3.2 3.2 0 1 0 0-6.4M21 20v-1.7a3.3 3.3 0 0 0-2.5-3.2M12.7 8.3a3.7 3.7 0 1 1-7.4 0 3.7 3.7 0 0 1 7.4 0Z"/></svg></span><span>Confirmed trainees</span><strong><asp:Literal ID="TraineeCount" runat="server" /></strong><small>Active placements in your cohort</small></article>
    <article><span class="summary-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3.2 2"/></svg></span><span>Approved hours</span><strong><asp:Literal ID="TraineeApprovedHours" runat="server" /></strong><small>Verified training time</small></article>
    <article><span class="summary-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M8 5h12M8 12h12M8 19h12M3 5l1 1 2-2m-3 8 1 1 2-2m-3 8 1 1 2-2"/></svg></span><span>Tasks completed</span><strong><asp:Literal ID="TraineeCompletedTasks" runat="server" /></strong><small>Completed across your cohort</small></article>
  </div>
  <div class="trainees-list-heading"><div><span class="trainees-eyebrow">PLACEMENTS</span><h2>Your training cohort</h2></div><div class="trainee-list-tools"><label class="trainee-search-wrap"><span class="visually-hidden">Search trainees</span><svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="10.8" cy="10.8" r="6.8"/><path d="m16 16 5 5"/></svg><input id="traineeSearch" type="search" class="trainee-search" placeholder="Search name or student ID" /></label><label class="trainee-filter-wrap"><span class="visually-hidden">Filter by placement status</span><select id="traineeStatusFilter" class="trainee-status-filter"><option value="all">All statuses</option><option value="active">In progress</option><option value="completed">Completed</option></select></label><span class="trainee-count-pill"><strong id="traineeVisibleCount"><asp:Literal ID="TraineeListCount" runat="server" /></strong> shown</span></div></div>
  <asp:Panel ID="TraineesEmpty" runat="server" CssClass="trainees-empty" Visible="false"><span class="trainees-empty-icon" aria-hidden="true">◌</span><h3>No confirmed trainees yet</h3><p>Confirmed placements assigned to your company will appear here.</p></asp:Panel>
  <asp:Repeater ID="TraineesList" runat="server">
    <HeaderTemplate><div class="trainees-grid"></HeaderTemplate>
    <ItemTemplate>
      <article class="trainee-card" data-trainee-status="<%#: Eval("PlacementStatus") %>">
        <header class="trainee-card-header"><div class="trainee-person"><span class="trainee-avatar"><%#: Eval("Initials") %></span><div><h3><%#: Eval("StudentName") %></h3><p>Student ID <%#: Eval("StudentNumber") %></p></div></div><span class="trainee-status"><i aria-hidden="true"></i><%#: Eval("PlacementStatus") %></span></header>
        <div class="trainee-company"><span class="trainee-company-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M4 21V4h11v17M15 10h5v11M2 21h20M8 8h1m3 0h1M8 12h1m3 0h1M8 16h1m3 0h1m6-2h1m-1 4h1" /></svg></span><div><small>HOST COMPANY</small><strong><%#: Eval("CompanyName") %></strong></div></div>
        <div class="trainee-progress"><div class="trainee-progress-label"><span>Approved training hours</span><strong><%#: Eval("HoursProgressLabel") %></strong><b><%#: Eval("ProgressPercent") %>%</b></div><div class="trainee-track" role="progressbar" aria-label="Approved training hours" aria-valuemin="0" aria-valuemax="100" aria-valuenow='<%# Eval("ProgressPercent") %>'><span style='width:<%# Eval("ProgressPercent") %>%'></span></div><div class="trainee-progress-foot"><span><%#: Eval("TaskProgressLabel") %></span></div></div>
        <div class="trainee-card-meta"><div><small>TRAINING PERIOD</small><strong><%#: Eval("TrainingPeriodLabel") %></strong></div><div><small>TASKS FOR REVIEW</small><strong><%#: Eval("SubmittedTasks") %></strong></div></div>
        <div class="trainee-card-actions">
          <button type="button" class="trainee-action trainee-action--secondary" data-info-trigger="true"
            data-name="<%#: Eval("StudentName") %>" data-number="<%#: Eval("StudentNumber") %>"
            data-email="<%#: Eval("StudentEmail") %>" data-phone="<%#: Eval("StudentPhone") %>"
            data-major="<%#: Eval("StudentMajor") %>" data-academic-status="<%#: Eval("StudentAcademicStatus") %>"
            data-gpa="<%#: Eval("StudentGpa") %>" aria-haspopup="dialog">
            <svg viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="4" width="18" height="16" rx="3"/><circle cx="9" cy="10" r="2.5"/><path d="M5.5 17a3.5 3.5 0 0 1 7 0M15 9h3m-3 4h3"/></svg><span>View student</span>
          </button>
          <a class="trainee-action trainee-action--primary" aria-label="View tasks for <%#: Eval("StudentName") %>"
            href='<%# ResolveUrl("~/CompanyPages/ManageTasks.aspx") + "?studentUid=" + System.Web.HttpUtility.UrlEncode(Convert.ToString(Eval("StudentUid"))) + "&amp;placementId=" + System.Web.HttpUtility.UrlEncode(Convert.ToString(Eval("PlacementId"))) %>'>
            <svg viewBox="0 0 24 24" aria-hidden="true"><path d="m4 7 1.5 1.5L8 6M11 7h9M4 14l1.5 1.5L8 13m3 1h9M4 20h1m6 0h9"/></svg><span>View tasks</span><span class="trainee-action-arrow" aria-hidden="true"><svg viewBox="0 0 24 24"><path d="M5 12h14m-6-6 6 6-6 6"/></svg></span>
          </a>
        </div>
      </article>
    </ItemTemplate>
    <FooterTemplate></div></FooterTemplate>
  </asp:Repeater>
  <p id="traineeSearchEmpty" class="trainee-search-empty" hidden>No trainees match your search.</p>
</section>
<dialog id="traineeInfoDialog" class="trainee-dialog" aria-labelledby="traineeDialogTitle">
  <div class="trainee-dialog-top"><span class="trainee-dialog-icon" aria-hidden="true"><svg viewBox="0 0 24 24"><rect x="3" y="4" width="18" height="16" rx="3"/><circle cx="9" cy="10" r="2.5"/><path d="M5.5 17a3.5 3.5 0 0 1 7 0M15 9h3m-3 4h3"/></svg></span><button type="button" class="trainee-dialog-close" data-dialog-close aria-label="Close student details"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="m6 6 12 12M18 6 6 18"/></svg></button></div>
  <span class="trainees-eyebrow">STUDENT PROFILE</span><h2 id="traineeDialogTitle">Student information</h2><p class="trainee-dialog-subtitle">Basic academic and contact details for this trainee.</p>
  <dl class="trainee-profile-grid">
    <div><dt>Full name</dt><dd data-profile-field="name">—</dd></div>
    <div><dt>Student ID</dt><dd data-profile-field="number">—</dd></div>
    <div><dt>Email</dt><dd data-profile-field="email">—</dd></div>
    <div><dt>Phone</dt><dd data-profile-field="phone">—</dd></div>
    <div><dt>Major</dt><dd data-profile-field="major">—</dd></div>
    <div><dt>Academic status</dt><dd data-profile-field="academicStatus">—</dd></div>
    <div><dt>GPA</dt><dd data-profile-field="gpa">—</dd></div>
  </dl>
  <div class="trainee-dialog-footer"><button type="button" class="trainee-action trainee-action--primary" data-dialog-close>Close</button></div>
</dialog>
</asp:Content>
