<%@ Page Title="Account" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" AutoEventWireup="true" CodeBehind="Account.aspx.cs" Inherits="Yarmouk.CompanyPages.Account" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CompanyPages/Account.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/Account.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Account</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
