<%@ Page Title="Student Dashboard" Language="C#" MasterPageFile="~/StudentPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Yarmouk.StudentPages.Dashboard" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/StudentPages/Dashboard.css?v=responsive-task-cards-1") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/StudentPages/Dashboard.js?v=admin-alignment-1") %>" defer></script>
<link href="<%= ResolveUrl("~/Content/dashboard-overview.css") %>?v=admin-alignment-1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<div class="dashboard role-dashboard">

            <section class="page-heading">
                <div>
                    <span class="overview-eyebrow">Yarmouk University &middot; Student training</span><h1>Student Dashboard</h1>
                    <p>An overview of your internship journey.</p>
                </div>

            </section>

            <asp:Panel ID="DashboardNotice" runat="server" CssClass="deadline-alert" Visible="false" role="status"><div class="alert-text"><i class="bi bi-info-circle" aria-hidden="true"></i><asp:Literal ID="DashboardNoticeText" runat="server" /></div></asp:Panel>

            <section class="deadline-alert" aria-label="Deadline reminder">

                <div class="alert-text">
                    <i class="bi bi-clipboard-check" aria-hidden="true"></i>
                    <span><asp:Literal ID="DeadlinesMessage" runat="server" Text="—" /></span>
                </div>

                <a href="#upcomingTasks" class="text-link">
                    View deadlines
                    <i class="bi bi-arrow-right" aria-hidden="true"></i>
                </a>

            </section>

