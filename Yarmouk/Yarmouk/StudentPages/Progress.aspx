<%@ Page Title="My Progress" Language="C#" MasterPageFile="~/StudentPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="Progress.aspx.cs" Inherits="Yarmouk.StudentPages.Progress" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous" />
    <link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/StudentPages/Progress.css?v=progress-ux-2") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/StudentPages/Progress.js") %>" defer></script>
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<script src="../Scripts/Shared/record-lists.js?v=organized-2" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <section class="progress-page" aria-labelledby="progress-title">
        <header class="progress-heading">
            <div>
                <p class="eyebrow">STUDENT TRAINING</p>
                <h1 id="progress-title">My Progress</h1>
                <p class="heading-copy">Complete 10 tasks to reach 100% internship progress.</p>
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
                        <p class="hours-total"><asp:Literal ID="CompletedTaskCount" runat="server" /> <span>of <asp:Literal ID="RequiredHours" runat="server" /> tasks completed</span></p>
                        <p class="summary-copy">Each completed task adds 10% toward your internship progress.</p>
                        <div id="hoursProgressBar" runat="server" class="progress-track" role="progressbar" aria-label="Completed tasks" aria-valuemin="0" aria-valuemax="0">
                            <span id="hoursTrack" runat="server"></span>
                        </div>
                        <div class="track-labels"><span>0 tasks</span><span><asp:Literal ID="TrackTarget" runat="server" /> tasks</span></div>
                    </div>
                </div>
                <div class="completion-footnote"><span class="gold-dot"></span> Required completion target: <strong><asp:Literal ID="TargetHours" runat="server" /> tasks</strong></div>
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
                    <span class="target-number"><asp:Literal ID="PlanRequiredHours" runat="server" /></span>
                    <span class="target-unit">hours <small>required total</small></span>
                </div>
                <div class="plan-divider"></div>
                <div class="plan-detail"><span class="detail-icon calendar-icon" aria-hidden="true">✓</span><span><strong><asp:Literal ID="PlanApprovedHours" runat="server" /></strong><small>Approved hours so far</small></span></div>
                <div class="plan-detail"><span class="detail-icon weeks-icon" aria-hidden="true">10</span><span><strong>10 tasks</strong><small>Each completed task adds 10%</small></span></div>
                <p class="plan-note">Only tasks marked Completed count toward your progress.</p>
            </article>
        </div>

        <article class="progress-card milestones-card">
            <div class="card-heading milestone-heading">
                <div>
                    <p class="card-kicker">TASK MILESTONES</p>
                    <h2>Your training milestones</h2>
                    <p class="section-description">Complete each of the 10 required tasks to reach the next milestone.</p>
                </div>
                <div class="milestone-legend"><span class="legend-complete"></span> Reached <span class="legend-upcoming"></span> Ahead</div>
            </div>
            <div class="milestone-list" aria-label="Progress through training milestones">
                <asp:Repeater ID="MilestoneRepeater" runat="server">
                    <ItemTemplate>
                        <div class='milestone <%# Eval("CssClass") %>'>
                            <div class="milestone-top"><span class="milestone-check"><%# Eval("Marker") %></span><span class="milestone-week">TASK <%# Eval("Week") %></span></div>
                            <div class="milestone-bar"><span style='width:<%# Eval("Fill") %>%'></span></div>
                            <div class="milestone-hours"><strong><%# Eval("HoursLabel") %></strong><span>/ <%# Eval("TargetHours") %> completed</span></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="milestone-caption"><span class="caption-line"></span><span>Only distinct tasks marked Completed count toward milestones.</span></div>
        </article>

        <article class="progress-card milestones-card academic-notes-card">
            <div class="card-heading"><div><p class="card-kicker">ACADEMIC GUIDANCE</p><h2>Notes from your academic supervisor</h2><p class="section-description">Feedback about your training progress and next steps.</p></div></div>
            <asp:Repeater ID="ProgressNotes" runat="server">
                <HeaderTemplate><div id="academic-notes-list"></HeaderTemplate>
                <ItemTemplate>
                    <div class="academic-note"><div class="academic-note-header"><strong><%#: Eval("Author") %></strong><time datetime='<%# Eval("DateIso") %>'><%#: Eval("Date") %></time></div><p><%#: Eval("Body") %></p></div>
                </ItemTemplate>
                <FooterTemplate></div></FooterTemplate>
            </asp:Repeater>
            <asp:Panel ID="NoProgressNotes" runat="server" Visible="false"><p class="plan-note">Your academic supervisor has not added a progress note yet.</p></asp:Panel>
        </article>

        <div class="progress-bottom-grid">
            <asp:Panel ID="NextStepCard" runat="server" CssClass="progress-card next-step-card">
                <div class="next-step-icon" aria-hidden="true">✦</div>
                <div class="next-step-copy">
                    <p class="card-kicker">KEEP GOING</p>
                    <h2>Small steps add up</h2>
                    <p>Check task feedback and requested revisions while you work toward completing your 10 required tasks.</p>
                </div>
                <a class="tasks-link" href="<%= ResolveUrl("~/StudentPages/MyTasks.aspx") %>">View my tasks <span aria-hidden="true">→</span></a>
            </asp:Panel>
            <asp:Panel ID="AcademicMeetingCard" runat="server" CssClass="progress-card academic-meeting-card" Visible="false">
                <div class="meeting-card-icon" aria-hidden="true">◷</div>
                <div class="meeting-card-copy">
                    <p class="card-kicker">ACADEMIC REVIEW MEETING</p>
                    <h2>Meet with your academic supervisor</h2>
                    <p class="meeting-card-status"><asp:Literal ID="AcademicMeetingStatus" runat="server" /></p>
                    <dl class="meeting-details">
                        <div id="AcademicMeetingDateRow" runat="server"><dt>Date</dt><dd><asp:Literal ID="AcademicMeetingDate" runat="server" /></dd></div>
                        <div id="AcademicMeetingNoteRow" runat="server"><dt>Note</dt><dd><asp:Literal ID="AcademicMeetingNote" runat="server" /></dd></div>
                    </dl>
                </div>
                <asp:HyperLink ID="AcademicMeetingLink" runat="server" CssClass="meeting-link" Target="_blank" rel="noopener noreferrer" Visible="false">Join Zoom meeting <span aria-hidden="true">↗</span></asp:HyperLink>
            </asp:Panel>
            <article class="progress-card completion-note-card">
                <span class="certificate-icon" aria-hidden="true">▤</span>
                <div><p class="card-kicker">COMPLETION REPORT</p><h2>Your certificate is the final milestone</h2><p>Once your internship is complete and approved by the coordinator, your completion certificate will be available here.</p></div>
                <asp:HyperLink ID="CompletionCertificateLink" runat="server" CssClass="completion-certificate-link" Target="_blank" rel="noopener noreferrer" Visible="false">Download certificate <span aria-hidden="true">↗</span></asp:HyperLink>
                <asp:Panel ID="CertificatePendingLabel" runat="server" CssClass="locked-label"><span aria-hidden="true">⌑</span> After approval</asp:Panel>
            </article>
        </div>
        <p class="privacy-note"><span aria-hidden="true">●</span> Final supervisor evaluations are private. This page shows your training progress only.</p>
    </section>
</asp:Content>
