<%@ Page Title="AboutUs" Language="C#" MasterPageFile="~/PublicPages/Section.Master" AutoEventWireup="true" CodeBehind="AboutUs.aspx.cs" Inherits="Yarmouk.PublicPages.AboutUs" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/PublicPages/AboutUs.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/PublicPages/AboutUs.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>About Us</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
