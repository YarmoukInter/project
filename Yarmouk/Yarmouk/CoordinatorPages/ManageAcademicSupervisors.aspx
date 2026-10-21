<%@ Page Title="ManageAcademicSupervisors" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageAcademicSupervisors.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ManageAcademicSupervisors" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/ManageAcademicSupervisors.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/ManageAcademicSupervisors.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Manage Academic Supervisors</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
