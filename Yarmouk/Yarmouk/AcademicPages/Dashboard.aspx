<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/AcademicPages/Section.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Yarmouk.AcademicPages.Dashboard" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/AcademicPages/Dashboard.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/AcademicPages/Dashboard.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Dashboard</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
