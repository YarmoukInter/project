<%@ Page Async="true" Title="Companies" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageCompanies.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ManageCompanies" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href='<%= ResolveUrl("~/Content/CoordinatorPages/ManageCompanies.css") %>?v=hr-contact-2' rel="stylesheet" />
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/roster.js") %>?v=hr-contact-2' defer></script>
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/ManageCompanies.js") %>?v=hr-contact-2' defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<script type="application/json" id="backend-data"><%= BackendJson %></script>
<input type="hidden" name="adminCsrf" value="<%= CsrfToken %>" />
<input type="hidden" name="adminAction" />
<input type="hidden" name="adminPayload" />
<section class="yu-roster" data-roster-page="ManageCompanies" aria-labelledby="roster-title">
  <div class="yr-page-title"><h1 id="roster-title">Companies</h1></div>
  <section class="yr-panel" aria-labelledby="roster-heading">
    <div class="yr-panel-heading">
      <h2 id="roster-heading"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="M3 21h18M5 21V7l8-4v18m0-10h6v10M8 8v1m0 3v1m0 3v1m8-3v1m0 3v1"/></svg>Partner companies roster</h2>
      <button class="yr-button yr-primary" type="button" data-roster-add><span aria-hidden="true">+</span> Register company</button>
    </div>
    <p class="yr-scroll-hint">Scroll across to see all columns.</p>
    <div class="yr-table-region" role="region" aria-label="Partner companies roster" tabindex="0">
      <table class="yr-table"><caption class="yr-sr-only">Partner companies roster. </caption><thead><tr><th scope="col">Company</th><th scope="col">HR name</th><th scope="col">HR email</th><th scope="col">Status</th><th scope="col">Actions</th></tr></thead><tbody id="roster-body" data-roster-body></tbody></table>
    </div>
    <noscript><p class="yr-notice">Enable JavaScript to manage this roster.</p></noscript>
    <p class="yr-notice" data-roster-feedback role="status" aria-live="polite"></p>
  </section>
  <dialog class="yr-dialog" data-roster-dialog aria-labelledby="editor-title" aria-describedby="editor-description">
    <div class="yr-dialog-heading"><h2 id="editor-title">Register company</h2><button class="yr-close" type="button" data-dialog-close aria-label="Close editor">×</button></div>
    <p id="editor-description">HR email is used to sign in to the company account. New accounts require a temporary password.</p>
    <div class="yr-editor-fields" data-editor-fields></div>
    <p class="yr-editor-error" data-editor-error role="alert"></p>
    <div class="yr-dialog-actions"><button class="yr-button" type="button" data-dialog-close>Cancel</button><button class="yr-button yr-primary" type="button" data-editor-save>Save company</button></div>
  </dialog>
  <dialog class="yr-dialog yr-delete-dialog" data-delete-dialog aria-labelledby="delete-title" aria-describedby="delete-description">
    <div class="yr-dialog-heading"><h2 id="delete-title">Deactivate company?</h2><button class="yr-close" type="button" data-delete-cancel aria-label="Close confirmation">×</button></div>
    <p id="delete-description" data-delete-description></p>
    <div class="yr-dialog-actions"><button class="yr-button" type="button" data-delete-cancel>Cancel</button><button class="yr-button yr-danger-solid" type="button" data-delete-confirm>Deactivate company</button></div>
  </dialog>
</section>
</asp:Content>
