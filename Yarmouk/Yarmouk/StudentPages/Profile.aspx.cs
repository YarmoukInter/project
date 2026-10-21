using System;
using System.Collections.Generic;
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

        private readonly StudentService studentService = new StudentService();


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(LoadProfileAsync));
            }
        }


        private async Task LoadProfileAsync()
        {

            AuthSession session = Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null)
            {
                Go("~/PublicPages/Login.aspx");
                return;
            }

            try
            {

                User user = await studentService.GetUserAsync(session.Uid, session.IdToken);
                Student student = await studentService.GetStudentAsync(session.Uid, session.IdToken);

                if (user == null || student == null)
                {
                    ShowError("Your student record is missing. Contact the internship coordinator.");
                    return;
                }

                Major major = await studentService.GetMajorAsync(student.MajorId, session.IdToken);


                ShowProfile(user, student, major);
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
        private async Task SaveProfileAsync()
        {
            AuthSession session = Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null)
            {
                Go("~/PublicPages/Login.aspx");
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

                User profile = new User();
                profile.FullName = fullName;
                profile.Phone = phone;
                profile.CvLink = cvLink;
                profile.PortfolioLink = portfolioLink;

                await studentService.UpdateProfileAsync(
                    session.Uid, profile, softSkills, technicalSkills, session.IdToken);


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

        private void ShowError(string message)
        {
            litError.Text = HttpUtility.HtmlEncode(message);
            pnlError.Visible = true;
            pnlMessage.Visible = false;
        }
    }
}