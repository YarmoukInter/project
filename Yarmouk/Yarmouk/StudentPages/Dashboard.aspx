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

                <div class="heading-tools"><span class="preview-label">Sample data preview</span><div class="dashboard-account"><span class="avatar"><asp:Literal ID="StudentInitials" runat="server" Text="ST" /></span><span class="account-copy"><strong><asp:Literal ID="StudentName" runat="server" Text="Student" /></strong><small>Student</small></span></div></div>
            </section>

            <section class="deadline-alert" aria-label="Deadline reminder">

                <div class="alert-text">
                    <i class="bi bi-clipboard-check" aria-hidden="true"></i>
                    <span>2 tasks due this week</span>
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
                                         Text="4" />
                        </div>

                        <p>2 pending · 1 confirmed · 1 rejected</p>
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
                                         Text="12" />
                        </div>

                        <p>9 completed · 3 active</p>
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
                                         Text="2" />
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
                                         Text="8" />
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
                                <tr>
                                    <td>
                                        <span class="cell-title">Login page interface</span>
                                        <span class="cell-subtitle">Web Development</span>
                                    </td>

                                    <td>
                                        <time datetime="2026-10-22">22 Oct 2026</time>
                                    </td>

                                    <td>
                                        <span class="badge badge-warning">
                                            <span class="status-dot"></span>
                                            In progress
                                        </span>
                                    </td>

                                    <td>
                                        <a href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>" class="text-link">
                                            Open
                                            <i class="bi bi-arrow-right" aria-hidden="true"></i>
                                        </a>
                                    </td>
                                </tr>

                                <tr>
                                    <td>
                                        <span class="cell-title">Weekly training report</span>
                                        <span class="cell-subtitle">Training Reports</span>
                                    </td>

                                    <td>
                                        <time datetime="2026-10-24">24 Oct 2026</time>
                                    </td>

                                    <td>
                                        <span class="badge badge-neutral">
                                            <span class="status-dot"></span>
                                            Not started
                                        </span>
                                    </td>

                                    <td>
                                        <a href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>" class="text-link">
                                            Open
                                            <i class="bi bi-arrow-right" aria-hidden="true"></i>
                                        </a>
                                    </td>
                                </tr>

                                <tr>
                                    <td>
                                        <span class="cell-title">Database schema update</span>
                                        <span class="cell-subtitle">Database Development</span>
                                    </td>

                                    <td>
                                        <time datetime="2026-10-26">26 Oct 2026</time>
                                    </td>

                                    <td>
                                        <span class="badge badge-danger">
                                            <span class="status-dot"></span>
                                            Revision requested
                                        </span>
                                    </td>

                                    <td>
                                        <a href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>" class="text-link">
                                            Resubmit
                                            <i class="bi bi-arrow-right" aria-hidden="true"></i>
                                        </a>
                                    </td>
                                </tr>
                            </tbody>

                        </table>
                    </div>

                </section>

                <section class="panel progress-panel">

                    <div class="panel-heading">
                        <h2>Internship Progress</h2>
                        <span class="training-status">Active training</span>
                    </div>

                    <div class="hours-summary">

                        <div class="progress-ring"
                             role="img"
                             aria-label="50 percent of training hours approved">
                            <div class="progress-ring-center">50%</div>
                        </div>

                        <div class="hours-text">
                            <h3>Approved training hours</h3>

                            <p class="hours-value">
                                <asp:Literal ID="ApprovedHours"
                                             runat="server"
                                             Text="120" />
                                /
                                <asp:Literal ID="RequiredHours"
                                             runat="server"
                                             Text="240" />
                                hours
                            </p>

                            <p class="muted">Illustrative training target</p>
                        </div>

                    </div>

                    <div class="task-progress">

                        <div class="progress-label">
                            <span>Completed tasks</span>
                            <span>9 of 12</span>
                        </div>

                        <div class="progress-bar-row">
                            <progress class="task-progress-bar"
                                      value="9"
                                      max="12"
                                      aria-label="Completed tasks">
                                75%
                            </progress>

                            <span class="progress-percentage">75%</span>
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
                                <tr>
                                    <td>
                                        <span class="cell-title">Orange Jordan</span>
                                    </td>

                                    <td>Software Development Intern</td>

                                    <td>
                                        <span class="badge badge-success">
                                            <span class="status-dot"></span>
                                            Confirmed
                                        </span>
                                    </td>
                                </tr>

                                <tr>
                                    <td>
                                        <span class="cell-title">ProgressSoft</span>
                                    </td>

                                    <td>Web Development Intern</td>

                                    <td>
                                        <span class="badge badge-warning">
                                            <span class="status-dot"></span>
                                            Pending
                                        </span>
                                    </td>
                                </tr>
                            </tbody>

                        </table>
                    </div>

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
                                <tr>
                                    <td>
                                        <span class="cell-title">Weekly Report 02</span>
                                        <span class="cell-subtitle">Reviewed · 19 Oct</span>
                                    </td>

                                    <td>Clear structure and well documented activities.</td>

                                    <td>
                                        <span class="badge badge-success">
                                            Meets expectations
                                        </span>
                                    </td>
                                </tr>

                                <tr>
                                    <td>
                                        <span class="cell-title">Database Schema</span>
                                        <span class="cell-subtitle">Reviewed · 18 Oct</span>
                                    </td>

                                    <td>Update the relationships and submit again.</td>

                                    <td>
                                        <span class="badge badge-warning">
                                            Revision requested
                                        </span>
                                    </td>
                                </tr>
                            </tbody>

                        </table>
                    </div>

                </section>

            </div>

            <section class="placement-strip" aria-label="Current internship details">

                <div class="placement-item">
                    <i class="bi bi-briefcase" aria-hidden="true"></i>

                    <div>
                        <span class="placement-label">Current placement:</span>
                        <span class="placement-value">Orange Jordan</span>
                    </div>
                </div>

                <div class="placement-item">
                    <i class="bi bi-person" aria-hidden="true"></i>

                    <div>
                        <span class="placement-label">Company supervisor:</span>
                        <span class="placement-value">Eng. Sara Ahmad</span>
                    </div>
                </div>

                <div class="placement-item">
                    <i class="bi bi-mortarboard" aria-hidden="true"></i>

                    <div>
                        <span class="placement-label">Academic supervisor:</span>
                        <span class="placement-value">Dr. Ahmad Omar</span>
                    </div>
                </div>

            </section>

            <footer class="footer">
                <span>UIMS · Yarmouk University</span>
                <span>Sample data for mockup</span>
            </footer>

        </div>
</asp:Content>
