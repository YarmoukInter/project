<%@ Page Language="C#" CodeBehind="ContactUs.aspx.cs" Inherits="Yarmouk.PublicPages.ContactUs" %>
<!doctype html>
<html lang="en" data-view="contact">
<head runat="server">
  <meta charset="utf-8" /><meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Yarmouk University — Contact Us</title>
<link rel="stylesheet" href="../Content/PublicPages/ContactUs.css" />
<script src="../Scripts/PublicPages/ContactUs.js" defer></script></head>
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
            <label class="field"><span>XXName</span><input class="ui-input" name="name" id="contact-name" type="text" autocomplete="on" maxlength="120" required /></label>
            <label class="field"><span>Email address</span><input class="ui-input" name="email" id="contact-email" type="email" autocomplete="on" maxlength="254" required /></label>
            <label class="field"><span>Subject</span><input class="ui-input" name="subject" id="contact-subject" type="text" maxlength="180" required /></label>
            <label class="field field-wide"><span>Message</span><textarea class="ui-textarea" name="message" id="contact-message" rows="5" maxlength="5000" required></textarea></label>
          </div>
          <p class="form-error" id="contact-status" role="status" aria-live="polite" aria-atomic="true"></p>
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
