<%@ Page Async="true" Title="Dashboard" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Yarmouk.CoordinatorPages.Dashboard" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<script src='<%= ResolveUrl("~/Scripts/CoordinatorPages/roster.js") %>?v=backend-1' defer></script>
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/Dashboard.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/Dashboard.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<script type="application/json" id="backend-data"><%= BackendJson %></script>
<input type="hidden" name="adminCsrf" value="<%= CsrfToken %>" />
<input type="hidden" name="adminAction" />
<input type="hidden" name="adminPayload" />
<h1>Dashboard</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
