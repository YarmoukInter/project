<%@ Page Title="ChangePassword" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="Yarmouk.StudentPages.ChangePassword" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/StudentPages/ChangePassword.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/ChangePassword.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Change Password</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
