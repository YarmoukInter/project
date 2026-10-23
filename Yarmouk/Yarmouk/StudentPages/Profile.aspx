<%@ Page Title="Student profile" Language="C#" MasterPageFile="~/StudentPages/Section.Master" Async="true" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="Yarmouk.StudentPages.Profile" %>
<asp:Content ID="HeadSection" ContentPlaceHolderID="HeadContent" runat="server">
<link href="../Content/Shared/workspace-polish.css?v=1" rel="stylesheet" />
<link href="<%= ResolveUrl("~/Content/StudentPages/Profile.css?v=profile-upload-2") %>" rel="stylesheet" />
<script src="<%= ResolveUrl("~/Scripts/StudentPages/Profile.js?v=profile-upload-2") %>" defer></script>
</asp:Content>
<asp:Content ID="BodySection" ContentPlaceHolderID="MainContent" runat="server">
<section class="profile-cover" aria-labelledby="profile-title">
  <div class="profile-cover__banner"><span>FACULTY OF INFORMATION TECHNOLOGY AND COMPUTER SCIENCE</span><img src="<%= ResolveUrl("~/Content/Images/yarmouk-college-logo.png") %>" alt="Yarmouk University" /></div>
  <div class="profile-cover__identity">
    <div class="profile-avatar-wrap">
      <asp:Image ID="imgProfileImage" runat="server" CssClass="profile-avatar-image" AlternateText="Profile photo" />
      <asp:Panel ID="profileInitialAvatar" runat="server" CssClass="profile-avatar" aria-hidden="true"><asp:Label ID="lblInitials" runat="server" /></asp:Panel>
      <asp:FileUpload ID="profileImageUpload" runat="server" ClientIDMode="Static" CssClass="profile-image-input" accept="image/jpeg,image/png,.jpg,.jpeg,.png" aria-label="Choose a profile photo" />
      <label for="profileImageUpload" class="profile-avatar-camera" title="Choose a profile photo" aria-label="Choose a profile photo"><svg viewBox="0 0 24 24" aria-hidden="true" focusable="false"><path d="M4 7.5h3l1.5-2h7l1.5 2h3A1.5 1.5 0 0 1 21.5 9v9a1.5 1.5 0 0 1-1.5 1.5H4A1.5 1.5 0 0 1 2.5 18V9A1.5 1.5 0 0 1 4 7.5Z"/><circle cx="12" cy="13" r="3.5"/></svg></label>
    </div>
    <div class="profile-cover__identity-copy"><p class="eyebrow">Student profile</p><h1 id="profile-title"><asp:Literal ID="litFullName" runat="server" /></h1><p class="profile-subtitle"><asp:Literal ID="litMajor" runat="server" /> <span aria-hidden="true">·</span> <asp:Literal ID="litStudentNumber" runat="server" /></p><div class="profile-photo-actions"><asp:Button ID="btnUploadProfileImage" runat="server" Text="Save photo" CssClass="profile-photo-save" OnClick="UploadProfileImage_Click" CausesValidation="false" /><span id="profileImageFileName" aria-live="polite">Choose a JPG or PNG image (up to 3 MB).</span></div></div>
  </div>
  <dl class="profile-stats">
    <div><dt>Training hours</dt><dd><asp:Literal ID="litTrainingHours" runat="server" /></dd></div>
    <div><dt>Academic supervisor</dt><dd><asp:Literal ID="litSupervisor" runat="server" /></dd></div>
    <div><dt>Account status</dt><dd><asp:Literal ID="litStatus" runat="server" /></dd></div>
  </dl>
</section>

<asp:Panel ID="pnlMessage" runat="server" CssClass="profile-message" role="status" Visible="false"><asp:Literal ID="litMessage" runat="server" /></asp:Panel>
<asp:Panel ID="pnlError" runat="server" CssClass="profile-message profile-message--error" role="alert" Visible="false"><asp:Literal ID="litError" runat="server" /></asp:Panel>

