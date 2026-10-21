<%@ Page Title="My Trainees" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="MyTrainees.aspx.cs" Inherits="Yarmouk.CompanyPages.MyTrainees" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CompanyPages/MyTrainees.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/MyTrainees.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="company-trainees-workspace">
  <header class="trainees-heading"><div><span class="trainees-eyebrow">COMPANY SUPERVISOR</span><h1>My Trainees</h1><p>See confirmed trainees, their placement status, training hours, and task progress.</p></div><span class="trainees-live"><i aria-hidden="true"></i> Live workspace</span></header>
  <asp:Panel ID="TraineesError" runat="server" CssClass="trainees-alert" Visible="false" role="alert"><asp:Literal ID="TraineesErrorText" runat="server" /></asp:Panel>
  <div class="trainees-summary" aria-label="Trainee summary">
    <article><span>Confirmed trainees</span><strong><asp:Literal ID="TraineeCount" runat="server" /></strong><small>Linked to your company account</small></article>
    <article><span>Training hours approved</span><strong><asp:Literal ID="TraineeApprovedHours" runat="server" /></strong><small>Total across your trainees</small></article>
    <article><span>Tasks completed</span><strong><asp:Literal ID="TraineeCompletedTasks" runat="server" /></strong><small>Completed of all assigned tasks</small></article>
  </div>
  <div class="trainees-list-heading"><div><span class="trainees-eyebrow">PLACEMENTS</span><h2>Your training cohort</h2></div><div class="trainee-list-tools"><label class="visually-hidden" for="traineeSearch">Search trainees</label><input id="traineeSearch" type="search" class="trainee-search" placeholder="Search your trainees" /><span class="trainee-count-pill"><asp:Literal ID="TraineeListCount" runat="server" /> trainees</span></div></div>
  <asp:Panel ID="TraineesEmpty" runat="server" CssClass="trainees-empty" Visible="false"><span class="trainees-empty-icon" aria-hidden="true">◌</span><h3>No confirmed trainees yet</h3><p>Confirmed placements assigned to your company will appear here.</p></asp:Panel>
  <asp:Repeater ID="TraineesList" runat="server">
    <HeaderTemplate><div class="trainees-grid"></HeaderTemplate>
    <ItemTemplate>
      <article class="trainee-card">
        <header class="trainee-card-header"><div class="trainee-person"><span class="trainee-avatar"><%#: Eval("Initials") %></span><div><h3><%#: Eval("StudentName") %></h3><p>ID <%#: Eval("StudentNumber") %></p></div></div><span class="trainee-status"><%#: Eval("PlacementStatus") %></span></header>
        <div class="trainee-company"><span aria-hidden="true">▦</span><div><small>HOST COMPANY</small><strong><%#: Eval("CompanyName") %></strong></div></div>
        <div class="trainee-progress"><div class="trainee-progress-label"><span>Approved training hours</span><strong><%#: Eval("HoursProgressLabel") %></strong></div><div class="trainee-track" role="progressbar" aria-label="Approved training hours" aria-valuemin="0" aria-valuemax="100" aria-valuenow='<%# Eval("ProgressPercent") %>'><span style='width:<%# Eval("ProgressPercent") %>%'></span></div><div class="trainee-progress-foot"><span><%#: Eval("ProgressPercent") %>% complete</span><span><%#: Eval("TaskProgressLabel") %></span></div></div>
        <footer class="trainee-card-footer"><span><small>TRAINING PERIOD</small><strong><%#: Eval("TrainingPeriodLabel") %></strong></span><span><small>SUBMITTED TASKS</small><strong><%#: Eval("SubmittedTasks") %> for review</strong></span></footer>
      </article>
    </ItemTemplate>
    <FooterTemplate></div></FooterTemplate>
  </asp:Repeater>
  <p id="traineeSearchEmpty" class="trainee-search-empty" hidden>No trainees match your search.</p>
</section>
</asp:Content>
