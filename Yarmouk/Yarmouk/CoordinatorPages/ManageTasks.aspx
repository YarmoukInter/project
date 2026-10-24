<%@ Page Async="true" Title="Manage Tasks" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageTasks.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ManageTasks" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/ManageTasks.css?v=task-pagination-1") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/roster.js?v=coordinator-header-2") %>" defer></script>
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/ManageTasks.js?v=task-pagination-1") %>" defer></script>
<script src="../Scripts/Shared/record-lists.js?v=export-1" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<script type="application/json" id="backend-data"><%= BackendJson %></script>
<section class="yu-roster coordinator-task-page" data-coordinator-tasks aria-labelledby="coordinator-tasks-title">
  <header class="task-admin-heading">
    <div><p class="task-admin-eyebrow">COORDINATOR WORKSPACE</p><h1 id="coordinator-tasks-title">Training tasks</h1><p>Review tasks assigned to trainees across active and completed placements.</p></div>
    <span class="task-admin-count" data-task-count>0 tasks</span>
  </header>
  <p class="task-admin-feedback" data-task-feedback role="status" aria-live="polite"></p>
  <section class="task-admin-panel" aria-labelledby="task-register-title">
    <div class="task-admin-panel-heading"><div><h2 id="task-register-title">All trainee tasks</h2><p>Key task information at a glance. Open a task to view its full record.</p></div></div>
    <div class="task-admin-tools" data-task-tools aria-label="Organize training tasks"></div>
    <div class="task-admin-list" id="coordinator-task-rows" aria-live="polite"></div>
    <p class="task-admin-empty" data-task-empty hidden>No training tasks are available.</p>
  </section>
  <dialog class="task-detail-dialog" data-task-dialog aria-labelledby="task-detail-title">
    <div class="task-detail-heading"><div><p>TRAINING TASK</p><h2 id="task-detail-title">Task details</h2></div><button class="task-detail-close" type="button" data-task-close aria-label="Close task details">×</button></div>
    <div class="task-detail-content" data-task-detail-content></div>
    <div class="task-detail-actions"><button type="button" class="task-detail-done" data-task-close>Close details</button></div>
  </dialog>
</section>
</asp:Content>