<div class="profile-tabs" role="tablist" aria-label="Profile sections">
  <button type="button" role="tab" id="tab-personal" aria-controls="panel-personal" aria-selected="true">Personal information</button>
  <button type="button" role="tab" id="tab-academic" aria-controls="panel-academic" aria-selected="false" tabindex="-1">Academic record</button>
  <button type="button" role="tab" id="tab-skills" aria-controls="panel-skills" aria-selected="false" tabindex="-1">Skills</button>
</div>
<asp:HiddenField ID="hidActiveProfileTab" runat="server" ClientIDMode="Static" Value="personal" />

<div class="profile-panel" id="panel-personal" role="tabpanel" aria-labelledby="tab-personal">
  <section class="profile-card">
    <div class="profile-card__heading"><div><p class="eyebrow">Keep your details up to date</p><h2>Personal information</h2></div><span class="profile-lock"><i class="bi bi-shield-check" aria-hidden="true"></i> Only your profile details</span></div>
    <div class="profile-fields">
      <div class="profile-field"><label for="txtFullName">Full name</label><asp:TextBox ID="txtFullName" runat="server" ClientIDMode="Static" MaxLength="120" CssClass="form-control" /></div>
      <div class="profile-field"><label for="txtPhone">Phone number</label><asp:TextBox ID="txtPhone" runat="server" ClientIDMode="Static" MaxLength="30" TextMode="Phone" CssClass="form-control" /></div>
      <div class="profile-field"><label for="txtCvLink">External CV link <span>(optional)</span></label><asp:TextBox ID="txtCvLink" runat="server" ClientIDMode="Static" MaxLength="500" TextMode="Url" CssClass="form-control" /></div>
      <div class="profile-field"><label for="txtPortfolioLink">Portfolio link <span>(optional)</span></label><asp:TextBox ID="txtPortfolioLink" runat="server" ClientIDMode="Static" MaxLength="500" TextMode="Url" CssClass="form-control" /></div>
      <div class="profile-field profile-field--readonly"><label>Email address</label><asp:Literal ID="litEmail" runat="server" /></div>
    </div>
    <section class="profile-cv-upload" aria-labelledby="cv-upload-title">
      <div class="profile-cv-upload__heading"><span class="profile-cv-upload__icon" aria-hidden="true"><svg viewBox="0 0 24 24" focusable="false"><path d="M7 3.75h7l4 4v12.5H7a2 2 0 0 1-2-2v-12.5a2 2 0 0 1 2-2Z"/><path d="M14 3.75v4h4M8.5 14h7M8.5 17h5"/></svg></span><div><p class="eyebrow">Your document</p><h3 id="cv-upload-title">Upload your CV</h3><p>PDF only, up to 2 MB. A new upload replaces your previous file.</p></div></div>
      <div class="profile-cv-upload__dropzone" id="cvDropzone">
        <asp:FileUpload ID="cvUpload" runat="server" ClientIDMode="Static" CssClass="profile-cv-upload__input" accept=".pdf,application/pdf" aria-label="Choose a CV PDF" />
        <span class="profile-cv-upload__file-icon" aria-hidden="true">PDF</span>
        <div class="profile-cv-upload__prompt"><strong id="cvFileName" aria-live="polite">Choose a PDF to upload</strong><span>or drag and drop it here</span></div>
        <label class="profile-cv-upload__browse" for="cvUpload">Browse files</label>
      </div>
      <div class="profile-cv-upload__footer"><span>Maximum file size: 2 MB</span><asp:Button ID="btnUploadCv" runat="server" Text="Upload CV" CssClass="btn btn-success" OnClick="UploadCv_Click" CausesValidation="false" /></div>
      <p class="profile-cv-upload__saved"><asp:Literal ID="litCvStatus" runat="server" Text="No CV uploaded yet." /> <asp:HyperLink ID="lnkSavedCv" runat="server" Visible="false" /></p>
      <p class="profile-cv-upload__error" id="cvUploadError" role="alert" hidden></p>
    </section>
    <div class="profile-actions profile-personal-actions"><asp:Button ID="btnSave" runat="server" Text="Save changes" CssClass="btn btn-success" OnClick="Save_Click" /><span>University academic records stay read only.</span></div>
  </section>
</div>

