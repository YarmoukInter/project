<%@ Page Async="true" Title="Students" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageStudents.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ManageStudents" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href='<%= ResolveUrl("~/Content/CoordinatorPages/ManageStudents.css") %>?v=student-inputs-2' rel="stylesheet" />
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/roster.js") %>?v=input-types-3' defer></script>
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/ManageStudents.js") %>?v=input-types-3' defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<script type="application/json" id="backend-data"><%= BackendJson %></script>
<input type="hidden" name="adminCsrf" value="<%= CsrfToken %>" />
<input type="hidden" name="adminAction" />
<input type="hidden" name="adminPayload" />
<section class="yu-roster" data-roster-page="ManageStudents" aria-labelledby="roster-title">
  <div class="yr-page-title"><h1 id="roster-title">Students</h1></div>
  <section class="yr-panel" aria-labelledby="roster-heading">
    <div class="yr-panel-heading">
      <h2 id="roster-heading"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="m2 8 10-5 10 5-10 5L2 8Zm4 3v6c4 3 8 3 12 0v-6m4-3v9"/></svg>Registered students roster</h2>
      <button class="yr-button yr-primary" type="button" data-roster-add><span aria-hidden="true">+</span> Register student</button>
    </div>
    <p class="yr-notice" data-roster-feedback role="status" aria-live="polite" tabindex="-1"></p>
    <p class="yr-scroll-hint">Scroll across to see all columns.</p>
    <div class="yr-table-region" role="region" aria-label="Registered students roster" tabindex="0">
      <table class="yr-table"><caption class="yr-sr-only">Registered students roster. </caption><thead><tr><th scope="col">Student</th><th scope="col">Department</th><th scope="col">GPA</th><th scope="col">College</th><th scope="col">Assigned supervisor</th><th scope="col">Actions</th></tr></thead><tbody id="roster-body" data-roster-body></tbody></table>
    </div>
    <noscript><p class="yr-notice">Enable JavaScript to manage this roster.</p></noscript>
  </section>
  <dialog class="yr-dialog" data-roster-dialog aria-labelledby="editor-title" aria-describedby="editor-description">
    <div class="yr-dialog-heading"><h2 id="editor-title">Register student</h2><button class="yr-close" type="button" data-dialog-close aria-label="Close editor">×</button></div>
    <p id="editor-description">Save account details to Firebase. New accounts require a temporary password.</p>
    <div class="yr-editor-fields" data-editor-fields></div>
    <p class="yr-editor-error" data-editor-error role="alert"></p>
    <div class="yr-dialog-actions"><button class="yr-button" type="button" data-dialog-close>Cancel</button><button class="yr-button yr-primary" type="button" data-editor-save>Save student</button></div>
  </dialog>
  <dialog class="yr-dialog yr-delete-dialog" data-delete-dialog aria-labelledby="delete-title" aria-describedby="delete-description">
    <div class="yr-dialog-heading"><h2 id="delete-title">Deactivate student?</h2><button class="yr-close" type="button" data-delete-cancel aria-label="Close confirmation">×</button></div>
    <p id="delete-description" data-delete-description></p>
    <div class="yr-dialog-actions"><button class="yr-button" type="button" data-delete-cancel>Cancel</button><button class="yr-button yr-danger-solid" type="button" data-delete-confirm>Deactivate student</button></div>
  </dialog>
</section>
</asp:Content>
