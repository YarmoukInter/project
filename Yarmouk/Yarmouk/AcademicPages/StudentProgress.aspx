<%@ Page Title="StudentProgress" Language="C#" MasterPageFile="~/AcademicPages/Section.Master" AutoEventWireup="true" CodeBehind="StudentProgress.aspx.cs" Inherits="Yarmouk.AcademicPages.StudentProgress" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/AcademicPages/StudentProgress.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/AcademicPages/StudentProgress.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Student Progress</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
