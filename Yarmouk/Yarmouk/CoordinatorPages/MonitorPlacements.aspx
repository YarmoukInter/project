<%@ Page Title="Monitor Placements & Progress" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="MonitorPlacements.aspx.cs" Inherits="Yarmouk.CoordinatorPages.MonitorPlacements" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="<%= ResolveUrl("~/Content/CoordinatorPages/MonitorPlacements.css?v=compact-details-1") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CoordinatorPages/MonitorPlacements.js?v=compact-details-1") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<div class="co-monitor" data-monitor-page></div>
</asp:Content>
