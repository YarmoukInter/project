<%@ Page Title="My Applications" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="MyApplications.aspx.cs" Inherits="Yarmouk.StudentPages.MyApplications" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/StudentPages/MyApplications.css") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/StudentPages/MyApplications.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <section class="my-applications-page">
        <header class="applications-hero">
            <div><p class="applications-kicker">STUDENT WORKSPACE</p><h1>My Applications</h1><p>Track your applications and follow every update in one place.</p></div>
            <a class="explore-shortcut" href="<%= ResolveUrl("~/StudentPages/Opportunities.aspx") %>">Explore opportunities <span aria-hidden="true">→</span></a>
            <aside class="one-placement-note"><span class="placement-icon" aria-hidden="true">i</span><div><strong>One internship at a time</strong><p>You can apply to multiple opportunities, but you can accept only one internship in the end.</p></div></aside>
        </header>
        <section class="applications-toolbar">
            <div class="application-filters" role="group" aria-label="Filter applications">
                <button type="button" class="filter-button active" data-filter="all">All Applications</button>
                <button type="button" class="filter-button" data-filter="pending">Pending</button>
                <button type="button" class="filter-button" data-filter="accepted">Accepted</button>
                <button type="button" class="filter-button" data-filter="rejected">Rejected</button>
            </div>
            <label class="application-search"><span aria-hidden="true">⌕</span><input id="applicationSearch" type="search" placeholder="Search by role or company" /></label>
            <select class="application-sort" id="applicationSort" aria-label="Sort applications"><option value="newest">Latest First</option><option value="oldest">Oldest First</option></select>
        </section>
        <asp:Literal ID="ApplicationsMessage" runat="server" />
        <section class="application-list" id="applicationList" aria-live="polite"></section>
        <div class="applications-empty" id="applicationsEmpty" hidden><span class="empty-mark">Y</span><h2>No applications to show</h2><p>When you apply for an opportunity, it will appear here.</p><a href="<%= ResolveUrl("~/StudentPages/Opportunities.aspx") %>">Explore opportunities</a></div>
        <p class="page-feedback" id="applicationsFeedback" role="status"></p>
        <div class="application-modal" id="applicationDetailsModal" hidden role="dialog" aria-modal="true" aria-labelledby="applicationModalTitle">
            <div class="application-modal-card"><button type="button" class="application-modal-close" data-close-details aria-label="Close">×</button><p class="eyebrow">APPLICATION DETAILS</p><h2 id="applicationModalTitle"></h2><div id="applicationModalBody"></div></div>
        </div>
        <asp:HiddenField ID="AcceptApplicationId" runat="server" />
        <asp:Button ID="AcceptApplicationPostback" runat="server" Text="Confirm offer" CssClass="accept-postback" CausesValidation="false" OnClick="AcceptApplication_Click" />
        <script id="my-applications-data" type="application/json"><asp:Literal ID="ApplicationsData" runat="server" /></script>
    </section>
</asp:Content>
