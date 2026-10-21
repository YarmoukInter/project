<%@ Page Title="Application Form" Language="C#" MasterPageFile="~/StudentPages/Section.Master" AutoEventWireup="true" Async="true" CodeBehind="Apply.aspx.cs" Inherits="Yarmouk.StudentPages.Apply" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Content/StudentPages/Apply.css") %>" rel="stylesheet" />
    <script src="<%= ResolveUrl("~/Scripts/StudentPages/Apply.js") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
    <div class="apply-backdrop" aria-hidden="true"></div>
    <section class="apply-dialog" aria-labelledby="applyTitle">
        <a class="apply-close" href="<%= ResolveUrl("~/StudentPages/Opportunities.aspx") %>" aria-label="Close application">×</a>
        <div class="apply-brand"><img src="<%= ResolveUrl("~/Content/Images/yarmouk-college-logo.png") %>" alt="Yarmouk University logo" /><span>INTERNSHIP APPLICATION</span></div>
        <h1 id="applyTitle">Apply for <asp:Literal ID="OpportunityTitle" runat="server" /></h1>
        <div class="apply-company" id="applyCompany"></div>
        <p class="apply-intro">Review your profile details and answer the company's questions before submitting.</p>
        <asp:Literal ID="ApplyMessage" runat="server" />
        <asp:HiddenField ID="AnswersJson" runat="server" />
        <section class="profile-card" aria-labelledby="profileTitle">
            <div class="section-heading"><div><span class="section-step">01</span><h2 id="profileTitle">Your information</h2></div><span class="read-only-note">Loaded from your student account</span></div>
            <div class="profile-grid">
                <div class="profile-field"><span>Full name</span><strong id="studentName">Loading…</strong></div>
                <div class="profile-field"><span>Email address</span><strong id="studentEmail">Loading…</strong></div>
                <div class="profile-field cv-field"><span>CV / Resume</span><strong id="studentCv">Loading…</strong><a id="cvLink" href="#" target="_blank" rel="noopener" hidden>View CV ↗</a></div>
            </div>
        </section>

        <section class="questions-card" aria-labelledby="questionsTitle">
            <div class="section-heading"><div><span class="section-step">02</span><h2 id="questionsTitle">Company questions</h2></div><span class="required-note" id="requiredNote">* Required</span></div>
            <div id="companyQuestions" class="company-questions"></div>
            <p class="no-questions" id="noQuestions" hidden>The company has not added any questions for this opportunity.</p>
        </section>

        <div class="apply-actions">
            <a class="cancel-button" href="<%= ResolveUrl("~/StudentPages/Opportunities.aspx") %>">Cancel</a>
            <asp:Button ID="SubmitApplication" runat="server" Text="Submit application" CssClass="submit-application" OnClick="SubmitApplication_Click" OnClientClick="return prepareApplicationSubmission();" />
        </div>
        <script id="application-data" type="application/json"><asp:Literal ID="ApplicationDataJson" runat="server" /></script>
    </section>
</asp:Content>
