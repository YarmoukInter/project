<%@ Page Async="true" Title="Contact Us" Language="C#" AutoEventWireup="true" CodeBehind="ContactUs.aspx.cs" Inherits="Yarmouk.PublicPages.ContactUs" %>
<!doctype html>
<html lang="en" data-view="contact">
<head runat="server">
  <meta charset="utf-8" /><meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Yarmouk University — Contact Us</title>
<link rel="stylesheet" runat="server" href="~/Content/PublicPages/ContactUs.css" />
<script src='<%= ResolveUrl("~/Scripts/PublicPages/ContactUs.js") %>' defer></script></head>
<body>
<form id="form1" runat="server">
  <a class="contact-skip" href="#main">Skip to content</a>
  <div data-public-navbar-placeholder="true"></div>
  <main class="public-main" id="main" tabindex="-1">
    <div class="contact-layout">
      <section class="contact-introduction" aria-labelledby="contact-title">
        <p class="contact-context">Practical training at Yarmouk</p>
        <h1 id="contact-title">Contact Us</h1>
        <p class="contact-lead">Have a question about your internship? Leave a message for your university internship coordinator.</p>
        <div class="contact-letter-mount"></div>
      </section>
      <section class="contact-form-panel" aria-labelledby="contact-form-title">
        <header class="contact-form-heading"><h2 id="contact-form-title">Leave a message</h2><p>All fields are required.</p></header>
        <div class="contact-fields" data-form="contact">
          <div class="form-grid">
            <label class="field" for="ContactName"><span>Name</span><asp:TextBox ID="ContactName" runat="server" ClientIDMode="Static" CssClass="ui-input" data-contact-field="name" MaxLength="120" autocomplete="name" required="required" /></label>
            <label class="field" for="ContactEmail"><span>Email address</span><asp:TextBox ID="ContactEmail" runat="server" ClientIDMode="Static" CssClass="ui-input" data-contact-field="email" TextMode="Email" MaxLength="254" autocomplete="email" required="required" /></label>
            <label class="field" for="ContactSubject"><span>Subject</span><asp:TextBox ID="ContactSubject" runat="server" ClientIDMode="Static" CssClass="ui-input" data-contact-field="subject" MaxLength="180" required="required" /></label>
            <label class="field field-wide" for="ContactMessage"><span>Message</span><asp:TextBox ID="ContactMessage" runat="server" ClientIDMode="Static" CssClass="ui-textarea" data-contact-field="message" TextMode="MultiLine" Rows="5" MaxLength="5000" required="required" /></label>
          </div>
          <p class="form-error" id="contact-status" data-sent="<%= MessageSent ? "true" : "false" %>" role="status" aria-live="polite" aria-atomic="true"><asp:Literal ID="ContactStatus" runat="server" Mode="Encode" /></p>
          <div class="form-footer">
            <asp:Button ID="SendButton" runat="server" CssClass="ui-button" Text="Send message" OnClick="SendButton_Click" />
          </div>
        </div>
      </section>
      <div class="contact-directory"><dl><dt>Faculty</dt><dd>Information Technology and<br class="contact-desktop-break" /> Computer Science</dd></dl><dl><dt>Location</dt><dd>Yarmouk University<br />Irbid, Jordan</dd></dl></div>
    </div>
  </main>
</form>
</body>
</html>
