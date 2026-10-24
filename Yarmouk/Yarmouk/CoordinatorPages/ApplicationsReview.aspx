<%@ Page Async="true" Title="Opportunity applications" Language="C#" MasterPageFile="~/CoordinatorPages/Section.Master" AutoEventWireup="true" CodeBehind="ApplicationsReview.aspx.cs" Inherits="Yarmouk.CoordinatorPages.ApplicationsReview" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=1" rel="stylesheet" />
<link href="../Content/CoordinatorPages/ManageOpportunities.css?v=applications-3" rel="stylesheet" />
<script src="../Scripts/CoordinatorPages/ApplicationsReview.js?v=2" defer></script>
<script src="../Scripts/Shared/record-lists.js?v=applications-directory-4" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<script type="application/json" id="backend-data"><%= BackendJson %></script>
<section class="op-admin app-admin" aria-labelledby="application-title">
 <header class="op-hero"><div><p class="op-eyebrow">COORDINATOR WORKSPACE</p><h1 id="application-title">Opportunity applications</h1><p data-application-scope>Review student applications and their current status.</p></div><a class="op-link" href="ManageOpportunities.aspx">← Opportunities</a></header>
 <p data-application-feedback role="status" hidden></p>
 <section class="op-panel" aria-labelledby="application-list-title"><header><h2 id="application-list-title">Applicants <span data-application-count></span></h2><p>Search applicants, filter by status, and open a request to see its answers.</p><a class="op-link" data-all-applications href="ApplicationsReview.aspx" hidden>View all applications</a></header><div id="coordinator-application-list" class="op-list"></div><p data-application-empty hidden>No applications have been received for this opportunity yet.</p></section>
 <dialog class="op-dialog" aria-labelledby="application-detail-title"><header><div><p class="op-eyebrow">APPLICATION DETAILS</p><h2 id="application-detail-title"></h2></div><button type="button" data-application-close aria-label="Close application details">×</button></header><div data-application-details></div><footer><button type="button" data-application-close>Close details</button></footer></dialog>
</section>
</asp:Content>
