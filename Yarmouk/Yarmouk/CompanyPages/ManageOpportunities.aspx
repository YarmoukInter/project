<%@ Page Title="Manage Opportunities" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="ManageOpportunities.aspx.cs" Inherits="Yarmouk.CompanyPages.ManageOpportunities" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link href='<%= ResolveUrl("~/Content/CompanyPages/ManageOpportunities.css") %>' rel="stylesheet" />
    <script src='<%= ResolveUrl("~/Scripts/CompanyPages/ManageOpportunities.js") %>?v=3' defer="defer"></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <section class="co-page co-opportunities" aria-labelledby="opportunitiesTitle" data-api-url='<%= ResolveUrl("~/CompanyPages/ManageOpportunities.aspx") %>'>
        <header class="co-page-heading">
            <div>
                <p class="co-eyebrow">COMPANY WORKSPACE</p>
                <h1 id="opportunitiesTitle">Manage opportunities</h1>
                <p class="co-subtitle">Create and manage your internship opportunities, set details, and track applications.</p>
            </div>
            <button class="co-button co-button-primary" type="button" id="openOpportunityForm">
                <span aria-hidden="true">+</span> Add opportunity
            </button>
        </header>

        <div class="co-stat-grid" id="opportunityStats" aria-label="Opportunity summary"></div>

        <section class="co-panel" aria-labelledby="opportunityListTitle">
            <div class="co-panel-heading">
                <div>
                    <h2 id="opportunityListTitle">Your opportunities</h2>
                    <p>Review availability and keep each posting up to date.</p>
                </div>
                <span class="co-count-pill" id="opportunityCount">0 opportunities</span>
            </div>
            <div class="co-toolbar">
                <label class="co-search"><span aria-hidden="true">⌕</span><input type="search" id="opportunitySearch" placeholder="Search opportunities…" aria-label="Search opportunities" /></label>
                <label class="co-filter-label" for="opportunityStatusFilter">Status</label>
                <select id="opportunityStatusFilter" class="co-select" aria-label="Filter opportunities by status">
                    <option value="all">All statuses</option><option value="active">Active</option><option value="draft">Draft</option><option value="closed">Closed</option>
                </select>
            </div>
            <div class="co-table-wrap">
                <table class="co-table">
                    <thead><tr><th scope="col">Internship title</th><th scope="col">Eligible majors</th><th scope="col">Seats</th><th scope="col">Deadline</th><th scope="col">Status</th><th scope="col">Actions</th></tr></thead>
                    <tbody id="opportunityRows"></tbody>
                </table>
            </div>
            <p class="co-empty-state" id="opportunityEmpty" hidden="hidden">No opportunities match these filters.</p>
        </section>
        <p class="co-demo-note" id="opportunityFeedback" role="status">Loading opportunities from Firebase…</p>

        <div class="co-modal-backdrop" id="opportunityModal" hidden="hidden">
            <section class="co-modal" role="dialog" aria-modal="true" aria-labelledby="opportunityModalTitle">
                <header class="co-modal-heading"><div><p class="co-eyebrow">OPPORTUNITY DETAILS</p><h2 id="opportunityModalTitle">Add opportunity</h2></div><button class="co-icon-button" type="button" data-close-opportunity-modal aria-label="Close">×</button></header>
                <div id="opportunityForm">
                    <div class="co-form-grid">
                        <label class="co-field co-field-wide">Internship title<input id="opportunityTitleInput" name="title" maxlength="120" required="required" placeholder="e.g. Software Developer Intern" /></label>
                        <label class="co-field">Application deadline<input id="opportunityDeadlineInput" name="deadline" type="date" required="required" /></label>
                        <label class="co-field">Available seats<input id="opportunitySeatsInput" name="seats" type="number" min="1" max="500" required="required" value="1" /></label>
                        <label class="co-field">Duration<input id="opportunityDurationInput" name="duration" maxlength="80" placeholder="e.g. 8 weeks" /></label>
                        <label class="co-field">Required training hours<input id="opportunityHoursInput" name="hours" type="number" min="0" max="10000" value="0" /></label>
                        <label class="co-field">Work mode<select id="opportunityWorkModeInput" name="workMode"><option>OnSite</option><option>Hybrid</option><option>Remote</option></select></label>
                        <fieldset class="co-field co-field-wide co-major-field"><legend>Eligible majors</legend><label><input type="checkbox" name="major" value="CIS" /> Computer Information Systems</label><label><input type="checkbox" name="major" value="CS" /> Computer Science</label><label><input type="checkbox" name="major" value="SE" /> Software Engineering</label></fieldset>
                        <label class="co-field co-field-wide">Description<textarea id="opportunityDescriptionInput" name="description" rows="3" maxlength="1500" required="required" placeholder="Describe the internship and the trainee’s responsibilities."></textarea></label>
                        <label class="co-field co-field-wide">Requirements<textarea id="opportunityRequirementsInput" name="requirements" rows="2" maxlength="1000" placeholder="Skills, prerequisites, or other requirements."></textarea></label>
                        <label class="co-field co-field-wide">Application questions<textarea id="opportunityQuestionsInput" name="questions" rows="3" placeholder="One question per line"></textarea><small>Students will answer these questions when they apply.</small></label>
                    </div>
                    <div class="co-modal-actions"><button class="co-button co-button-quiet" type="button" data-close-opportunity-modal>Cancel</button><button class="co-button co-button-primary" type="button" id="saveOpportunityButton">Save opportunity</button></div>
                </div>
            </section>
        </div>
    </section>
</asp:Content>
