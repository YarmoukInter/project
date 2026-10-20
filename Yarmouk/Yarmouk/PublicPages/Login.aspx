<%@ Page Title="Login" Language="C#" MasterPageFile="~/PublicPages/Section.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Yarmouk.PublicPages.Login" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/PublicPages/Login.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/PublicPages/Login.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Login</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
