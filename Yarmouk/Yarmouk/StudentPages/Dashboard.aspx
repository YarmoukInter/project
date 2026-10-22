<%@ Page Title="Student Dashboard" Language="C#" MasterPageFile="~/StudentPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Yarmouk.StudentPages.Dashboard" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/StudentPages/Dashboard.css") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/StudentPages/Dashboard.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<div class="dashboard">

            <section class="page-heading">
                <div>
                    <h1>Student Dashboard</h1>
                    <p>An overview of your internship journey.</p>
                </div>

                <div class="heading-tools"><div class="dashboard-account"><span class="avatar"><asp:Literal ID="StudentInitials" runat="server" Text="ST" /></span><span class="account-copy"><strong><asp:Literal ID="StudentName" runat="server" Text="Student" /></strong><small>Student</small></span></div></div>
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

            <section class="stats-grid" aria-label="Internship summary">

                <article class="stat-card">
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
                </article>

                <article class="stat-card">
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
                </article>

                <article class="stat-card">
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
                </article>

                <article class="stat-card">
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
                </article>

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

                    <div class="table-container">
                        <table>
                            <caption class="visually-hidden">
                                Upcoming tasks, deadlines and submission statuses
                            </caption>

                            <thead>
                                <tr>
                                    <th scope="col">Task</th>
                                    <th scope="col">Deadline</th>
                                    <th scope="col">Status</th>
                                    <th scope="col">Action</th>
                                </tr>
                            </thead>

                            <tbody>
                                <asp:Repeater ID="UpcomingTasks" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td><span class="cell-title"><%#: Eval("Title") %></span><span class="cell-subtitle"><%#: Eval("Description") %></span></td>
                                            <td><time datetime='<%# Eval("DeadlineIso") %>'><%#: Eval("Deadline") %></time></td>
                                            <td><span class='badge <%# Eval("BadgeClass") %>'><span class="status-dot"></span><%#: Eval("Status") %></span></td>
                                            <td><a href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>" class="text-link">Open <i class="bi bi-arrow-right" aria-hidden="true"></i></a></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>

                        </table>
                    </div>
                    <asp:Panel ID="NoUpcomingTasks" runat="server" Visible="false"><p class="muted">No tasks are due within the next seven days.</p></asp:Panel>

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
                            <span>Completed tasks</span>
                            <span><asp:Literal ID="TaskCompletionLabel" runat="server" Text="—" /></span>
                        </div>

                        <div class="progress-bar-row">
                            <progress id="DashboardTaskProgress" runat="server" class="task-progress-bar"
                                      value="0"
                                      max="1"
                                      aria-label="Completed tasks">
                                0%
                            </progress>

                            <span class="progress-percentage"><asp:Literal ID="DashboardTaskProgressPercent" runat="server" Text="0" />%</span>
                        </div>

                    </div>

                    <p class="progress-note">
                        <i class="bi bi-info-circle" aria-hidden="true"></i>
                        <span>Training hours require supervisor approval.</span>
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

        </div>
</asp:Content>
