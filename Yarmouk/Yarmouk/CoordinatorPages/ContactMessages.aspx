<%@ Page Async="true" Title="Contact messages" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ContactMessages.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ContactMessages" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/roster.js") %>?v=coordinator-header-2' defer></script>
<link href='<%= ResolveUrl("~/Content/CoordinatorPages/ContactMessages.css") %>?v=inbox-pro-5' rel="stylesheet" />
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/ContactMessages.js") %>?v=inbox-pro-1' defer></script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<script src="../Scripts/Shared/record-lists.js?v=messages-tools-2" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<script type="application/json" id="backend-data"><%= BackendJson %></script>
<input type="hidden" name="adminCsrf" value="<%= CsrfToken %>" />
<input type="hidden" name="adminAction" />
<input type="hidden" name="adminPayload" />
<section class="yu-roster cm-inbox" data-contact-inbox aria-labelledby="inbox-title">
  <div class="yr-page-title"><h1 id="inbox-title">Contact messages</h1></div>
  <div class="cm-intro-row"><p class="cm-intro">Stay in touch with your students and training partners.</p><div class="cm-overview" aria-live="polite"><span class="cm-overview-icon" aria-hidden="true"><svg viewBox="0 0 24 24" fill="none"><path d="M4 6.5A2.5 2.5 0 0 1 6.5 4h11A2.5 2.5 0 0 1 20 6.5v11a2.5 2.5 0 0 1-2.5 2.5h-11A2.5 2.5 0 0 1 4 17.5v-11Z" stroke="currentColor" stroke-width="1.6"/><path d="m5 7 7 5 7-5" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"/></svg></span><span><strong data-total-messages>0 messages</strong><small data-overview-unread>0 unread</small></span></div></div>
  <section class="cm-panel" aria-labelledby="inbox-heading">
    <div class="cm-workspace">
      <section class="cm-list-panel" aria-label="Messages">
        <div class="cm-list-chrome">
          <div class="cm-panel-heading"><h2 id="inbox-heading">Inbox</h2><span class="cm-unread-summary" data-unread-summary></span></div>
          <div class="cm-filters" role="group" aria-label="Filter messages">
            <button type="button" class="cm-filter" data-view="inbox" aria-pressed="true">All <span data-count="inbox">0</span></button>
            <button type="button" class="cm-filter" data-view="unread" aria-pressed="false">Unread <span data-count="unread">0</span></button>
            <button type="button" class="cm-filter" data-view="archived" aria-pressed="false">Archived <span data-count="archived">0</span></button>
          </div>
        </div>
        <div class="cm-list-toolbar"><label for="message-sort" class="yr-sr-only">Sort messages</label><select id="message-sort" data-message-sort><option value="newest">Newest first</option><option value="oldest">Oldest first</option></select></div>
        <div class="cm-message-list" id="message-list" data-message-list></div>
      </section>
      <section class="cm-reader" data-message-reader aria-label="Message details"></section>
    </div>
    <span class="yr-sr-only" data-inbox-feedback role="status" aria-live="polite"></span>
  </section>
  <noscript><p class="yr-notice">Enable JavaScript to manage this inbox.</p></noscript>
  <dialog class="yr-dialog yr-delete-dialog" data-message-delete aria-labelledby="delete-message-title" aria-describedby="delete-message-description">
    <div class="yr-dialog-heading"><h2 id="delete-message-title">Delete this message?</h2><button class="yr-close" type="button" data-delete-cancel aria-label="Close confirmation">×</button></div>
    <p id="delete-message-description" data-delete-description></p>
    <div class="yr-dialog-actions"><button class="yr-button" type="button" data-delete-cancel>Cancel</button><button class="yr-button yr-danger-solid" type="button" data-delete-confirm>Delete message</button></div>
  </dialog>
</section>
</asp:Content>