<div class="overview-caption"><span>At a glance</span><small>Current training activity</small></div>
            <section class="stats-grid" aria-label="Internship summary">

                <a class="stat-card" href="<%= ResolveUrl("~/StudentPages/MyApplications.aspx") %>">
                    <div class="stat-icon">
                        <i class="bi bi-file-earmark-text" aria-hidden="true"></i>
                    </div>

                    <div class="stat-content">
                        <h2>Applications</h2>

                        <div class="stat-number">
                            <asp:Literal ID="ApplicationsCount"
                                         runat="server"
                                         Text="—" />
                        </div>

                        <p><asp:Literal ID="ApplicationsSummary" runat="server" Text="Application status summary" /></p>
                    </div>
                </a>

                <a class="stat-card" href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>">
                    <div class="stat-icon">
                        <i class="bi bi-check2-square" aria-hidden="true"></i>
                    </div>

                    <div class="stat-content">
                        <h2>Tasks</h2>

                        <div class="stat-number">
                            <asp:Literal ID="TasksCount"
                                         runat="server"
                                         Text="—" />
                        </div>

                        <p><asp:Literal ID="TasksSummary" runat="server" Text="Task status summary" /></p>
                    </div>
                </a>

                <a class="stat-card" href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>">
                    <div class="stat-icon">
                        <i class="bi bi-calendar4-week" aria-hidden="true"></i>
                    </div>

                    <div class="stat-content">
                        <h2>Upcoming Deadlines</h2>

                        <div class="stat-number">
                            <asp:Literal ID="DeadlinesCount"
                                         runat="server"
                                         Text="—" />
                        </div>

                        <p>Due within the next 7 days</p>
                    </div>
                </a>

                <a class="stat-card" href="<%= ResolveUrl("~/StudentPages/Progress.aspx") %>">
                    <div class="stat-icon">
                        <i class="bi bi-bar-chart" aria-hidden="true"></i>
                    </div>

                    <div class="stat-content">
                        <h2>Evaluations</h2>

                        <div class="stat-number">
                            <asp:Literal ID="EvaluationsCount"
                                         runat="server"
                                         Text="—" />
                        </div>

                        <p>Reviewed task submissions</p>
                    </div>
                </a>

            </section>

            <div class="main-grid">

                <section class="panel tasks-panel" id="upcomingTasks">

                    <div class="panel-heading">
                        <h2>Tasks &amp; Deadlines</h2>

                        <a href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>" class="text-link">
                            View all tasks
                            <i class="bi bi-arrow-right" aria-hidden="true"></i>
                        </a>
                    </div>

                    <div class="upcoming-task-list" aria-label="Upcoming tasks, deadlines and submission statuses">
                        <asp:Repeater ID="UpcomingTasks" runat="server">
                            <ItemTemplate>
                                <article class="upcoming-task-card">
                                    <div class="upcoming-task-main">
                                        <div class="upcoming-task-title-row">
                                            <h3><%#: Eval("Title") %></h3>
                                            <span class='badge <%# Eval("BadgeClass") %>'><span class="status-dot"></span><%#: Eval("Status") %></span>
                                        </div>
                                        <p class="upcoming-task-description"><%#: Eval("Description") %></p>
                                    </div>
                                    <div class="upcoming-task-footer">
                                        <div class="upcoming-task-deadline"><i class="bi bi-calendar3" aria-hidden="true"></i><span><small>Due date</small><time datetime='<%# Eval("DeadlineIso") %>'><%#: Eval("Deadline") %></time><span class="task-deadline-note"><%#: Eval("DeadlineNote") %></span></span></div>
                                        <a href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>" class="upcoming-task-open">Open task <i class="bi bi-arrow-up-right" aria-hidden="true"></i></a>
                                    </div>
                                </article>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <asp:Panel ID="NoUpcomingTasks" runat="server" Visible="false"><p class="muted">No tasks have been assigned to your current placement yet.</p></asp:Panel>

                </section>

                <section class="panel progress-panel">

                    <div class="panel-heading">
                        <h2>Internship Progress</h2>
                        <span class="training-status"><asp:Literal ID="InternshipStatus" runat="server" Text="Training status unavailable" /></span>
                    </div>

                    <div class="hours-summary">

                        <div id="DashboardProgressRing" runat="server" class="progress-ring"
                             role="img"
                             aria-label="Internship progress">
                            <div class="progress-ring-center"><asp:Literal ID="DashboardProgressPercent" runat="server" Text="—" /></div>
                        </div>

                        <div class="hours-text">
                            <h3>Approved training hours</h3>

                            <p class="hours-value">
                                <asp:Literal ID="ApprovedHours" runat="server" Text="—" />
                                /
                                <asp:Literal ID="RequiredHours" runat="server" Text="—" />
                                hours
                            </p>

                            <p class="muted">Approved hours count toward your required training hours.</p>
                        </div>

                    </div>

                    <div class="task-progress">

                        <div class="progress-label">
                            <span>Submitted tasks</span>
                            <span><asp:Literal ID="TaskCompletionLabel" runat="server" Text="—" /></span>
                        </div>

                        <div class="progress-bar-row">
                            <progress id="DashboardTaskProgress" runat="server" class="task-progress-bar"
                                      value="0"
                                      max="1"
                                      aria-label="Submitted tasks">
                                0%
                            </progress>

                            <span class="progress-percentage"><asp:Literal ID="DashboardTaskProgressPercent" runat="server" Text="0" />%</span>
                        </div>

                    </div>

                    <p class="progress-note">
                        <i class="bi bi-info-circle" aria-hidden="true"></i>
                        <span>Submit 10 tasks to reach 100%. Each task adds 10%.</span>
                    </p>

                </section>

            </div>

            <div class="bottom-grid">

                <section class="panel">

                    <div class="panel-heading">
                        <h2>Recent Applications</h2>

                        <a href="<%= ResolveUrl("~/StudentPages/MyApplications.aspx") %>" class="text-link">
                            View all
                            <i class="bi bi-arrow-right" aria-hidden="true"></i>
                        </a>
                    </div>

                    <div class="table-container">
                        <table>
                            <caption class="visually-hidden">
                                Recent internship applications
                            </caption>

                            <thead>
                                <tr>
                                    <th scope="col">Company</th>
                                    <th scope="col">Opportunity</th>
                                    <th scope="col">Status</th>
                                </tr>
                            </thead>

                            <tbody>
                                <asp:Repeater ID="RecentApplications" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td><span class="cell-title"><%#: Eval("Company") %></span></td>
                                            <td><%#: Eval("Opportunity") %></td>
                                            <td><span class='badge <%# Eval("BadgeClass") %>'><span class="status-dot"></span><%#: Eval("Status") %></span></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>

                        </table>
                    </div>
                    <asp:Panel ID="NoRecentApplications" runat="server" Visible="false"><p class="muted">You have not submitted any applications yet.</p></asp:Panel>

                </section>

                <section class="panel">

                    <div class="panel-heading">
                        <h2>Latest Task Evaluations</h2>

                        <a href="<%= ResolveUrl("~/StudentPages/Progress.aspx") %>" class="text-link">
                            View evaluations
                            <i class="bi bi-arrow-right" aria-hidden="true"></i>
                        </a>
                    </div>

                    <div class="table-container">
                        <table class="evaluations-table">
                            <caption class="visually-hidden">
                                Recent task evaluations and supervisor feedback
                            </caption>

                            <thead>
                                <tr>
                                    <th scope="col">Task</th>
                                    <th scope="col">Feedback</th>
                                    <th scope="col">Status</th>
                                </tr>
                            </thead>

                            <tbody>
                                <asp:Repeater ID="RecentReviews" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td><span class="cell-title"><%#: Eval("Task") %></span><span class="cell-subtitle"><%#: Eval("Date") %></span></td>
                                            <td><%#: Eval("Feedback") %></td>
                                            <td><span class='badge <%# Eval("BadgeClass") %>'><%#: Eval("Status") %></span></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>

                        </table>
                    </div>
                    <asp:Panel ID="NoRecentReviews" runat="server" Visible="false"><p class="muted">No reviewed task submissions yet.</p></asp:Panel>

                </section>

            </div>

            <section class="placement-strip" aria-label="Current internship details">

                <div class="placement-item">
                    <i class="bi bi-briefcase" aria-hidden="true"></i>

                    <div>
                        <span class="placement-label">Current placement:</span>
                        <span class="placement-value"><asp:Literal ID="PlacementCompany" runat="server" Text="No confirmed placement" /></span>
                    </div>
                </div>

                <div class="placement-item">
                    <i class="bi bi-person" aria-hidden="true"></i>

                    <div>
                        <span class="placement-label">Company supervisor:</span>
                        <span class="placement-value"><asp:Literal ID="CompanySupervisor" runat="server" Text="—" /></span>
                    </div>
                </div>

                <div class="placement-item">
                    <i class="bi bi-mortarboard" aria-hidden="true"></i>

                    <div>
                        <span class="placement-label">Academic supervisor:</span>
                        <span class="placement-value"><asp:Literal ID="AcademicSupervisor" runat="server" Text="—" /></span>
                    </div>
                </div>

            </section>

            <footer class="footer">
                <span>UIMS · Yarmouk University</span>
                <span>Student training overview</span>
            </footer>


<section class="overview-workspace" aria-labelledby="workspaceTitle"><h2 id="workspaceTitle">Your student workspace</h2><nav aria-label="Student workspace"><a href="<%= ResolveUrl("~/StudentPages/Opportunities.aspx") %>"><span><strong>Find opportunities</strong><small>Explore available internships</small></span><span aria-hidden="true">&#8594;</span></a>
<a href="<%= ResolveUrl("~/StudentPages/MyApplications.aspx") %>"><span><strong>Your applications</strong><small>Track application decisions</small></span><span aria-hidden="true">&#8594;</span></a>
<a href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>"><span><strong>Training tasks</strong><small>Assignments and submissions</small></span><span aria-hidden="true">&#8594;</span></a>
<a href="<%= ResolveUrl("~/StudentPages/Progress.aspx") %>"><span><strong>Your progress</strong><small>Hours and supervisor feedback</small></span><span aria-hidden="true">&#8594;</span></a></nav></section>
</div>
</asp:Content>
