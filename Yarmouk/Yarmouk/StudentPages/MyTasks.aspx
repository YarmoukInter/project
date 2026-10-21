<%@ Page Title="MyTasks" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" CodeBehind="MyTasks.aspx.cs" Inherits="Yarmouk.StudentPages.MyTasks" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/StudentPages/MyTasks.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/MyTasks.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>My Tasks</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
