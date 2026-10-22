<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="Dashboard.aspx.cs" Inherits="Yarmouk.CompanyPages.Dashboard" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link rel="preconnect" href="https://fonts.googleapis.com" />
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
<link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
<link href="<%= ResolveUrl("~/Content/CompanyPages/Dashboard.css") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/Dashboard.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<div class="company-dashboard">
    <section class="dashboard-heading" aria-labelledby="dashboardTitle">
        <div>
            <span class="eyebrow"><span class="eyebrow-dot"></span> Company portal</span>
            <h1 id="dashboardTitle">Company Dashboard</h1>
            <p>A clear overview of your opportunities, applicants, trainees, and reviews.</p>
        </div>
        <div class="heading-actions">
            <span class="snapshot-date"><i class="bi bi-calendar3" aria-hidden="true"></i><time id="dashboardDate"></time></span>
            <a class="button button-primary" href="<%= ResolveUrl("~/CompanyPages/ManageOpportunities.aspx") %>"><i class="bi bi-plus-lg" aria-hidden="true"></i> Add opportunity</a>
        </div>
    </section>

    <div id="DashboardNotice" runat="server" class="preview-notice" role="status" visible="false">
        <i class="bi bi-info-circle" aria-hidden="true"></i>
        <span><asp:Literal ID="DashboardNoticeText" runat="server" /></span>
        <span class="notice-period">This week</span>
    </div>
    <div id="PreviewNotice" runat="server" class="preview-notice" role="note" visible="false">
        <i class="bi bi-info-circle" aria-hidden="true"></i><span>Dashboard preview with sample information</span><span class="notice-period">This week</span>
    </div>

    <section class="metrics-grid" aria-label="Company overview">
        <a class="metric-card" href="<%= ResolveUrl("~/CompanyPages/ManageOpportunities.aspx") %>">
            <span class="metric-icon"><i class="bi bi-briefcase" aria-hidden="true"></i></span>
            <span class="metric-label">Active opportunities</span>
            <strong class="metric-value"><asp:Literal ID="ActiveOpportunitiesCount" runat="server" Text="0" /></strong>
            <span class="metric-foot"><span class="metric-highlight"><asp:Literal ID="AllOpportunitiesCount" runat="server" Text="0" /></span> total opportunities</span>
        </a>
        <a class="metric-card" href="<%= ResolveUrl("~/CompanyPages/ApplicationsReview.aspx") %>">
            <span class="metric-icon icon-blue"><i class="bi bi-person-lines-fill" aria-hidden="true"></i></span>
            <span class="metric-label">Applications</span>
            <strong class="metric-value"><asp:Literal ID="NewApplicationsCount" runat="server" Text="0" /></strong>
            <span class="metric-foot"><span class="metric-highlight"><asp:Literal ID="AwaitingApplicationsCount" runat="server" Text="0" /></span> awaiting review</span>
        </a>
        <a class="metric-card" href="<%= ResolveUrl("~/CompanyPages/MyTrainees.aspx") %>">
            <span class="metric-icon icon-violet"><i class="bi bi-people" aria-hidden="true"></i></span>
            <span class="metric-label">Current trainees</span>
            <strong class="metric-value"><asp:Literal ID="CurrentTraineesCount" runat="server" Text="0" /></strong>
            <span class="metric-foot">Across <span class="metric-highlight"><asp:Literal ID="TraineeOpportunitiesCount" runat="server" Text="0" /></span> opportunities</span>
        </a>
        <a class="metric-card" href="<%= ResolveUrl("~/CompanyPages/ManageTasks.aspx") %>">
            <span class="metric-icon icon-gold"><i class="bi bi-list-check" aria-hidden="true"></i></span>
            <span class="metric-label">Open tasks</span>
            <strong class="metric-value"><asp:Literal ID="OpenTasksCount" runat="server" Text="0" /></strong>
            <span class="metric-foot"><span class="metric-highlight"><asp:Literal ID="TasksDueThisWeekCount" runat="server" Text="0" /></span> due this week</span>
        </a>
        <a class="metric-card" href="#reviewQueue">
            <span class="metric-icon icon-coral"><i class="bi bi-inbox" aria-hidden="true"></i></span>
            <span class="metric-label">Submissions to review</span>
            <strong class="metric-value"><asp:Literal ID="SubmissionsToReviewCount" runat="server" Text="0" /></strong>
            <span class="metric-foot">Across <span class="metric-highlight"><asp:Literal ID="SubmissionTraineesCount" runat="server" Text="0" /></span> trainees</span>
        </a>
        <a class="metric-card" href="<%= ResolveUrl("~/CompanyPages/FinalEvaluation.aspx") %>">
            <span class="metric-icon icon-teal"><i class="bi bi-clipboard2-check" aria-hidden="true"></i></span>
            <span class="metric-label">Evaluations due</span>
            <strong class="metric-value"><asp:Literal ID="EvaluationsDueCount" runat="server" Text="0" /></strong>
            <span class="metric-foot">Ready for your review</span>
        </a>
    </section>

    <div class="dashboard-grid">
        <section class="panel activity-panel" id="reviewQueue" aria-labelledby="queueTitle">
            <div class="panel-heading">
                <div>
                    <h2 id="queueTitle">Needs your attention</h2>
                    <p>Review recent applications and trainee submissions.</p>
                </div>
                <label class="search-box"><i class="bi bi-search" aria-hidden="true"></i><span class="visually-hidden">Search queue</span><input id="queueSearch" type="search" placeholder="Search" autocomplete="off" /></label>
            </div>

            <div class="queue-tabs" role="tablist" aria-label="Review queue">
                <button class="queue-tab is-active" type="button" role="tab" id="applicationsTab" aria-selected="true" aria-controls="applicationsQueue" data-queue-tab="applicationsQueue">Applications <span><asp:Literal ID="ApplicationsTabCount" runat="server" Text="0" /></span></button>
                <button class="queue-tab" type="button" role="tab" id="submissionsTab" aria-selected="false" aria-controls="submissionsQueue" data-queue-tab="submissionsQueue" tabindex="-1">Task submissions <span><asp:Literal ID="SubmissionsTabCount" runat="server" Text="0" /></span></button>
            </div>

            <div class="queue-content" id="applicationsQueue" role="tabpanel" aria-labelledby="applicationsTab" data-queue-panel>
                <div class="table-wrap">
                    <table class="dashboard-table">
                        <thead><tr><th scope="col">Applicant</th><th scope="col">Opportunity</th><th scope="col">Applied</th><th scope="col">Status</th><th scope="col"><span class="visually-hidden">Action</span></th></tr></thead>
                        <tbody>
                            <asp:Repeater ID="ApplicationsRepeater" runat="server"><ItemTemplate>
                                <tr data-search-row><td><span class="person-cell"><span class="person-avatar"><%#: Eval("Initials") %></span><span><strong><%#: Eval("Name") %></strong><small><%#: Eval("Major") %></small></span></span></td><td><%#: Eval("Opportunity") %></td><td><%#: Eval("Applied") %></td><td><span class="status-pill status-pending">Pending review</span></td><td><a class="row-action" href="<%= ResolveUrl("~/CompanyPages/ApplicationsReview.aspx") %>" aria-label="Review application"><i class="bi bi-arrow-up-right" aria-hidden="true"></i></a></td></tr>
                            </ItemTemplate></asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <p id="NoApplications" runat="server" class="empty-state" visible="false">No pending applications.</p><p class="empty-state" data-empty-state hidden>No matching applications.</p>
                <a class="panel-link" href="<%= ResolveUrl("~/CompanyPages/ApplicationsReview.aspx") %>">Review all applications <i class="bi bi-arrow-right" aria-hidden="true"></i></a>
            </div>

            <div class="queue-content" id="submissionsQueue" role="tabpanel" aria-labelledby="submissionsTab" data-queue-panel hidden>
                <div class="table-wrap">
                    <table class="dashboard-table">
                        <thead><tr><th scope="col">Trainee</th><th scope="col">Task</th><th scope="col">Submitted</th><th scope="col">Status</th><th scope="col"><span class="visually-hidden">Action</span></th></tr></thead>
                        <tbody>
                            <asp:Repeater ID="SubmissionsRepeater" runat="server"><ItemTemplate>
                                <tr data-search-row><td><span class="person-cell"><span class="person-avatar"><%#: Eval("Initials") %></span><span><strong><%#: Eval("Name") %></strong><small>Trainee</small></span></span></td><td><%#: Eval("Task") %></td><td><%#: Eval("Submitted") %></td><td><span class="status-pill status-review">Needs review</span></td><td><a class="row-action" href="<%= ResolveUrl("~/CompanyPages/ManageTasks.aspx") %>" aria-label="Review submission"><i class="bi bi-arrow-up-right" aria-hidden="true"></i></a></td></tr>
                            </ItemTemplate></asp:Repeater>
                        </tbody>
                    </table>
                </div>
                <p id="NoSubmissions" runat="server" class="empty-state" visible="false">No submissions waiting for review.</p><p class="empty-state" data-empty-state hidden>No matching submissions.</p>
                <a class="panel-link" href="<%= ResolveUrl("~/CompanyPages/ManageTasks.aspx") %>">Open task management <i class="bi bi-arrow-right" aria-hidden="true"></i></a>
            </div>
        </section>

        <section class="panel opportunities-panel" aria-labelledby="opportunitiesTitle">
            <div class="panel-heading">
                <div><h2 id="opportunitiesTitle">Opportunity overview</h2><p>Current openings at your company.</p></div>
                <a class="icon-link" href="<%= ResolveUrl("~/CompanyPages/ManageOpportunities.aspx") %>" aria-label="Manage opportunities"><i class="bi bi-arrow-up-right" aria-hidden="true"></i></a>
            </div>
            <div class="opportunity-list">
                <asp:Repeater ID="OpportunitiesRepeater" runat="server"><ItemTemplate>
                    <article class="opportunity-item"><span class="opportunity-symbol"><i class="bi bi-briefcase" aria-hidden="true"></i></span><div class="opportunity-info"><strong><%#: Eval("Title") %></strong><span><%#: Eval("Applicants") %> applicants · <%#: Eval("Seats") %> seats</span><div class="seat-track"><span style='width:<%#: Eval("Fill") %>%'></span></div></div><span class="opportunity-state"><%#: Eval("State") %></span></article>
                </ItemTemplate></asp:Repeater>
            </div>
            <p id="NoOpportunities" runat="server" class="empty-state" visible="false">No opportunities found.</p>
            <a class="panel-link" href="<%= ResolveUrl("~/CompanyPages/ManageOpportunities.aspx") %>">Manage opportunities <i class="bi bi-arrow-right" aria-hidden="true"></i></a>
        </section>
    </div>

    <div class="dashboard-grid lower-grid">
        <section class="panel trainees-panel" aria-labelledby="traineesTitle">
            <div class="panel-heading"><div><h2 id="traineesTitle">Trainee progress</h2><p>Active trainees and their task completion.</p></div><a class="text-link" href="<%= ResolveUrl("~/CompanyPages/MyTrainees.aspx") %>">View trainees <i class="bi bi-arrow-right" aria-hidden="true"></i></a></div>
            <div class="trainee-list"><asp:Repeater ID="TraineesRepeater" runat="server"><ItemTemplate>
                <div class="trainee-row"><span class="person-avatar"><%#: Eval("Initials") %></span><div class="trainee-name"><strong><%#: Eval("Name") %></strong><small><%#: Eval("Program") %></small></div><div class="trainee-progress"><div class="progress-caption"><span>Tasks completed</span><strong><%#: Eval("Completed") %> of <%#: Eval("Total") %></strong></div><div class="progress-track"><span style='width:<%#: Eval("Percent") %>%'></span></div></div><span class="progress-value"><%#: Eval("Percent") %>%</span></div>
            </ItemTemplate></asp:Repeater></div>
            <p id="NoTrainees" runat="server" class="empty-state" visible="false">No active trainees found.</p>
        </section>
        <section class="panel evaluation-panel" aria-labelledby="evaluationTitle">
            <div class="panel-heading"><div><h2 id="evaluationTitle">Evaluations</h2><p>Final reviews waiting for completion.</p></div><span class="evaluation-count"><asp:Literal ID="EvaluationSummary" runat="server" Text="0" /> due</span></div>
            <div class="evaluation-summary"><span class="evaluation-icon"><i class="bi bi-clipboard2-check" aria-hidden="true"></i></span><div><strong><asp:Literal ID="EvaluationDueCaption" runat="server" Text="0" /> trainee evaluations</strong><p>Complete the company evaluation for trainees approaching the end of their placement.</p></div></div>
            <a class="button button-outline" href="<%= ResolveUrl("~/CompanyPages/FinalEvaluation.aspx") %>">Go to final evaluations <i class="bi bi-arrow-right" aria-hidden="true"></i></a>
        </section>
    </div>

    <p class="dashboard-footnote"><i class="bi bi-shield-check" aria-hidden="true"></i> Trainee information is available to authorized company supervisors.</p>
</div>
</asp:Content>
