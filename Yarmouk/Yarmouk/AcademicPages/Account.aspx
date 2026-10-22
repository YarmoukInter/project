<%@ Page Title="Academic Supervisor Account" Language="C#" MasterPageFile="~/AcademicPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="Account.aspx.cs" Inherits="Yarmouk.AcademicPages.Account" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/AcademicPages/Account.css" rel="stylesheet" />
<script src="../Scripts/AcademicPages/Account.js" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="academic-account" aria-labelledby="accountTitle">
    <header class="account-heading">
        <div><span class="account-eyebrow">ACADEMIC SUPERVISOR</span><h1 id="accountTitle">My profile</h1><p>Review your account information and keep your contact details current.</p></div>
        <a class="account-back-link" href="Dashboard.aspx"><span aria-hidden="true">←</span> Back to dashboard</a>
    </header>

    <div class="preview-notice" role="note"><span aria-hidden="true">ⓘ</span><span>Your account information is loaded from your university account.</span></div>

    <div class="account-layout">
        <aside class="profile-summary-card" aria-label="Supervisor account summary">
            <div class="profile-avatar" aria-hidden="true"><asp:Literal ID="ProfileInitials" runat="server" /></div>
            <h2 id="profileNamePreview"><asp:Literal ID="ProfileName" runat="server" /></h2>
            <span class="profile-role">Academic Supervisor</span>
            <span class="profile-status"><i aria-hidden="true"></i> Active account</span>
            <div class="profile-summary-divider"></div>
            <div class="profile-summary-detail"><span>Sign-in email</span><strong><asp:Literal ID="ProfileEmailSummary" runat="server" /></strong></div>
            <div class="profile-summary-note"><span aria-hidden="true">ⓘ</span><p>Your email is managed through account access. Contact the coordinator if it needs to change.</p></div>
        </aside>

        <section class="account-form-card" aria-labelledby="personalDetailsTitle">
            <div class="account-card-heading"><div><span class="account-eyebrow">ACCOUNT DETAILS</span><h2 id="personalDetailsTitle">Personal information</h2><p>Update your name and contact number.</p></div><span class="edit-mark" aria-hidden="true">✎</span></div>
            <div class="account-fields">
                <div class="account-field"><label for="FullNameInput">Full name</label><asp:TextBox ID="FullNameInput" runat="server" ClientIDMode="Static" CssClass="account-input" MaxLength="100" autocomplete="name" /></div>
                <div class="account-field"><label for="EmailInput">Sign-in email</label><asp:TextBox ID="EmailInput" runat="server" ClientIDMode="Static" CssClass="account-input account-input--readonly" TextMode="Email" ReadOnly="true" /><small>This address is used to sign in and cannot be changed here.</small></div>
                <div class="account-field"><label for="PhoneInput">Phone number</label><asp:TextBox ID="PhoneInput" runat="server" ClientIDMode="Static" CssClass="account-input" TextMode="Phone" MaxLength="30" autocomplete="tel" placeholder="Add a contact number" /><small>Optional. Include your country code when needed.</small></div>
                <div class="account-field"><span class="account-field-label">Supervisor role</span><div class="account-readonly-value"><span class="readonly-dot" aria-hidden="true"></span>Academic Supervisor</div><small>Your role and student assignments are managed by the coordinator.</small></div>
            </div>
            <div class="account-form-footer"><asp:Label ID="ProfileSaveMessage" runat="server" ClientIDMode="Static" CssClass="account-message" role="status" aria-live="polite" Text="Only your name and phone number can be edited." /><asp:Button ID="SaveProfileButton" runat="server" ClientIDMode="Static" CssClass="account-save-button" Text="Save changes" OnClick="SaveProfile_Click" /></div>
        </section>
    </div>
</section>
</asp:Content>