<div class="profile-panel" id="panel-academic" role="tabpanel" aria-labelledby="tab-academic" hidden>
  <section class="profile-card">
    <div class="profile-card__heading"><div><p class="eyebrow">University record</p><h2>Academic information</h2></div><span class="profile-lock"><i class="bi bi-lock" aria-hidden="true"></i> Read only</span></div>
    <dl class="academic-details">
      <div><dt>Name (Arabic)</dt><dd><asp:Literal ID="litArabicName" runat="server" /></dd></div>
      <div><dt>Name (English)</dt><dd><asp:Literal ID="litEnglishName" runat="server" /></dd></div>
      <div><dt>National ID</dt><dd><asp:Literal ID="litNationalId" runat="server" /></dd></div>
      <div><dt>Gender</dt><dd><asp:Literal ID="litGender" runat="server" /></dd></div>
      <div><dt>Nationality</dt><dd><asp:Literal ID="litNationality" runat="server" /></dd></div>
      <div><dt>Place of birth</dt><dd><asp:Literal ID="litBirthPlace" runat="server" /></dd></div>
      <div><dt>Date of birth</dt><dd><asp:Literal ID="litBirthDate" runat="server" /></dd></div>
      <div><dt>Student number</dt><dd><asp:Literal ID="litAcademicStudentId" runat="server" /></dd></div>
      <div><dt>Email address</dt><dd><asp:Literal ID="litAcademicEmail" runat="server" /></dd></div>
      <div><dt>College</dt><dd>Faculty of Information Technology and Computer Science</dd></div>
      <div><dt>Major</dt><dd><asp:Literal ID="litAcademicMajor" runat="server" /></dd></div>
      <div><dt>Academic status</dt><dd><asp:Literal ID="litAcademicStatus" runat="server" /></dd></div>
      <div><dt>Cumulative GPA</dt><dd><asp:Literal ID="litGpa" runat="server" /></dd></div>
      <div><dt>Completed credits</dt><dd><asp:Literal ID="litCredits" runat="server" /></dd></div>
      <div><dt>Expected graduation</dt><dd><asp:Literal ID="litGraduation" runat="server" /></dd></div>
    </dl>
  </section>
</div>

<div class="profile-panel" id="panel-skills" role="tabpanel" aria-labelledby="tab-skills" hidden>
  <section class="profile-card">
    <div class="profile-card__heading"><div><p class="eyebrow">Your strengths</p><h2>Skills</h2></div><span class="profile-lock"><i class="bi bi-cloud-check" aria-hidden="true"></i> Saved to your student record</span></div>
    <div class="skills-grid">
      <div class="profile-field skill-group" data-skill-group="soft"><label for="txtSoftSkill">Soft skills</label><div class="skill-list" data-skill-list></div><div class="skill-entry"><input id="txtSoftSkill" class="form-control" type="text" maxlength="40" placeholder="Add a soft skill" data-skill-input /><button type="button" class="btn btn-outline-success" data-skill-add>Add</button></div><p class="field-error" data-skill-error role="alert"></p><asp:HiddenField ID="hidSoftSkills" runat="server" ClientIDMode="Static" /></div>
      <div class="profile-field skill-group" data-skill-group="technical"><label for="txtTechnicalSkill">Technical skills</label><div class="skill-list" data-skill-list></div><div class="skill-entry"><input id="txtTechnicalSkill" class="form-control" type="text" maxlength="40" placeholder="Add a technical skill" data-skill-input /><button type="button" class="btn btn-outline-success" data-skill-add>Add</button></div><p class="field-error" data-skill-error role="alert"></p><asp:HiddenField ID="hidTechnicalSkills" runat="server" ClientIDMode="Static" /></div>
    </div>
    <p class="profile-note">Add up to 20 skills in each list. Save your skills to update your student record.</p>
    <div class="profile-actions profile-skills-actions"><asp:Button ID="btnSaveSkills" runat="server" ClientIDMode="Static" Text="Save skills" CssClass="btn btn-success" OnClick="SaveSkills_Click" CausesValidation="false" formnovalidate="formnovalidate" /><span>Skills are saved to your student record.</span></div>
  </section>
</div>

</asp:Content>
