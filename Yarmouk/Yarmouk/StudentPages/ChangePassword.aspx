<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/StudentPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Yarmouk.StudentPages.ChangePassword" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/StudentPages/ChangePassword.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/ChangePassword.js") %>" defer></script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<div class="setup-page"><section id="main" class="setup-main"><span class="partner">ACCOUNT SETUP</span><h1>Choose your own password.</h1><p class="lead">Your account requires a temporary-password change before you enter the workspace.</p>
<section class="card-surface" aria-labelledby="changeTitle"><div class="card-heading"><span class="lock-icon" aria-hidden="true">◇</span><h2 id="changeTitle">Change temporary password</h2></div>
<div class="form-grid"><div class="field"><label for="CurrentPassword">Temporary password</label><div class="password-control"><asp:TextBox ID="CurrentPassword" ClientIDMode="Static" runat="server" TextMode="Password" autocomplete="current-password" MaxLength="128" /><button type="button" data-password="CurrentPassword" aria-controls="CurrentPassword" aria-pressed="false">Show</button></div></div>
<div class="field"><label for="NewPassword">New password</label><div class="password-control"><asp:TextBox ID="NewPassword" ClientIDMode="Static" runat="server" TextMode="Password" autocomplete="new-password" MaxLength="128" aria-describedby="passwordHelp" /><button type="button" data-password="NewPassword" aria-controls="NewPassword" aria-pressed="false">Show</button></div></div>
<div class="field"><label for="ConfirmPassword">Confirm new password</label><div class="password-control"><asp:TextBox ID="ConfirmPassword" ClientIDMode="Static" runat="server" TextMode="Password" autocomplete="new-password" MaxLength="128" /><button type="button" data-password="ConfirmPassword" aria-controls="ConfirmPassword" aria-pressed="false">Show</button></div></div></div>
<p id="passwordHelp" class="help-text">Use 8–128 characters and a password different from your temporary password.</p><p class="form-error" role="alert"><asp:Literal ID="Message" runat="server" /></p>
<asp:Button ID="ChangeButton" runat="server" Text="Continue to workspace" CssClass="primary-button" OnClick="Change_Click" />
<p class="help-text">You can access your workspace after your password has been changed successfully.</p></section></section></div>
</asp:Content>
