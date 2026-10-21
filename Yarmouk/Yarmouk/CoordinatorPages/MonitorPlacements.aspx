<%@ Page Title="MonitorPlacements" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="MonitorPlacements.aspx.cs" Inherits="Yarmouk.CoordinatorPages.MonitorPlacements" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/MonitorPlacements.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/MonitorPlacements.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<h1>Monitor Placements</h1>
<p>Page layout placeholder. Add controls and events here.</p>
</asp:Content>
