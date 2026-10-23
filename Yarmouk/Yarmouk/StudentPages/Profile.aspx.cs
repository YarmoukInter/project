using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class Profile : AuthPage
    {
        private const int MaxCvBytes = 2 * 1024 * 1024;
        private const int MaxProfileImageBytes = 3 * 1024 * 1024;

        private readonly StudentService studentService = new StudentService();

        public sealed class CvDocument
        {
            public string FileName { get; set; }
            public string DataBase64 { get; set; }
            public long UploadedAtUnixMs { get; set; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.Form != null) Page.Form.Enctype = "multipart/form-data";
            if (string.Equals(Request.QueryString["downloadCv"], "1", StringComparison.Ordinal))
            {
                RegisterAsyncTask(new PageAsyncTask(DownloadCvAsync));
                return;
            }
            if (!IsPostBack)
            {
                if (Request.QueryString["cvUploaded"] == "1")
                {
                    litMessage.Text = "Your CV was uploaded and saved.";
                    pnlMessage.Visible = true;
                }
                if (Request.QueryString["profileImageUploaded"] == "1")
                {
                    litMessage.Text = "Your profile photo has been uploaded and saved.";
                    pnlMessage.Visible = true;
                }
                RegisterAsyncTask(new PageAsyncTask(LoadProfileAsync));
            }
        }

        protected void UploadProfileImage_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(UploadProfileImageAsync));
        }

        private async Task UploadProfileImageAsync()
        {
            var session = Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null) { Go("~/PublicPages/Login"); return; }
            if (!profileImageUpload.HasFile || profileImageUpload.PostedFile == null)
            { ShowError("Choose a JPG or PNG photo first."); return; }
            var file = profileImageUpload.PostedFile;
            var extension = Path.GetExtension(profileImageUpload.FileName);
            if (file.ContentLength <= 0 || file.ContentLength > MaxProfileImageBytes
                || (!string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase)))
            { ShowError("Choose a JPG or PNG photo up to 3 MB."); return; }

            string destination = null;
            bool databaseSaved = false;
            try
            {
                using (var source = Image.FromStream(file.InputStream, false, true))
                {
                    if ((source.RawFormat.Guid != ImageFormat.Jpeg.Guid && source.RawFormat.Guid != ImageFormat.Png.Guid)
                        || source.Width <= 0 || source.Height <= 0 || source.Width > 4096 || source.Height > 4096
                        || (long)source.Width * source.Height > 16000000L)
                    { ShowError("The selected file is not a supported photo or its dimensions are too large."); return; }

                    var folder = Server.MapPath("~/Uploads/StudentProfileImages");
                    Directory.CreateDirectory(folder);
                    var fileName = Guid.NewGuid().ToString("N") + ".png";
                    destination = Path.Combine(folder, fileName);
                    using (var normalized = new Bitmap(source.Width, source.Height))
                    using (var graphics = Graphics.FromImage(normalized))
                    {
                        graphics.DrawImage(source, 0, 0, source.Width, source.Height);
                        normalized.Save(destination, ImageFormat.Png);
                    }

                    var student = await studentService.GetStudentAsync(session.Uid, session.IdToken);
                    await studentService.UpdateProfileImageFileNameAsync(session.Uid, fileName, session.IdToken);
                    databaseSaved = true;
                    RemovePreviousProfileImage(student == null ? null : student.ProfileImageFileName, fileName, folder);
                }

                Response.Redirect(ResolveUrl("~/StudentPages/Profile.aspx?profileImageUploaded=1"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                if (!databaseSaved && !string.IsNullOrWhiteSpace(destination) && File.Exists(destination))
                { try { File.Delete(destination); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
                System.Diagnostics.Trace.TraceWarning("Could not upload student profile photo: " + ex.GetType().Name);
                ShowError("Could not save the profile photo. Please try again.");
            }
        }

        private static void RemovePreviousProfileImage(string oldFileName, string newFileName, string folder)
        {
            if (string.IsNullOrWhiteSpace(oldFileName) || string.Equals(oldFileName, newFileName, StringComparison.Ordinal)) return;
            var safeName = Path.GetFileName(oldFileName);
            if (!string.Equals(safeName, oldFileName, StringComparison.Ordinal)
                || !string.Equals(Path.GetExtension(safeName), ".png", StringComparison.OrdinalIgnoreCase)) return;
            var previousPath = Path.Combine(folder, safeName);
            try { if (File.Exists(previousPath)) File.Delete(previousPath); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        protected void UploadCv_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(UploadCvAsync));
        }

        private async Task UploadCvAsync()
        {
            var session = Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null) { Go("~/PublicPages/Login"); return; }
            if (!cvUpload.HasFile || cvUpload.PostedFile == null)
            { ShowError("Choose a PDF file to upload."); return; }
            var file = cvUpload.PostedFile;
            if (file.ContentLength <= 0 || file.ContentLength > MaxCvBytes
                || !string.Equals(Path.GetExtension(cvUpload.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            { ShowError("Choose a PDF file up to 2 MB."); return; }

            try
            {
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    file.InputStream.CopyTo(stream);
                    bytes = stream.ToArray();
                }
                if (bytes.Length == 0 || bytes.Length > MaxCvBytes || bytes.Length < 5
                    || bytes[0] != '%' || bytes[1] != 'P' || bytes[2] != 'D' || bytes[3] != 'F' || bytes[4] != '-')
                { ShowError("The selected file is not a valid PDF up to 2 MB."); return; }

                await new FirebaseHelper().SaveAsync(DatabasePaths.StudentCvFiles + "/" + session.Uid,
                    new CvDocument
                    {
                        FileName = Path.GetFileName(cvUpload.FileName),
                        DataBase64 = Convert.ToBase64String(bytes),
                        UploadedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    }, session.IdToken);
                Response.Redirect(ResolveUrl("~/StudentPages/Profile.aspx?cvUploaded=1"), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Could not upload student CV: " + ex.GetType().Name);
                ShowError("Could not save the CV to Firebase. Please try again.");
            }
        }

        private async Task DownloadCvAsync()
        {
            var session = Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null) { Go("~/PublicPages/Login"); return; }
            try
            {
                var document = await new FirebaseHelper().GetAsync<CvDocument>(
                    DatabasePaths.StudentCvFiles + "/" + session.Uid, session.IdToken);
                if (document == null || string.IsNullOrWhiteSpace(document.DataBase64))
                { Response.StatusCode = 404; Response.SuppressContent = true; return; }
                var bytes = Convert.FromBase64String(document.DataBase64);
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=\"student-cv.pdf\"");
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.SuppressContent = true;
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Could not download student CV: " + ex.GetType().Name);
                Response.StatusCode = 404;
                Response.SuppressContent = true;
            }
        }


        private async Task LoadProfileAsync()
        {

            AuthSession session = Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null)
            {
                Go("~/PublicPages/Login");
                return;
            }

            try
            {

                Task<User> userRequest = studentService.GetUserAsync(session.Uid, session.IdToken);
                Task<Student> studentRequest = studentService.GetStudentAsync(session.Uid, session.IdToken);
                await Task.WhenAll(userRequest, studentRequest);
                User user = await userRequest;
                Student student = await studentRequest;

                if (user == null || student == null)
                {
                    ShowError("Your student record is missing. Contact the internship coordinator.");
                    return;
                }

                // Use the academic record first, and fill missing identity/assignment
                // fields from the user's account record when available.
                if (string.IsNullOrWhiteSpace(student.StudentId))
                    student.StudentId = FirstValue(student.StudentNumber, user.StudentId);
                if (string.IsNullOrWhiteSpace(student.MajorId))
                    student.MajorId = user.MajorId;
                if (string.IsNullOrWhiteSpace(student.AcademicSupervisorId))
                    student.AcademicSupervisorId = user.AcademicSupervisorId;

                Major major = null;
                if (!string.IsNullOrWhiteSpace(student.MajorId))
                    major = await studentService.GetMajorAsync(student.MajorId.Trim(), session.IdToken);


                ShowProfile(user, student, major);
                try
                {
                    var cv = await new FirebaseHelper().GetAsync<CvDocument>(
                        DatabasePaths.StudentCvFiles + "/" + session.Uid, session.IdToken);
                    ShowSavedCv(cv);
                }
                catch (Exception ex)
                { System.Diagnostics.Trace.TraceWarning("Could not load student CV metadata: " + ex.GetType().Name); }
            }
            catch (Exception)
            {
                ShowError("Could not load the profile. Please try again.");
            }
        }


        protected void Save_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(SaveProfileAsync));
        }

        protected void SaveSkills_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(SaveSkillsAsync));
        }

        private async Task SaveSkillsAsync()
        {
            AuthSession session = Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null)
            {
                Go("~/PublicPages/Login");
                return;
            }

            List<string> softSkills;
            List<string> technicalSkills;
            if (!TryReadSkills(hidSoftSkills.Value, out softSkills) ||
                !TryReadSkills(hidTechnicalSkills.Value, out technicalSkills))
            {
                ShowError("Each skill list can have up to 20 skills, with 40 characters per skill.");
                return;
            }

            try
            {
                await studentService.UpdateSkillsAsync(session.Uid, softSkills, technicalSkills, session.IdToken);
                litMessage.Text = "Your skills have been saved to your student record.";
                pnlMessage.Visible = true;
                pnlError.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Could not save student skills: " + ex.GetType().Name);
                ShowError("Could not save your skills. Check Firebase student write rules and try again.");
            }
        }

        private async Task SaveProfileAsync()
        {
            AuthSession session = Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null)
            {
                Go("~/PublicPages/Login");
                return;
            }


            string fullName = txtFullName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string cvLink = txtCvLink.Text.Trim();
            string portfolioLink = txtPortfolioLink.Text.Trim();

            if (fullName.Length == 0 || fullName.Length > 120)
            {
                ShowError("Enter your name (up to 120 characters).");
                return;
            }
            if (phone.Length > 30)
            {
                ShowError("The phone number can be up to 30 characters.");
                return;
            }
            if (!IsWebLink(cvLink) || !IsWebLink(portfolioLink))
            {
                ShowError("CV and portfolio links must start with http:// or https://.");
                return;
            }


            try
            {

                User profile = new User();
                profile.FullName = fullName;
                profile.Phone = phone;
                profile.CvLink = cvLink;
                profile.PortfolioLink = portfolioLink;

                await studentService.UpdateProfileAsync(session.Uid, profile, session.IdToken);


                litFullName.Text = SafeText(fullName);
                lblInitials.Text = SafeText(GetInitials(fullName));
                litMessage.Text = "Your profile changes have been saved.";
                pnlMessage.Visible = true;
                pnlError.Visible = false;
            }
            catch (Exception)
            {
                ShowError("Could not save your profile. Check the Firebase rules and try again.");
            }
        }

        private void ShowProfile(User user, Student student, Major major)
        {
            string fullName = user.FullName ?? "";
            string majorName = student.MajorId;
            if (major != null && !string.IsNullOrWhiteSpace(major.Name))
                majorName = major.Name;

            litFullName.Text = SafeText(fullName);
            lblInitials.Text = SafeText(GetInitials(fullName));
            var profileImageFileName = student == null ? null : student.ProfileImageFileName;
            var safeProfileImageFileName = string.IsNullOrWhiteSpace(profileImageFileName) ? null : Path.GetFileName(profileImageFileName);
            var profileImagePath = safeProfileImageFileName == null ? null : Server.MapPath("~/Uploads/StudentProfileImages/" + safeProfileImageFileName);
            var hasProfileImage = !string.IsNullOrWhiteSpace(safeProfileImageFileName)
                && string.Equals(safeProfileImageFileName, profileImageFileName, StringComparison.Ordinal)
                && string.Equals(Path.GetExtension(safeProfileImageFileName), ".png", StringComparison.OrdinalIgnoreCase)
                && File.Exists(profileImagePath);
            imgProfileImage.Visible = true;
            profileInitialAvatar.Visible = !hasProfileImage;
            if (hasProfileImage) imgProfileImage.ImageUrl = ResolveUrl("~/Uploads/StudentProfileImages/" + safeProfileImageFileName);
            imgProfileImage.Style["display"] = hasProfileImage ? "" : "none";
            txtFullName.Text = fullName;
            txtPhone.Text = user.Phone ?? "";
            txtCvLink.Text = user.CvLink ?? "";
            txtPortfolioLink.Text = user.PortfolioLink ?? "";
            litEmail.Text = SafeText(user.Email);
            litAcademicEmail.Text = SafeText(user.Email);

            // تعبئة البيانات الأكاديمية
            litMajor.Text = SafeText(majorName);
            litAcademicMajor.Text = SafeText(majorName);

            string studentNumber = string.IsNullOrWhiteSpace(student.StudentNumber) ? student.StudentId : student.StudentNumber;
            litStudentNumber.Text = SafeText(studentNumber);
            litAcademicStudentId.Text = SafeText(studentNumber);

            int requiredHours = student.RequiredTrainingHours > 0 ? student.RequiredTrainingHours : student.RequiredHours;
            litTrainingHours.Text = requiredHours + " required";
            if (!string.IsNullOrWhiteSpace(student.Term))
                litTrainingHours.Text += " · " + SafeText(student.Term) + " term";

            string supervisor = student.AcademicSupervisorName;
            if (string.IsNullOrWhiteSpace(supervisor))
                supervisor = string.IsNullOrWhiteSpace(student.AcademicSupervisorId) ? "Not assigned" : "Assigned";
            litSupervisor.Text = SafeText(supervisor);
            litStatus.Text = SafeText(student.InternshipStatus ?? (user.IsActive ? "Active" : "Inactive"));


            litEnglishName.Text = SafeText(student.EnglishName);
            litArabicName.Text = SafeText(user.FullName);
            litNationalId.Text = SafeText(student.NationalId);
            litGender.Text = SafeText(student.Gender);
            litNationality.Text = SafeText(student.Nationality);
            litBirthPlace.Text = SafeText(student.BirthPlace);
            litBirthDate.Text = SafeText(student.BirthDate);
            litAcademicStatus.Text = SafeText(student.AcademicStatus);
            litGpa.Text = SafeText(student.Gpa);
            litCredits.Text = (student.CompletedCredits > 0 ? student.CompletedCredits : student.CompletedCreditHours).ToString();
            litGraduation.Text = SafeText(student.ExpectedGraduation);


            hidSoftSkills.Value = JsonConvert.SerializeObject(student.SoftSkills ?? new List<string>());
            hidTechnicalSkills.Value = JsonConvert.SerializeObject(student.TechnicalSkills ?? new List<string>());
        }

        private void ShowSavedCv(CvDocument cv)
        {
            if (cv == null || string.IsNullOrWhiteSpace(cv.DataBase64)) return;
            litCvStatus.Text = "Saved CV:";
            lnkSavedCv.Text = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(cv.FileName) ? "Download PDF" : cv.FileName);
            lnkSavedCv.NavigateUrl = ResolveUrl("~/StudentPages/Profile.aspx?downloadCv=1");
            lnkSavedCv.Visible = true;
        }

        // 4. دوال مساعدة (Validation, Links, Helpers)
        private static bool TryReadSkills(string json, out List<string> skills)
        {
            skills = new List<string>();
            try
            {
                List<string> submittedSkills = JsonConvert.DeserializeObject<List<string>>(json ?? "[]");
                if (submittedSkills == null || submittedSkills.Count > 20)
                    return false;

                foreach (string skill in submittedSkills)
                {
                    string cleanSkill = (skill ?? "").Trim();
                    if (cleanSkill.Length == 0 || cleanSkill.Length > 40)
                        return false;

                    bool isDuplicate = false;
                    foreach (string savedSkill in skills)
                    {
                        if (string.Equals(savedSkill, cleanSkill, StringComparison.OrdinalIgnoreCase))
                        {
                            isDuplicate = true;
                            break;
                        }
                    }
                    if (!isDuplicate)
                        skills.Add(cleanSkill);
                }
                return skills.Count <= 20;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static bool IsWebLink(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            Uri link;
            return Uri.TryCreate(value, UriKind.Absolute, out link) &&
                (link.Scheme == Uri.UriSchemeHttp || link.Scheme == Uri.UriSchemeHttps);
        }

        private static string GetInitials(string name)
        {
            string[] words = (name ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string initials = "";
            for (int i = 0; i < words.Length && i < 2; i++)
                initials += words[i].Substring(0, 1);
            return initials;
        }

        private static string SafeText(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "—";
            return HttpUtility.HtmlEncode(value);
        }

        private static string FirstValue(params string[] values)
        {
            foreach (string value in values)
                if (!string.IsNullOrWhiteSpace(value)) return value;
            return string.Empty;
        }

        private void ShowError(string message)
        {
            litError.Text = HttpUtility.HtmlEncode(message);
            pnlError.Visible = true;
            pnlMessage.Visible = false;
        }
    }
}
