<%@ Page Title="Change temporary password" Language="C#" Async="true" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Yarmouk.StudentPages.ChangePassword" %>
<!DOCTYPE html>
<html lang="en"><head runat="server"><meta charset="utf-8" /><meta name="viewport" content="width=device-width, initial-scale=1" /><title>Change temporary password — Yarmouk University</title>
<link href="<%= ResolveUrl("~/Content/StudentPages/ChangePassword.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/ChangePassword.js") %>" defer></script>
</head><body><form id="SetupForm" runat="server" novalidate>
<header class="setup-header"><a href="<%= ResolveUrl("~/PublicPages/Home.aspx") %>"><img src="<%= ResolveUrl("~/Content/Images/yarmouk-college-logo.png") %>" alt="Yarmouk University" /><span>Yarmouk University<small>Internship Management System</small></span></a><asp:Button ID="SignOutButton" runat="server" Text="Sign out" CssClass="sign-out" OnClick="SignOut_Click" CausesValidation="false" /></header>
<main id="main" class="setup-main"><span class="partner">ACCOUNT SETUP</span><h1>Choose your own password.</h1><p class="lead">Your account requires a temporary-password change before you enter the workspace.</p>
<section class="card-surface" aria-labelledby="changeTitle"><div class="card-heading"><span class="lock-icon" aria-hidden="true">◇</span><h2 id="changeTitle">Change temporary password</h2></div>
<div class="form-grid"><div class="field"><label for="CurrentPassword">Temporary password</label><div class="password-control"><asp:TextBox ID="CurrentPassword" ClientIDMode="Static" runat="server" TextMode="Password" autocomplete="current-password" MaxLength="128" /><button type="button" data-password="CurrentPassword" aria-controls="CurrentPassword" aria-pressed="false">Show</button></div></div>
<div class="field"><label for="NewPassword">New password</label><div class="password-control"><asp:TextBox ID="NewPassword" ClientIDMode="Static" runat="server" TextMode="Password" autocomplete="new-password" MaxLength="128" aria-describedby="passwordHelp" /><button type="button" data-password="NewPassword" aria-controls="NewPassword" aria-pressed="false">Show</button></div></div>
<div class="field"><label for="ConfirmPassword">Confirm new password</label><div class="password-control"><asp:TextBox ID="ConfirmPassword" ClientIDMode="Static" runat="server" TextMode="Password" autocomplete="new-password" MaxLength="128" /><button type="button" data-password="ConfirmPassword" aria-controls="ConfirmPassword" aria-pressed="false">Show</button></div></div></div>
<p id="passwordHelp" class="help-text">Use 8–128 characters and a password different from your temporary password.</p><p class="form-error" role="alert"><asp:Literal ID="Message" runat="server" /></p>
<asp:Button ID="ChangeButton" runat="server" Text="Continue to workspace" CssClass="primary-button" OnClick="Change_Click" />
<p class="help-text">You can access your workspace after your password has been changed successfully.</p></section></main>
<footer>Faculty of Information Technology and Computer Science · Yarmouk University</footer>
</form></body></html>
