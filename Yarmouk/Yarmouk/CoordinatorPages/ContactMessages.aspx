<%@ Page Title="ContactMessages" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ContactMessages.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ContactMessages" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/ContactMessages.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/ContactMessages.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Contact Messages</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
