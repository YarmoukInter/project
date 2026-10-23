<%@ Page Async="true" Title="Academic supervisors" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageAcademicSupervisors.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ManageAcademicSupervisors" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href='<%= ResolveUrl("~/Content/CoordinatorPages/ManageAcademicSupervisors.css") %>?v=backend-1' rel="stylesheet" />
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/roster.js") %>?v=backend-1' defer></script>
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/ManageAcademicSupervisors.js") %>' defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<script type="application/json" id="backend-data"><%= BackendJson %></script>
<input type="hidden" name="adminCsrf" value="<%= CsrfToken %>" />
<input type="hidden" name="adminAction" />
<input type="hidden" name="adminPayload" />
<section class="yu-roster" data-roster-page="ManageAcademicSupervisors" aria-labelledby="roster-title">
  <div class="yr-page-title"><h1 id="roster-title">Academic supervisors</h1></div>
  <section class="yr-panel" aria-labelledby="roster-heading">
    <div class="yr-panel-heading">
      <h2 id="roster-heading"><svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="9" cy="7" r="4"/><path d="M2 21v-2a4 4 0 0 1 4-4h6a4 4 0 0 1 4 4v2m1-17a4 4 0 0 0 0-8m5 17v-2a4 4 0 0 0-3-4"/></svg>Academic supervisors roster</h2>
      <button class="yr-button yr-primary" type="button" data-roster-add><span aria-hidden="true">+</span> Register supervisor</button>
    </div>
    <p class="yr-scroll-hint">Scroll across to see all columns.</p>
    <div class="yr-table-region" role="region" aria-label="Academic supervisors roster" tabindex="0">
      <table class="yr-table"><caption class="yr-sr-only">Academic supervisors roster. </caption><thead><tr><th scope="col">Supervisor</th><th scope="col">Department</th><th scope="col">College</th><th scope="col">Assigned students</th><th scope="col">Status</th><th scope="col">Actions</th></tr></thead><tbody id="roster-body" data-roster-body></tbody></table>
    </div>
    <noscript><p class="yr-notice">Enable JavaScript to manage this roster.</p></noscript>
    <p class="yr-notice" data-roster-feedback role="status" aria-live="polite"></p>
  </section>
  <dialog class="yr-dialog" data-roster-dialog aria-labelledby="editor-title" aria-describedby="editor-description">
    <div class="yr-dialog-heading"><h2 id="editor-title">Register supervisor</h2><button class="yr-close" type="button" data-dialog-close aria-label="Close editor">×</button></div>
    <p id="editor-description">Save account details to Firebase. New accounts require a temporary password.</p>
    <div class="yr-editor-fields" data-editor-fields></div>
    <p class="yr-editor-error" data-editor-error role="alert"></p>
    <div class="yr-dialog-actions"><button class="yr-button" type="button" data-dialog-close>Cancel</button><button class="yr-button yr-primary" type="button" data-editor-save>Save supervisor</button></div>
  </dialog>
  <dialog class="yr-dialog yr-delete-dialog" data-delete-dialog aria-labelledby="delete-title" aria-describedby="delete-description">
    <div class="yr-dialog-heading"><h2 id="delete-title">Deactivate supervisor?</h2><button class="yr-close" type="button" data-delete-cancel aria-label="Close confirmation">×</button></div>
    <p id="delete-description" data-delete-description></p>
    <div class="yr-dialog-actions"><button class="yr-button" type="button" data-delete-cancel>Cancel</button><button class="yr-button yr-danger-solid" type="button" data-delete-confirm>Deactivate supervisor</button></div>
  </dialog>
</section>
</asp:Content>