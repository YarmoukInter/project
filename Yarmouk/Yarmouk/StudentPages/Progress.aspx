<%@ Page Title="My Progress" Language="C#" MasterPageFile="~/StudentPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="Progress.aspx.cs" Inherits="Yarmouk.StudentPages.Progress" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/StudentPages/Progress.css") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/StudentPages/Progress.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <section class="progress-page" aria-labelledby="progress-title">
        <header class="progress-heading">
            <div>
                <p class="eyebrow">STUDENT TRAINING</p>
                <h1 id="progress-title">My Progress</h1>
                <p class="heading-copy">A clear view of your internship hours and your path to completion.</p>
            </div>
            <div class="program-pill"><span class="status-dot"></span> Internship journey</div>
        </header>

        <asp:Panel ID="ProgressNotice" runat="server" CssClass="progress-notice" Visible="false" role="status">
            <span class="notice-icon" aria-hidden="true">i</span>
            <asp:Literal ID="NoticeText" runat="server" />
        </asp:Panel>

        <div class="progress-grid">
            <article class="progress-card completion-card">
                <div class="card-heading">
                    <div>
                        <p class="card-kicker">OVERALL COMPLETION</p>
                        <h2>Your training progress</h2>
                    </div>
                    <span class="card-mark" aria-hidden="true">↗</span>
                </div>
                <div class="completion-content">
                    <div class="progress-ring" id="progressRing" runat="server" role="progressbar" aria-valuemin="0" aria-valuemax="100" aria-label="Internship completion progress">
                        <div class="ring-center">
                            <span class="completion-number"><asp:Literal ID="CompletionPercent" runat="server" /></span><span class="percent-sign">%</span>
                            <span class="ring-caption">completed</span>
                        </div>
                    </div>
                    <div class="completion-summary">
                        <p class="hours-total"><asp:Literal ID="ApprovedHours" runat="server" /> <span>of 240 hours</span></p>
                        <p class="summary-copy">Approved training hours count toward your internship completion.</p>
                        <div class="progress-track" role="progressbar" aria-label="Approved training hours" aria-valuemin="0" aria-valuemax="240">
                            <span id="hoursTrack" runat="server"></span>
                        </div>
                        <div class="track-labels"><span>0 hours</span><span>240 hours</span></div>
                    </div>
                </div>
                <div class="completion-footnote"><span class="gold-dot"></span> Your target: <strong>240 approved hours</strong> across two months</div>
            </article>

            <article class="progress-card target-card">
                <div class="card-heading">
                    <div>
                        <p class="card-kicker">YOUR TRAINING PLAN</p>
                        <h2>Stay on track</h2>
                    </div>
                    <span class="plan-icon" aria-hidden="true">◷</span>
                </div>
                <div class="target-stat">
                    <span class="target-number">30</span>
                    <span class="target-unit">hours <small>each week</small></span>
                </div>
                <div class="plan-divider"></div>
                <div class="plan-detail"><span class="detail-icon calendar-icon" aria-hidden="true">▦</span><span><strong>2 months</strong><small>Internship duration</small></span></div>
                <div class="plan-detail"><span class="detail-icon weeks-icon" aria-hidden="true">8</span><span><strong>8 weeks</strong><small>30 hours per week</small></span></div>
                <p class="plan-note">Keep a steady weekly rhythm to reach your 240-hour goal.</p>
            </article>
        </div>

        <article class="progress-card milestones-card">
            <div class="card-heading milestone-heading">
                <div>
                    <p class="card-kicker">HOUR MILESTONES</p>
                    <h2>Your eight-week journey</h2>
                    <p class="section-description">Each milestone represents 30 approved training hours.</p>
                </div>
                <div class="milestone-legend"><span class="legend-complete"></span> Reached <span class="legend-upcoming"></span> Ahead</div>
            </div>
            <div class="milestone-list" aria-label="Progress through eight 30-hour milestones">
                <asp:Repeater ID="MilestoneRepeater" runat="server">
                    <ItemTemplate>
                        <div class='milestone <%# Eval("CssClass") %>'>
                            <div class="milestone-top"><span class="milestone-check"><%# Eval("Marker") %></span><span class="milestone-week">STAGE <%# Eval("Week") %></span></div>
                            <div class="milestone-bar"><span style='width:<%# Eval("Fill") %>%'></span></div>
                            <div class="milestone-hours"><strong><%# Eval("HoursLabel") %></strong><span>/ 30 h</span></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="milestone-caption"><span class="caption-line"></span><span>Each 30-hour milestone is calculated from total approved hours, not a weekly hours breakdown</span></div>
        </article>

        <div class="progress-bottom-grid">
            <article class="progress-card next-step-card">
                <div class="next-step-icon" aria-hidden="true">✦</div>
                <div class="next-step-copy">
                    <p class="card-kicker">KEEP GOING</p>
                    <h2>Small steps add up</h2>
                    <p>Log your training consistently and check your tasks for feedback or requested revisions.</p>
                </div>
                <a class="tasks-link" href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>">View my tasks <span aria-hidden="true">→</span></a>
            </article>
            <article class="progress-card completion-note-card">
                <span class="certificate-icon" aria-hidden="true">▤</span>
                <div><p class="card-kicker">COMPLETION REPORT</p><h2>Your certificate is the final milestone</h2><p>Once your internship is complete and approved by the coordinator, your completion certificate will be available here.</p></div>
                <span class="locked-label"><span aria-hidden="true">⌑</span> After approval</span>
            </article>
        </div>
        <p class="privacy-note"><span aria-hidden="true">●</span> Final supervisor evaluations are private. This page shows your training progress only.</p>
    </section>
</asp:Content>
