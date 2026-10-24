<%@ Page Title="Company Supervisor Account" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="Account.aspx.cs" Inherits="Yarmouk.CompanyPages.Account" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="<%= ResolveUrl("~/Content/CompanyPages/Account.css?v=theme-3") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/Account.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="company-account">
    <header class="account-heading">
        <div><p class="eyebrow">YARMOUK UNIVERSITY · FACULTY OF IT</p><h1>Company supervisor profile</h1><p>Review your account information and keep your contact details up to date.</p></div>
        <span class="account-mark" aria-hidden="true">YU<span>IT</span></span>
    </header>
    <asp:Panel ID="AccountNotice" runat="server" Visible="false" CssClass="account-notice" role="status"><asp:Literal ID="AccountNoticeText" runat="server" /></asp:Panel>
    <asp:Panel ID="AccountError" runat="server" Visible="false" CssClass="account-error" role="alert"><asp:Literal ID="AccountErrorText" runat="server" /></asp:Panel>
    <div class="account-grid account-single-grid">
        <section class="account-card identity-card">
            <div class="card-title"><div><p class="eyebrow">SUPERVISOR ACCOUNT</p><h2>Profile information</h2></div><span class="live-pill"><i></i> Your account</span></div>
            <asp:Panel ID="ProfileForm" runat="server" DefaultButton="SaveAccountButton">
                <div class="form-grid">
                    <div class="field"><label for="FullNameBox">Supervisor name</label><asp:TextBox ID="FullNameBox" runat="server" ClientIDMode="Static" MaxLength="120" required="required" /></div>
                    <div class="field"><label for="PhoneBox">Phone number</label><asp:TextBox ID="PhoneBox" runat="server" ClientIDMode="Static" TextMode="Phone" MaxLength="32" /></div>
                    <div class="field field-wide"><label>Email address</label><asp:Literal ID="EmailValue" runat="server" /></div>
                </div>
                <p class="read-only-note">Contact the coordinator if your sign-in email needs to change.</p>
                <div class="card-actions"><asp:Button ID="SaveAccountButton" runat="server" Text="Save changes" CssClass="primary-button" OnClick="SaveAccount_Click" /></div>
            </asp:Panel>
        </section>
    </div>
</section>
</asp:Content>
