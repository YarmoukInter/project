<%@ Page Title="Reports & Final Evaluation" Language="C#" MasterPageFile="~/CompanyPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="FinalEvaluation.aspx.cs" Inherits="Yarmouk.CompanyPages.FinalEvaluation" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="../Content/Shared/record-lists.css?v=organized-2" rel="stylesheet" />
<link href="<%= ResolveUrl("~/Content/CompanyPages/FinalEvaluation.css?v=criteria-2") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/CompanyPages/FinalEvaluation.js?v=modal-7") %>" defer></script>
<script src="../Scripts/Shared/record-lists.js?v=organized-2" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<main class="company-evaluation" id="companyEvaluation">
  <header class="evaluation-hero">
    <div class="evaluation-hero__copy">
      <p class="evaluation-eyebrow">YARMOUK UNIVERSITY · FACULTY OF INFORMATION TECHNOLOGY</p>
      <h1>Reports &amp; final evaluation</h1>
      <p>Review trainee reports, complete the company assessment, and send it to the academic supervisor.</p>
    </div>
    <img class="evaluation-hero__logo" src="<%= ResolveUrl("~/Content/Images/yarmouk-logo-on-dark.svg") %>" alt="Yarmouk University" />
    <div class="evaluation-hero__rings" aria-hidden="true"></div>
  </header>

  <asp:Panel ID="EvaluationMessage" runat="server" CssClass="evaluation-alert" Visible="false" role="status">
    <span class="evaluation-alert__icon" aria-hidden="true">i</span><asp:Literal ID="EvaluationMessageText" runat="server" />
  </asp:Panel>

  <div class="evaluation-layout">
    <aside class="trainee-panel" aria-label="Assigned trainees">
      <div class="panel-heading"><p class="evaluation-eyebrow">COMPANY RECORDS</p><h2>Company trainees</h2><span class="trainee-count"><asp:Literal ID="TraineeCount" runat="server" Text="0" /></span></div>
      <div class="evaluation-trainee-list">
        <asp:Repeater ID="TraineeList" runat="server">
          <ItemTemplate>
            <article class='evaluation-trainee-card <%#: Eval("CssClass") %>' title='<%#: Eval("ToolTip") %>' data-status='<%#: Eval("StatusLabel") %>'>
              <div class="evaluation-trainee-identity"><span class="trainee-avatar trainee-avatar--small uims-student-photo-avatar" data-student-photo-url='<%#: Eval("ProfileImageUrl") %>'><span><%#: Eval("Initials") %></span></span><div><strong class="evaluation-trainee-name"><%#: Eval("StudentName") %></strong><span class="evaluation-trainee-number"><%#: Eval("StudentNumber") %></span></div></div>
              <div class="evaluation-trainee-detail"><span>Major</span><strong><%#: Eval("MajorId") %></strong></div>
              <div class="evaluation-trainee-detail"><span>Placement</span><strong><%#: Eval("PlacementStatus") %></strong></div>
              <div class="evaluation-trainee-period"><div><span>Start</span><strong><%#: Eval("StartDate") %></strong></div><div><span>End</span><strong><%#: Eval("EndDate") %></strong></div></div>
              <div class="evaluation-trainee-review"><span class="status-pill"><%#: Eval("StatusLabel") %></span>
                <asp:HyperLink runat="server" CssClass="evaluation-button evaluation-button--primary"
                    data-evaluation-open="true" NavigateUrl='<%# Eval("SelectUrl") %>' Visible='<%# (bool)Eval("CanEvaluate") %>' Text="Evaluate" />
              </div>
            </article>
          </ItemTemplate>
        </asp:Repeater>
      </div>
      <asp:Panel ID="TraineesEmpty" runat="server" CssClass="empty-state" Visible="false">
        <span class="empty-state__icon" aria-hidden="true">✓</span><strong>No trainees to evaluate yet</strong><p>Accepted placements linked to your company will appear here.</p>
      </asp:Panel>
    </aside>

    <section class="evaluation-main" aria-label="Selected student evaluation">
      <asp:HiddenField ID="SelectedPlacementId" runat="server" ClientIDMode="Static" />
      <asp:HiddenField ID="ActiveEvaluationTab" runat="server" ClientIDMode="Static" Value="details" />
      <asp:Panel ID="EvaluationWorkspace" runat="server" CssClass="evaluation-workspace" Visible="false">
        <section class="student-summary" id="studentSummary">
          <div class="student-summary__identity"><span class="trainee-avatar trainee-avatar--large uims-student-photo-avatar" data-student-photo-url='<%= StudentProfileImageUrl %>'><span><asp:Literal ID="StudentInitials" runat="server" /></span></span><div><p class="evaluation-eyebrow">FIELD TRAINING EVALUATION</p><h2><asp:Literal ID="StudentName" runat="server" /></h2><p class="student-summary__meta"><asp:Literal ID="StudentNumber" runat="server" /> <span>·</span> <asp:Literal ID="PlacementLabel" runat="server" /></p></div></div>
          <span class="status-pill"><asp:Literal ID="EvaluationStatus" runat="server" /></span>
        </section>
        <nav class="evaluation-tabs" role="tablist" aria-label="Evaluation sections">
          <button type="button" class="evaluation-tab is-active" id="tab-details" role="tab" aria-controls="section-details" aria-selected="true" data-evaluation-tab="details">General information</button>
          <button type="button" class="evaluation-tab" id="tab-criteria" role="tab" aria-controls="section-criteria" aria-selected="false" data-evaluation-tab="criteria">Skills &amp; behavior</button>
          <button type="button" class="evaluation-tab" id="tab-review" role="tab" aria-controls="section-review" aria-selected="false" data-evaluation-tab="review">Global appraisal &amp; notes</button>
        </nav>

        <section class="evaluation-section" id="section-details" role="tabpanel" aria-labelledby="tab-details" data-evaluation-section="details" hidden>
          <div class="section-heading"><div><p class="evaluation-eyebrow">PLACEMENT DETAILS</p><h3>General information</h3></div></div>
          <dl class="placement-details">
            <div><dt>Student</dt><dd><asp:Literal ID="DetailsStudentName" runat="server" /></dd></div>
            <div><dt>Student number</dt><dd><asp:Literal ID="DetailsStudentNumber" runat="server" /></dd></div>
            <div><dt>Company</dt><dd><asp:Literal ID="DetailsCompany" runat="server" /></dd></div>
            <div><dt>Placement status</dt><dd><asp:Literal ID="DetailsStatus" runat="server" /></dd></div>
            <div><dt>Training start</dt><dd><asp:Literal ID="DetailsStart" runat="server" /></dd></div>
            <div><dt>Training end</dt><dd><asp:Literal ID="DetailsEnd" runat="server" /></dd></div>
          </dl>
        </section>

        <section class="evaluation-section" id="section-criteria" role="tabpanel" aria-labelledby="tab-criteria" data-evaluation-section="criteria" hidden>
          <div class="section-heading"><div><p class="evaluation-eyebrow">OFFICIAL COMPANY FORM</p><h3>Skills &amp; behavior</h3><p>Choose one performance level for each criterion. N/A is available only where the form allows it.</p></div></div>
          <div class="criteria-heading"><strong>CRITERION</strong><strong>CHOOSE ONE PERFORMANCE LEVEL</strong></div>
          <asp:Literal ID="CriteriaMarkup" runat="server" Mode="PassThrough" />
        </section>

        <section class="evaluation-section" id="section-review" role="tabpanel" aria-labelledby="tab-review" data-evaluation-section="review" hidden>
          <div class="section-heading"><div><p class="evaluation-eyebrow">FINAL COMPANY ASSESSMENT</p><h3>Global appraisal &amp; notes</h3><p>Enter the overall company score and observations for the academic supervisor.</p></div></div>
          <div class="global-review-card">
            <div class="evaluation-field"><label for="CompanyScore">Overall company score <span>out of 100</span></label><asp:TextBox ID="CompanyScore" runat="server" ClientIDMode="Static" CssClass="evaluation-control evaluation-control--score" TextMode="Number" min="0" max="100" step="1" placeholder="0–100" /><small>The company component contributes 40% to the weighted final result.</small></div>
            <div class="evaluation-field"><label for="CompanyFeedback">Overall observations</label><asp:TextBox ID="CompanyFeedback" runat="server" ClientIDMode="Static" CssClass="evaluation-control" TextMode="MultiLine" Rows="6" MaxLength="4000" placeholder="Summarize the trainee’s performance, strengths, and areas for improvement." /></div>
          </div>
        </section>

        <footer class="evaluation-actions">
          <p><asp:Literal ID="EvaluationGuidance" runat="server" /></p>
          <div class="evaluation-actions__buttons"><button type="button" class="evaluation-button evaluation-button--light" data-evaluation-print>Print / save as PDF</button><asp:Button ID="SubmitEvaluation" runat="server" Text="Send final evaluation" CssClass="evaluation-button evaluation-button--primary" OnClick="SubmitEvaluation_Click" CausesValidation="false" OnClientClick="return confirm('Send this final evaluation to the academic supervisor? You will not be able to edit it after submission.');" /></div>
        </footer>
      </asp:Panel>
      <asp:Panel ID="EvaluationEmpty" runat="server" CssClass="evaluation-placeholder" Visible="true"><span class="empty-state__icon" aria-hidden="true">✓</span><h2>Select a trainee</h2><p>Choose a trainee from the list above to review reports and complete the final company evaluation.</p></asp:Panel>
    </section>
  </div>
</main>
<dialog id="evaluationDialog" class="evaluation-dialog"><header class="evaluation-dialog-heading"><div><span>COMPANY ASSESSMENT</span><h2>Reports &amp; final evaluation</h2></div><button type="button" data-close-evaluation aria-label="Close evaluation">×</button></header><div id="evaluationDialogBody" class="evaluation-dialog-body"></div></dialog>
</asp:Content>


