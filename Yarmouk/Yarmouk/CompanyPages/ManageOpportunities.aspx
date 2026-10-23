<%@ Page Title="Manage Opportunities" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="ManageOpportunities.aspx.cs" Inherits="Yarmouk.CompanyPages.ManageOpportunities" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link href='<%= ResolveUrl("~/Content/CompanyPages/ManageOpportunities.css") %>' rel="stylesheet" />
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<script src="../Scripts/Shared/record-lists.js?v=organized-2" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <section class="co-page co-opportunities" aria-labelledby="opportunitiesTitle">
        <header class="co-page-heading">
            <div><p class="co-eyebrow">COMPANY WORKSPACE</p><h1 id="opportunitiesTitle">Manage opportunities</h1><p class="co-subtitle">Create and manage your internship opportunities, set details, and track applications.</p></div>
            <asp:Button ID="OpenOpportunityFormButton" runat="server" CssClass="co-button co-button-primary" Text="＋ Add opportunity" CausesValidation="false" OnClick="OpenOpportunityFormButton_Click" />
        </header>

        <div class="co-stat-grid" aria-label="Opportunity summary">
            <article class="co-stat-card"><span class="co-stat-icon">▣</span><span class="co-stat-copy"><span>Active opportunities</span><strong><asp:Literal ID="ActiveCountLiteral" runat="server" /></strong></span></article>
            <article class="co-stat-card"><span class="co-stat-icon amber">▤</span><span class="co-stat-copy"><span>Draft opportunities</span><strong><asp:Literal ID="DraftCountLiteral" runat="server" /></strong></span></article>
            <article class="co-stat-card"><span class="co-stat-icon">♧</span><span class="co-stat-copy"><span>Total applications</span><strong><asp:Literal ID="ApplicationCountLiteral" runat="server" /></strong></span></article>
        </div>

        <section class="co-panel" aria-labelledby="opportunityListTitle">
            <div class="co-panel-heading"><div><h2 id="opportunityListTitle">Your opportunities</h2><p>Review availability and keep each posting up to date.</p></div><asp:Label ID="OpportunityCountLabel" runat="server" CssClass="co-count-pill" /></div>
            <div class="co-toolbar">
                <label class="co-search"><span aria-hidden="true">⌕</span><asp:TextBox ID="OpportunitySearchTextBox" runat="server" TextMode="Search" placeholder="Search opportunities…" aria-label="Search opportunities" /></label>
                <label class="co-filter-label" for="OpportunityStatusFilter">Status</label>
                <asp:DropDownList ID="OpportunityStatusFilter" runat="server" CssClass="co-select"><asp:ListItem Value="all">All statuses</asp:ListItem><asp:ListItem Value="active">Active</asp:ListItem><asp:ListItem Value="draft">Draft</asp:ListItem><asp:ListItem Value="closed">Closed</asp:ListItem></asp:DropDownList>
                <asp:Button ID="ApplyFiltersButton" runat="server" CssClass="co-button co-button-quiet" Text="Apply" CausesValidation="false" OnClick="ApplyFiltersButton_Click" />
            </div>
            <div class="co-table-wrap"><table class="co-table">
                <thead><tr><th scope="col">Internship title</th><th scope="col">Eligible majors</th><th scope="col">Seats</th><th scope="col">Deadline</th><th scope="col">Status</th><th scope="col">Actions</th></tr></thead>
                <tbody><asp:Repeater ID="OpportunitiesRepeater" runat="server" OnItemCommand="OpportunitiesRepeater_ItemCommand"><ItemTemplate>
                    <tr>
                        <td><span class="co-opportunity-title"><%# Encode(Eval("Title")) %></span><span class="co-opportunity-subtitle"><%# Encode(Eval("Id")) %> · <%# Eval("Applications") %> applications</span></td>
                        <td><span class="co-major-list"><%# RenderMajors(Eval("Majors")) %></span></td>
                        <td><span class="co-seat-value"><%# Eval("Seats") %> seats</span></td><td><%# FormatDeadline(Eval("Deadline")) %></td>
                        <td><span class='<%# StatusCss(Eval("Status")) %>'><%# StatusLabel(Eval("Status")) %></span></td>
                        <td><span class="co-row-actions"><asp:LinkButton runat="server" CssClass="co-text-action" Text="Edit" CommandName="EditOpportunity" CommandArgument='<%# Eval("Id") %>' CausesValidation="false" /><asp:LinkButton runat="server" CssClass='<%# ToggleCss(Eval("Status")) %>' Text='<%# ToggleLabel(Eval("Status")) %>' CommandName="ToggleStatus" CommandArgument='<%# Eval("Id") %>' CausesValidation="false" /></span></td>
                    </tr>
                </ItemTemplate></asp:Repeater></tbody>
            </table></div>
            <asp:Panel ID="EmptyStatePanel" runat="server" CssClass="co-empty-state" Visible="false">No opportunities match these filters.</asp:Panel>
        </section>
        <asp:Label ID="PageFeedbackLabel" runat="server" CssClass="co-demo-note" role="status" />

        <asp:Panel ID="OpportunityEditorPanel" runat="server" CssClass="co-modal-backdrop" Visible="false">
            <section class="co-modal" role="dialog" aria-modal="true" aria-labelledby="opportunityModalTitle">
                <header class="co-modal-heading"><div><p class="co-eyebrow">OPPORTUNITY DETAILS</p><h2 id="opportunityModalTitle"><asp:Literal ID="EditorTitleLiteral" runat="server" /></h2></div><asp:Button ID="CloseEditorButton" runat="server" CssClass="co-icon-button" Text="×" CausesValidation="false" OnClick="CloseEditorButton_Click" aria-label="Close" /></header>
                <asp:HiddenField ID="EditingOpportunityId" runat="server" /><asp:HiddenField ID="EditingOpportunityStatus" runat="server" />
                <div class="co-form-grid">
                    <label class="co-field co-field-wide">Internship title<asp:TextBox ID="OpportunityTitleInput" runat="server" MaxLength="120" required="required" placeholder="e.g. Software Developer Intern" /></label>
                    <label class="co-field">Application deadline (optional)<asp:TextBox ID="OpportunityDeadlineInput" runat="server" TextMode="Date" /></label>
                    <label class="co-field">Available seats<asp:TextBox ID="OpportunitySeatsInput" runat="server" TextMode="Number" min="1" max="500" required="required" /></label>
                    <label class="co-field">Duration<asp:TextBox ID="OpportunityDurationInput" runat="server" MaxLength="80" placeholder="e.g. 8 weeks" /></label>
                    <label class="co-field">Required training hours<asp:TextBox ID="OpportunityHoursInput" runat="server" TextMode="Number" min="0" max="10000" /></label>
                    <label class="co-field">Work mode<asp:DropDownList ID="OpportunityWorkModeInput" runat="server"><asp:ListItem>OnSite</asp:ListItem><asp:ListItem>Hybrid</asp:ListItem><asp:ListItem>Remote</asp:ListItem></asp:DropDownList></label>
                    <fieldset class="co-field co-field-wide co-major-field"><legend>Eligible majors</legend><label><asp:CheckBox ID="MajorCisCheckBox" runat="server" /> Computer Information Systems</label><label><asp:CheckBox ID="MajorCsCheckBox" runat="server" /> Computer Science</label><label><asp:CheckBox ID="MajorSeCheckBox" runat="server" /> Software Engineering</label></fieldset>
                    <label class="co-field co-field-wide">Description<asp:TextBox ID="OpportunityDescriptionInput" runat="server" TextMode="MultiLine" Rows="3" MaxLength="1500" required="required" placeholder="Describe the internship and the trainee’s responsibilities." /></label>
                    <label class="co-field co-field-wide">Requirements<asp:TextBox ID="OpportunityRequirementsInput" runat="server" TextMode="MultiLine" Rows="2" MaxLength="1000" placeholder="Skills, prerequisites, or other requirements." /></label>
                    <label class="co-field co-field-wide">Application questions<asp:TextBox ID="OpportunityQuestionsInput" runat="server" TextMode="MultiLine" Rows="3" placeholder="One question per line" /><small>Students will answer these questions when they apply.</small></label>
                </div>
                <asp:Label ID="EditorFeedbackLabel" runat="server" CssClass="co-modal-feedback" Visible="false" role="alert" />
                <div class="co-modal-actions"><asp:Button ID="CancelEditorButton" runat="server" CssClass="co-button co-button-quiet" Text="Cancel" CausesValidation="false" OnClick="CloseEditorButton_Click" /><asp:Button ID="SaveOpportunityButton" runat="server" CssClass="co-button co-button-primary" Text="Save opportunity" OnClick="SaveOpportunityButton_Click" /></div>
            </section>
        </asp:Panel>
    </section>
</asp:Content>
