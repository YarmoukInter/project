<%@ Page Title="Explore Opportunities" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="Opportunities.aspx.cs" Inherits="Yarmouk.StudentPages.Opportunities" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/StudentPages/Opportunities.css") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/StudentPages/Opportunities.js") %>" defer></script>
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
                <div class="results-heading"><h2>Available internships</h2><span id="opportunityCount">0 opportunities</span></div>
                <div class="opportunity-list" id="opportunityList"></div>
                <div class="empty-state" id="opportunityEmpty" hidden>No opportunities match your search right now.</div>
            </section>
            <aside class="opportunity-detail" id="opportunityDetail" aria-live="polite">
                <div class="detail-empty"><span class="detail-mark">Y</span><h2>Choose an opportunity</h2><p>Select an internship to see its full details.</p></div>
            </aside>
        </div>

        <p class="page-feedback" id="opportunityFeedback" role="status"></p>
        <script id="opportunities-data" type="application/json"><asp:Literal ID="OpportunityData" runat="server" /></script>
    </section>
</asp:Content>
