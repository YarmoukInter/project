<%@ Page Title="Academic Dashboard" Language="C#" MasterPageFile="~/AcademicPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Yarmouk.AcademicPages.Dashboard" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/AcademicPages/Dashboard.css" rel="stylesheet" />
<script src="../Scripts/AcademicPages/Dashboard.js" defer></script>
<link href="<%= ResolveUrl("~/Content/dashboard-overview.css") %>?v=admin-alignment-1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="academic-dashboard role-dashboard" aria-labelledby="dashboardTitle">
    <header class="academic-dashboard-heading">
        <div>
            <span class="academic-eyebrow">Yarmouk University &middot; Academic supervision</span>
            <h1 id="dashboardTitle">Academic Dashboard</h1>
            <p>Your overview of assigned students, their placements, and the work awaiting your review.</p>
        </div>
        <div class="dashboard-heading-actions">
            <time id="dashboardDate" class="dashboard-date"></time>
            <a class="academic-button academic-button--primary" href="StudentProgress.aspx">Open student progress <span aria-hidden="true">↗</span></a>
        </div>
    </header>

    <div class="preview-notice" role="note"><span aria-hidden="true">ⓘ</span><span>Live overview from your assigned students and their training records.</span><span class="notice-period">Academic supervisor view</span></div>

    <asp:Panel ID="DashboardError" runat="server" CssClass="dashboard-empty dashboard-load-error" Visible="false">
        <div class="empty-mark" aria-hidden="true">!</div><h3>Dashboard data couldn't load</h3><p><asp:Literal ID="DashboardErrorText" runat="server" /></p>
    </asp:Panel>

<div class="overview-caption"><span>At a glance</span><small>Current training activity</small></div>
    <section class="academic-metrics" aria-label="Your assigned students summary">
        <a class="academic-metric metric-green" href="StudentProgress.aspx"><span class="metric-icon" aria-hidden="true">◎</span><span class="metric-label">Assigned students</span><strong class="metric-value"><asp:Literal ID="AssignedStudentsCount" runat="server">—</asp:Literal></strong><small>Students linked to your supervision</small></a>
        <a class="academic-metric metric-blue" href="StudentProgress.aspx"><span class="metric-icon" aria-hidden="true">↗</span><span class="metric-label">Active placements</span><strong class="metric-value"><asp:Literal ID="ActivePlacementsCount" runat="server">—</asp:Literal></strong><small>Currently completing internships</small></a>
        <a class="academic-metric metric-amber" href="Tasks.aspx"><span class="metric-icon" aria-hidden="true">▤</span><span class="metric-label">Submissions to review</span><strong class="metric-value"><asp:Literal ID="SubmissionsToReviewCount" runat="server">—</asp:Literal></strong><small>Awaiting your review</small></a>
        <a class="academic-metric metric-violet" href="FinalEvaluation.aspx"><span class="metric-icon" aria-hidden="true">✓</span><span class="metric-label">Evaluations ready</span><strong class="metric-value"><asp:Literal ID="EvaluationsReadyCount" runat="server">—</asp:Literal></strong><small>Company evaluations received</small></a>
    </section>

    <section class="academic-dashboard-panel" aria-labelledby="studentsTitle">
        <div class="dashboard-panel-heading">
            <div><span class="academic-eyebrow">Your caseload</span><h2 id="studentsTitle">Assigned students</h2><p>A quick look at placement status, approved hours, and task activity.</p></div>
            <div class="dashboard-list-tools"><label class="dashboard-search-label" for="studentSearch">Search students</label><input id="studentSearch" class="dashboard-search" type="search" placeholder="Student or company" autocomplete="off" /><span class="caseload-count"><asp:Literal ID="CaseloadCount" runat="server">—</asp:Literal></span></div>
        </div>
        <asp:Panel ID="StudentsEmpty" runat="server" CssClass="dashboard-empty" Visible="false">
            <div class="empty-mark" aria-hidden="true">◎</div><h3>No assigned students yet</h3><p>Students assigned to your supervision will appear here.</p>
        </asp:Panel>
        <asp:Panel ID="StudentTablePanel" runat="server" CssClass="dashboard-table-wrap" Visible="false">
            <table class="academic-students-table">
                <thead><tr><th scope="col">Student</th><th scope="col">Company</th><th scope="col">Placement</th><th scope="col">Training progress</th><th scope="col">Task activity</th></tr></thead>
                <tbody>
                    <asp:Repeater ID="AssignedStudents" runat="server">
                        <ItemTemplate>
                            <tr data-student-row>
                                <td><span class="student-cell"><span class="student-avatar uims-student-photo-avatar" data-student-photo-url='<%# Html(Eval("ProfileImageUrl")) %>'><span><%# Html(Eval("Initials")) %></span></span><span><strong><%# Html(Eval("StudentName")) %></strong><small>ID <%# Html(Eval("StudentNumber")) %></small></span></span></td>
                                <td><span class="company-name"><%# Html(Eval("CompanyName")) %></span></td>
                                <td><span class="placement-badge <%# PlacementBadgeClass(Eval("PlacementStatus")) %>"><%# PlacementStatusLabel(Eval("PlacementStatus")) %></span></td>
                                <td><div class="training-cell"><div class="training-label"><span><%# Html(Eval("TaskProgressLabel")) %></span><strong><%# Eval("ProgressPercent") %>%</strong></div><div class="training-track" role="progressbar" aria-label="<%# Html(Eval("StudentName")) %> training progress" aria-valuemin="0" aria-valuemax="100" aria-valuenow="<%# Eval("ProgressPercent") %>"><span style="width:<%# Eval("ProgressPercent") %>%"></span></div></div></td>
                                <td><span class="task-summary"><strong><%# Html(Eval("TaskProgressLabel")) %></strong><small><%# TaskReviewLabel(Eval("SubmittedTasks")) %></small></span></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </asp:Panel>
        <p id="studentSearchEmpty" class="search-empty" hidden>No assigned students match your search.</p>
        <a class="academic-panel-link" href="StudentProgress.aspx">View all assigned students <span aria-hidden="true">→</span></a>
    </section>

    <div class="academic-dashboard-footer"><span>Each submitted task adds 10% toward the 10-task training target.</span><a href="FinalEvaluation.aspx">Review final evaluations <span aria-hidden="true">→</span></a></div>

<section class="overview-workspace" aria-labelledby="workspaceTitle"><h2 id="workspaceTitle">Your academic workspace</h2><nav aria-label="Academic workspace"><a href="<%= ResolveUrl("~/AcademicPages/StudentProgress.aspx") %>"><span><strong>Student progress</strong><small>Placements and training activity</small></span><span aria-hidden="true">&#8594;</span></a>
<a href="<%= ResolveUrl("~/AcademicPages/Tasks.aspx") %>"><span><strong>Task reviews</strong><small>Submissions and feedback</small></span><span aria-hidden="true">&#8594;</span></a>
<a href="<%= ResolveUrl("~/AcademicPages/FinalEvaluation.aspx") %>"><span><strong>Final evaluations</strong><small>Company reviews and results</small></span><span aria-hidden="true">&#8594;</span></a>
<a href="<%= ResolveUrl("~/AcademicPages/Account.aspx") %>"><span><strong>Your account</strong><small>Supervisor account details</small></span><span aria-hidden="true">&#8594;</span></a></nav></section>
</section>
</asp:Content>
