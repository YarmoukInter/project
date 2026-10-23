<%@ Page Title="Applications Review" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="ApplicationsReview.aspx.cs" Inherits="Yarmouk.CompanyPages.ApplicationsReview" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link href='<%= ResolveUrl("~/Content/CompanyPages/ApplicationsReview.css") %>?v=3' rel="stylesheet" />
    <script src='<%= ResolveUrl("~/Scripts/CompanyPages/ApplicationsReview.js") %>?v=4' defer="defer"></script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<script src="../Scripts/Shared/record-lists.js?v=organized-2" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <section class="co-page co-applications" aria-labelledby="applicationsTitle" data-api-url='<%= ResolveUrl("~/CompanyPages/ApplicationsReview") %>'>
        <header class="co-page-heading">
            <div><p class="co-eyebrow">COMPANY WORKSPACE</p><h1 id="applicationsTitle">Applications review</h1><p class="co-subtitle">Review student applications and make a decision for each opportunity.</p></div>
        </header>
        <p class="co-demo-note co-review-feedback" id="applicationFeedback" role="status" aria-live="polite">Loading applications…</p>

        <div class="co-review-stats" id="applicationStats" aria-label="Application summary"></div>

        <section class="co-review-layout" aria-label="Application review workspace">
            <div class="co-panel co-candidate-panel">
                <div class="co-panel-heading co-candidate-heading"><div><h2>Candidate applications</h2><p id="applicationResultsCount">0 applications</p></div></div>
                <div class="co-toolbar co-review-toolbar">
                    <label class="co-search"><span aria-hidden="true">⌕</span><input type="search" id="applicationSearch" placeholder="Search student name or major…" aria-label="Search applications" /></label>
                    <select class="co-select" id="applicationOpportunityFilter" aria-label="Filter applications by opportunity"><option value="all">All opportunities</option></select>
                    <select class="co-select" id="applicationStatusFilter" aria-label="Filter applications by status"><option value="all">All statuses</option><option value="new">New</option><option value="in-review">In review</option><option value="accepted">Accepted</option><option value="rejected">Rejected</option></select>
                </div>
                <div class="co-table-wrap co-candidate-table-wrap">
                    <table class="co-table co-candidate-table"><thead><tr><th scope="col">Student</th><th scope="col">Major</th><th scope="col">Applied</th><th scope="col">Status</th><th scope="col"><span class="co-visually-hidden">View</span></th></tr></thead><tbody id="applicationRows"></tbody></table>
                </div>
                <p class="co-empty-state" id="applicationEmpty" hidden="hidden">No applications match these filters.</p>
            </div>

            <aside class="co-panel co-candidate-detail" id="candidateDetail" aria-live="polite" aria-label="Selected applicant details">
                <div class="co-detail-empty"><span class="co-detail-mark" aria-hidden="true">Y</span><h2>Select an application</h2><p>Choose a student from the list to review their details.</p></div>
            </aside>
        </section>
        <div class="co-modal-backdrop" id="rejectionModal" hidden="hidden">
            <section class="co-modal co-rejection-modal" role="dialog" aria-modal="true" aria-labelledby="rejectionModalTitle">
                <header class="co-modal-heading"><div><p class="co-eyebrow">APPLICATION DECISION</p><h2 id="rejectionModalTitle">Reject application</h2></div><button class="co-icon-button" type="button" data-close-rejection aria-label="Close">×</button></header>
                <label class="co-field">Reason for rejection<textarea id="rejectionReasonInput" rows="4" maxlength="500" required="required" placeholder="Share a brief reason with the student."></textarea></label>
                <div class="co-modal-actions"><button class="co-button co-button-quiet" type="button" data-close-rejection>Cancel</button><button class="co-button co-button-danger" id="confirmRejection" type="button">Confirm rejection</button></div>
            </section>
        </div>
    </section>
</asp:Content>
