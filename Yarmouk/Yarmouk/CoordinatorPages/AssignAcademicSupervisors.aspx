<%@ Page Title="AssignAcademicSupervisors" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="AssignAcademicSupervisors.aspx.cs" Inherits="Yarmouk.CoordinatorPages.AssignAcademicSupervisors" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/AssignAcademicSupervisors.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/AssignAcademicSupervisors.js") %>" defer></script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="assignment-page" aria-labelledby="assignment-title">
  <header class="assignment-heading">
    <p class="eyebrow">Coordinator workspace</p>
    <h1 id="assignment-title">Assign academic supervisors</h1>
    <p>Connect students with their academic supervisor through the student roster. Review existing assignments before making a change.</p>
  </header>
  <div class="assignment-layout">
    <section class="assignment-panel" aria-labelledby="assignment-steps-title">
      <h2 id="assignment-steps-title">Make an assignment</h2>
      <p>Supervisor assignments are managed alongside each student's university record.</p>
      <ol class="assignment-steps">
        <li><strong>Find the student</strong><span>Open the student roster and search by name, student number, or email.</span></li>
        <li><strong>Choose an academic supervisor</strong><span>Use the Assigned supervisor menu in the student's row to select a supervisor.</span></li>
        <li><strong>Review the saved assignment</strong><span>Wait for confirmation in the roster before moving to the next student.</span></li>
      </ol>
      <a class="primary-button" href="ManageStudents.aspx">Open student roster <span aria-hidden="true">→</span></a>
    </section>
    <aside class="assignment-panel" aria-labelledby="assignment-resources-title">
      <h2 id="assignment-resources-title">Related workspaces</h2>
      <a class="assignment-resource" href="ManageAcademicSupervisors.aspx"><strong>Academic supervisors <span aria-hidden="true">↗</span></strong><span>Review supervisor accounts and assigned student counts.</span></a>
      <a class="assignment-resource" href="MonitorPlacements.aspx"><strong>Placement progress <span aria-hidden="true">↗</span></strong><span>Follow training progress and placements that need attention.</span></a>
    </aside>
  </div>
</section>
</asp:Content>
