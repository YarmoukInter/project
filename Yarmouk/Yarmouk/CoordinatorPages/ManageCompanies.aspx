<%@ Page Title="ManageCompanies" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageCompanies.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ManageCompanies" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/ManageCompanies.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/ManageCompanies.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Manage Companies</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
