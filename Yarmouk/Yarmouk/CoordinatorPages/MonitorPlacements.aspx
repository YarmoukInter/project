<%@ Page Title="Monitor Placements & Progress" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="MonitorPlacements.aspx.cs" Inherits="Yarmouk.CoordinatorPages.MonitorPlacements" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/MonitorPlacements.css?v=placement-spacing-3") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/MonitorPlacements.js?v=coordinator-header-2") %>" defer></script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<script src="../Scripts/Shared/record-lists.js?v=export-1" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<div class="co-monitor" data-monitor-page></div>
</asp:Content>
