<%@ Page Title="ManageTasks" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageTasks.aspx.cs" Inherits="Yarmouk.CompanyPages.ManageTasks" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CompanyPages/ManageTasks.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/ManageTasks.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Manage Tasks</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
