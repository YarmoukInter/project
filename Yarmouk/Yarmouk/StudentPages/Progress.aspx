<%@ Page Title="Progress" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" CodeBehind="Progress.aspx.cs" Inherits="Yarmouk.StudentPages.Progress" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/StudentPages/Progress.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/Progress.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Progress</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
