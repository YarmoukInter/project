<%@ Page Async="true" Title="Training opportunities" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ManageOpportunities.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ManageOpportunities" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=1" rel="stylesheet" />
<link href="../Content/CoordinatorPages/ManageOpportunities.css?v=opportunity-controls-1" rel="stylesheet" />
<script src="../Scripts/CoordinatorPages/ManageOpportunities.js?v=opportunity-controls-1" defer></script>
<script src="../Scripts/Shared/record-lists.js?v=applications-directory-4" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<script type="application/json" id="backend-data"><%= BackendJson %></script>
<input type="hidden" name="adminCsrf" value="<%= CsrfToken %>" />
<input type="hidden" name="adminAction" />
<input type="hidden" name="adminPayload" />
<section class="op-admin" aria-labelledby="op-title">
  <header class="op-hero"><div><p class="op-eyebrow">COORDINATOR WORKSPACE</p><h1 id="op-title">Training opportunities</h1><p>Explore training providers, available places, and application activity.</p></div><span class="op-total" data-op-total>Loading opportunities</span></header>
  <p data-op-feedback role="status" hidden></p>
  <div class="op-metrics" data-op-metrics></div>
  <section class="op-panel" aria-labelledby="op-register-title"><header><h2 id="op-register-title">Opportunity directory</h2><p>Filter the directory or open an opportunity to review its full details.</p></header><div id="coordinator-opportunity-list" class="op-list"></div><p data-op-empty hidden>No opportunities have been published yet.</p></section>
  <dialog class="op-dialog" aria-labelledby="op-detail-title"><header><div><p class="op-eyebrow">OPPORTUNITY DETAILS</p><h2 id="op-detail-title"></h2></div><button type="button" data-op-close aria-label="Close details">×</button></header><div data-op-details></div><footer><button type="button" data-op-close>Close details</button></footer></dialog>
</section>
</asp:Content>
