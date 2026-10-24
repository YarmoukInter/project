<%@ Page Title="Applications Review" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="ApplicationsReview.aspx.cs" Inherits="Yarmouk.CompanyPages.ApplicationsReview" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
    <link href='<%= ResolveUrl("~/Content/CompanyPages/ApplicationsReview.css") %>?v=horizontal-pages-8' rel="stylesheet" />
<script src="../Scripts/Shared/record-lists.js?v=record-cards-3" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <section class="co-page co-applications" aria-labelledby="applicationsTitle">
        <header class="co-page-heading"><div><p class="co-eyebrow">COMPANY WORKSPACE</p><h1 id="applicationsTitle">Applications review</h1><p class="co-subtitle">Review student applications and make a decision for each opportunity.</p></div></header>
        <asp:Label ID="PageFeedbackLabel" runat="server" CssClass="co-demo-note co-review-feedback" role="status" />

        <div class="co-review-stats" aria-label="Application summary">
            <article class="co-review-stat"><span class="co-review-stat-icon">♧</span><span class="co-review-stat-copy"><span>New applications</span><strong><asp:Literal ID="NewCountLiteral" runat="server" /></strong></span></article>
            <article class="co-review-stat"><span class="co-review-stat-icon">◷</span><span class="co-review-stat-copy"><span>In review</span><strong><asp:Literal ID="ReviewCountLiteral" runat="server" /></strong></span></article>
            <article class="co-review-stat"><span class="co-review-stat-icon">✓</span><span class="co-review-stat-copy"><span>Accepted</span><strong><asp:Literal ID="AcceptedCountLiteral" runat="server" /></strong></span></article>
            <article class="co-review-stat"><span class="co-review-stat-icon">×</span><span class="co-review-stat-copy"><span>Rejected</span><strong><asp:Literal ID="RejectedCountLiteral" runat="server" /></strong></span></article>
        </div>

        <section class="co-review-layout" aria-label="Application review workspace">
            <div class="co-panel co-candidate-panel">
                <div class="co-panel-heading co-candidate-heading"><div><h2>Candidate applications</h2><asp:Label ID="ApplicationResultsCountLabel" runat="server" /></div></div>
                <div class="co-toolbar co-review-toolbar">
                    <label class="co-search"><span aria-hidden="true">⌕</span><asp:TextBox ID="ApplicationSearchTextBox" runat="server" TextMode="Search" placeholder="Search student name or major…" aria-label="Search applications" /></label>
                    <asp:DropDownList ID="ApplicationOpportunityFilter" runat="server" CssClass="co-select" aria-label="Filter applications by opportunity"><asp:ListItem Value="all">All opportunities</asp:ListItem></asp:DropDownList>
                    <asp:DropDownList ID="ApplicationStatusFilter" runat="server" CssClass="co-select" aria-label="Filter applications by status"><asp:ListItem Value="all">All statuses</asp:ListItem><asp:ListItem Value="new">New</asp:ListItem><asp:ListItem Value="in-review">In review</asp:ListItem><asp:ListItem Value="offered">Offer sent</asp:ListItem><asp:ListItem Value="accepted">Accepted</asp:ListItem><asp:ListItem Value="rejected">Rejected</asp:ListItem><asp:ListItem Value="withdrawn">Withdrawn</asp:ListItem></asp:DropDownList>
                    <asp:Button ID="ApplyFiltersButton" runat="server" CssClass="co-button co-button-quiet" Text="Apply" CausesValidation="false" OnClick="ApplyFiltersButton_Click" />
                </div>
                <div class="co-candidate-list" aria-label="Candidate applications"><asp:Repeater ID="ApplicationsRepeater" runat="server"><ItemTemplate>
                    <article class='co-candidate-card <%# CandidateRowCss(Eval("Id")) %>' data-status='<%# Attribute(Eval("Status")) %>'>
                        <div class="co-candidate-identity"><span class="co-candidate-avatar uims-student-photo-avatar" data-student-photo-url='<%# Encode(Eval("ProfileImageUrl")) %>'><span><%# Encode(Initials(Eval("Name"))) %></span></span><div><span class="co-student-name"><%# Encode(Eval("Name")) %></span><span class="co-student-number"><%# Encode(Eval("Number")) %></span></div></div>
                        <div class="co-candidate-meta"><span class="co-card-label">Major</span><span class="co-major-cell"><%# Encode(Eval("Major")) %></span></div>
                        <div class="co-candidate-meta co-candidate-applied"><span class="co-card-label">Applied</span><span><%# FormatDate(Eval("AppliedAtUnixMs")) %></span></div>
                        <div class="co-candidate-status"><span class="co-card-label">Status</span><span class='<%# StatusCss(Eval("Status")) %>'><%# StatusLabel(Eval("Status")) %></span></div>
                        <button type="button" class="co-row-open" data-application-open="<%# Attribute(Eval("Id")) %>" aria-label="View application details">View details <span aria-hidden="true">→</span></button>
                    </article>
                </ItemTemplate></asp:Repeater></div>
                <asp:Panel ID="EmptyStatePanel" runat="server" CssClass="co-empty-state" Visible="false">No applications match these filters.</asp:Panel>
            </div>

        </section>

        <asp:HiddenField ID="SelectedApplicationId" runat="server" ClientIDMode="Static" />
        <script type="application/json" id="applicationsReviewData"><asp:Literal ID="ApplicationsDataLiteral" runat="server" /></script>
        <div id="applicationDetailModal" class="co-modal-backdrop co-application-modal-backdrop" hidden>
            <section class="co-modal co-application-modal" role="dialog" aria-modal="true" aria-labelledby="applicationModalTitle">
                    <header class="co-detail-head"><div><p class="co-eyebrow">APPLICATION DETAILS</p><h2 id="applicationModalTitle" data-detail-name></h2><p data-detail-summary></p></div><div class="co-detail-head-actions"><span data-detail-status></span><button type="button" class="co-icon-button" data-application-close aria-label="Close application details">×</button></div></header>
                    <div class="co-profile-strip"><span class="co-profile-avatar uims-student-photo-avatar" data-detail-photo data-student-photo-url=""><span data-detail-initials></span></span><div><strong data-detail-name-secondary></strong><span data-detail-number></span></div></div>
                    <div class="co-detail-facts"><span class="co-detail-fact"><span>Email</span><strong data-detail-email></strong></span><span class="co-detail-fact"><span>Applied</span><strong data-detail-applied></strong></span><span class="co-detail-fact"><span>Opportunity</span><strong data-detail-opportunity></strong></span></div>
                    <section class="co-detail-section"><h3>CV / RESUME</h3><a data-detail-cv class="co-cv-link" target="_blank" rel="noopener noreferrer">Open student CV</a><p data-detail-no-cv class="co-empty-copy">No CV attached to this profile.</p></section>
                    <section class="co-detail-section"><h3>COMPANY QUESTIONS &amp; ANSWERS</h3><div data-detail-answers></div></section>
                    <div data-detail-note class="co-decision-note" hidden></div>
                    <div id="applicationDecisionActions" class="co-detail-actions"><asp:Button ID="AcceptApplicationButton" runat="server" ClientIDMode="Static" CssClass="co-button co-button-primary" Text="Accept application" CausesValidation="false" formnovalidate="formnovalidate" OnClick="AcceptApplicationButton_Click" /><button id="openRejectButton" type="button" class="co-button co-button-danger">Reject application</button></div>
                    <div id="acceptanceBlockedMessage" class="co-blocked-note" hidden></div>
                    <div class="co-modal-actions co-application-modal-actions"><button type="button" class="co-button co-button-quiet" data-application-close>Close</button></div>
            </section>
        </div>

        <div id="rejectionModal" class="co-modal-backdrop" hidden>
            <section class="co-modal co-rejection-modal" role="dialog" aria-modal="true" aria-labelledby="rejectionModalTitle">
                <header class="co-modal-heading"><div><p class="co-eyebrow">APPLICATION DECISION</p><h2 id="rejectionModalTitle">Reject application</h2></div><button type="button" class="co-icon-button" data-rejection-close aria-label="Close">×</button></header>
                <label class="co-field">Reason for rejection<asp:TextBox ID="RejectionReasonInput" runat="server" TextMode="MultiLine" Rows="4" MaxLength="500" required="required" placeholder="Share a brief reason with the student." /></label>
                <asp:Label ID="RejectionFeedbackLabel" runat="server" CssClass="co-modal-feedback" Visible="false" role="alert" />
                <div class="co-modal-actions"><button type="button" class="co-button co-button-quiet" data-rejection-close>Cancel</button><asp:Button ID="ConfirmRejectionButton" runat="server" CssClass="co-button co-button-danger" Text="Confirm rejection" OnClick="ConfirmRejectionButton_Click" /></div>
            </section>
        </div>
    </section>
    <script src='<%= ResolveUrl("~/Scripts/CompanyPages/ApplicationsReview.js") %>?v=4' defer></script>
</asp:Content>
