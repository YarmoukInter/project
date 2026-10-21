<%@ Page Title="Apply" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" CodeBehind="Apply.aspx.cs" Inherits="Yarmouk.StudentPages.Apply" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/StudentPages/Apply.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/Apply.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Apply</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
