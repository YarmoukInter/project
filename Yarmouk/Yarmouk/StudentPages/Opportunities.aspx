<%@ Page Title="Explore Opportunities" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="Opportunities.aspx.cs" Inherits="Yarmouk.StudentPages.Opportunities" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/StudentPages/Opportunities.css?v=apply-modal-1") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/StudentPages/Opportunities.js?v=apply-modal-1") %>" defer="defer"></script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<script src="../Scripts/Shared/record-lists.js?v=organized-2" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <section class="opportunities-page">
        <section class="opportunities-hero" aria-label="Career development">
            <div class="hero-copy">
                <p class="hero-kicker"><span></span> YARMOUK UNIVERSITY <b>•</b> CAREER DEVELOPMENT</p>
                <h1>Your future<br />starts with an <em>opportunity.</em></h1>
                <p class="hero-description">Discover internships, develop your skills, and turn your academic knowledge into real-world experience.</p>
                <a class="hero-cta" href="#opportunityList">Explore opportunities <span aria-hidden="true">↘</span></a>
                <p class="hero-motto"><span aria-hidden="true">✦</span> Learn today. Lead tomorrow.</p>
            </div>
            <div class="hero-art" aria-hidden="true">
                <div class="hero-orbit orbit-one"></div><div class="hero-orbit orbit-two"></div>
                <div class="hero-note"><span class="hero-cap">✦</span><small>YOUR NEXT CHAPTER</small><strong>Starts here.</strong><i>✦</i></div>
                <span class="hero-float float-top">⌁ &nbsp; Grow your skills</span>
                <span class="hero-float float-bottom">▣ &nbsp; Gain experience</span>
            </div>
        </section>
        <header class="opportunities-heading">
            <div>
                <p class="eyebrow">FIND YOUR NEXT STEP</p>
                <h1>Explore opportunities</h1>
                <p class="heading-copy">Browse internships, review the details, and find a role that fits your major.</p>
            </div>
            <a class="applications-shortcut" href="<%= ResolveUrl("~/StudentPages/MyApplications.aspx") %>">My applications <span aria-hidden="true">→</span></a>
        </header>

        <section class="opportunity-toolbar" aria-label="Search and filter opportunities">
            <label class="search-field"><span aria-hidden="true">⌕</span><input id="opportunitySearch" type="search" placeholder="Search roles, companies or locations" /></label>
            <select id="opportunityField" aria-label="Filter by field"><option value="">All fields</option></select>
        </section>

        <div class="opportunity-layout">
            <section class="opportunity-results" aria-live="polite">
                <div class="results-heading"><h2>Internship opportunities</h2><span id="opportunityCount">0 opportunities</span></div>
                <div class="opportunity-list" id="opportunityList"></div>
                <div class="empty-state" id="opportunityEmpty" hidden>No opportunities match your search right now.</div>
            </section>
            <aside class="opportunity-detail" id="opportunityDetail" aria-live="polite">
                <div class="detail-empty"><span class="detail-mark">Y</span><h2>Choose an opportunity</h2><p>Select an internship to see its full details.</p></div>
            </aside>
        </div>

        <p class="page-feedback" id="opportunityFeedback" role="status"><asp:Literal ID="OpportunityFeedback" runat="server" /></p>
        <asp:HiddenField ID="SelectedOpportunityId" runat="server" />
        <asp:HiddenField ID="ApplicationAnswers" runat="server" />
        <div class="opportunity-modal" id="opportunityModal" hidden>
            <div class="opportunity-modal-backdrop" data-close-modal></div>
            <section class="opportunity-modal-panel" role="dialog" aria-modal="true" aria-labelledby="applicationModalTitle" tabindex="-1">
                <button type="button" class="opportunity-modal-close" data-close-modal aria-label="Close application">×</button>
                <span class="modal-eyebrow">INTERNSHIP APPLICATION</span>
                <h2 id="applicationModalTitle">Apply for an opportunity</h2>
                <p class="modal-intro" id="applicationModalIntro"></p>
                <div class="modal-section"><h3>Your information</h3><div class="modal-student-grid" id="applicationStudent"></div></div>
                <div class="modal-section"><h3>Opportunity details</h3><div id="applicationOpportunity"></div></div>
                <div class="modal-section"><h3>Company questions</h3><div id="applicationQuestions"></div></div>
                <p class="modal-error" id="applicationError" role="alert" hidden></p>
                <div class="modal-actions"><button type="button" class="modal-cancel" data-close-modal>Cancel</button><asp:Button ID="SubmitApplication" runat="server" Text="Submit application" CssClass="modal-submit" OnClick="SubmitApplication_Click" OnClientClick="return prepareOpportunityApplication();" /></div>
            </section>
        </div>
        <script id="opportunities-data" type="application/json"><asp:Literal ID="OpportunityData" runat="server" Mode="PassThrough" /></script>
        <script id="opportunity-student-data" type="application/json"><asp:Literal ID="StudentData" runat="server" Mode="PassThrough" /></script>
    </section>
</asp:Content>
