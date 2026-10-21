<%@ Page Title="ManageStudents" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageStudents.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ManageStudents" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/ManageStudents.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/ManageStudents.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Manage Students</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
