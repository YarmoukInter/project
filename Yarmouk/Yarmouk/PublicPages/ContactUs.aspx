<%@ Page Title="ContactUs" Language="C#" MasterPageFile="~/PublicPages/Section.Master" AutoEventWireup="true" CodeBehind="ContactUs.aspx.cs" Inherits="Yarmouk.PublicPages.ContactUs" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/PublicPages/ContactUs.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/PublicPages/ContactUs.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Contact Us</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
