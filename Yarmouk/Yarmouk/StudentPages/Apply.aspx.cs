using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;
namespace Yarmouk.StudentPages
{
    public partial class Apply : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null) { GoToLogin(); return; }
                string id = Request.QueryString["id"];
                try
                {
                    var opportunity = await new OpportunityService().GetByIdAsync(id, auth.IdToken);
                    if (opportunity == null) { SetMessage("This opportunity is no longer available.", true); return; }
                    var user = auth.User ?? await new StudentService().GetUserAsync(auth.Uid, auth.IdToken);
                    var student = await new StudentService().GetStudentAsync(auth.Uid, auth.IdToken);
                    if (user == null || student == null) { SetMessage("Your student profile could not be loaded. Please contact the internship coordinator.", true); return; }
                    if (!OpportunityService.IsAvailableForStudent(opportunity, student))
                    { SetMessage("This opportunity is closed, full, or not available for your major.", true); return; }
                    var company = string.IsNullOrWhiteSpace(opportunity.CompanyId) ? null :
                        await new FirebaseHelper().GetAsync<Company>(DatabasePaths.Companies + "/" + opportunity.CompanyId, auth.IdToken);
                    OpportunityTitle.Text = HttpUtility.HtmlEncode(opportunity.Title ?? "Internship");
                    var applications = await new ApplicationService().GetForStudentAsync(auth.Uid, auth.IdToken);
                    if (applications.Any(a => a.OpportunityId == opportunity.Id))
                    { SetMessage("You have already applied for this opportunity. You can track it in My Applications.", true); return; }
                    var questions = (opportunity.ApplicationQuestions ?? new List<string>()).Where(q => !string.IsNullOrWhiteSpace(q)).Select(q => q.Trim()).ToList();
                    ApplicationDataJson.Text = SafeJson(new
                    {
                        opportunity = new
                        {
                            id = opportunity.Id,
                            title = opportunity.Title,
                            companyName = company == null ? "Company" : company.Name,
                            companyLogoUrl = company == null ? "" : company.LogoUrl,
                            questions = questions
                        },
                        student = new
                        {
                            fullName = user.FullName,
                            email = auth.Email ?? user.Email,
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Could not load application form: " + ex.GetType().Name);
                    SetMessage("The application form could not be loaded. Please try again.", true);
                }
            }));
        }

        protected void SubmitApplication_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null) { GoToLogin(); return; }
                Opportunity opportunity;
                Student student;
                User user;
                try
                {
                    opportunity = await new OpportunityService().GetByIdAsync(Request.QueryString["id"], auth.IdToken);
                    if (opportunity == null) { SetMessage("This opportunity is no longer available.", true); return; }
                    student = await new StudentService().GetStudentAsync(auth.Uid, auth.IdToken);
                    user = await new StudentService().GetUserAsync(auth.Uid, auth.IdToken);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Could not verify application request: " + ex.GetType().Name);
                    SetMessage("We could not verify this opportunity right now. Please try again.", true);
                    return;
                }
                if (student == null || user == null) { SetMessage("Your student profile could not be verified. Please sign in again.", true); return; }
                var questions = (opportunity.ApplicationQuestions ?? new List<string>()).Where(q => !string.IsNullOrWhiteSpace(q)).Select(q => q.Trim()).ToList();
                Dictionary<string, string> answers;
                try { answers = JsonConvert.DeserializeObject<Dictionary<string, string>>(AnswersJson.Value ?? "") ?? new Dictionary<string, string>(); }
                catch { SetMessage("Please review your answers and try again.", true); return; }
                var validatedAnswers = new Dictionary<string, string>();
                for (int i = 0; i < questions.Count; i++)
                {
                    string answer;
                    if (!answers.TryGetValue(i.ToString(), out answer) || string.IsNullOrWhiteSpace(answer))
                    { SetMessage("Please answer all company questions before submitting.", true); return; }
                    answer = answer.Trim();
                    if (answer.Length > 3000) { SetMessage("Each answer must be 3,000 characters or fewer.", true); return; }
                    validatedAnswers[i.ToString()] = answer;
                }
                try
                {
                    var service = new ApplicationService();
                    var existing = await service.GetForStudentAsync(auth.Uid, auth.IdToken);
                    if (existing.Any(a => a.OpportunityId == opportunity.Id))
                    { SetMessage("You have already applied for this opportunity.", true); return; }
                    if (!OpportunityService.IsAvailableForStudent(opportunity, student))
                    { SetMessage("This opportunity is closed, full, or not available for your major.", true); return; }
                    await service.SaveForStudentAsync(new Application
                    {
                        StudentUid = auth.Uid,
                        OpportunityId = opportunity.Id,
                        Status = "pending",
                        Answers = validatedAnswers
                    }, auth.Uid, auth.IdToken);
                    SetMessage("Your application was submitted. <a href=\"" + ResolveUrl("~/StudentPages/MyApplications.aspx") + "\">View My Applications</a>", false);
                    SubmitApplication.Enabled = false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Application submission failed: " + ex.GetType().Name);
                    SetMessage("We could not submit your application. Please try again or contact the coordinator.", true);
                }
            }));
        }

        private void SetMessage(string message, bool error)
        {
            ApplyMessage.Text = "<p class=\"apply-message " + (error ? "error" : "success") + "\" role=\"status\">" +
                (message.StartsWith("Your application", StringComparison.Ordinal) ? message : HttpUtility.HtmlEncode(message)) + "</p>";
        }
        private static string SafeJson(object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })
                .Replace("<", "\\u003c").Replace(">", "\\u003e").Replace("&", "\\u0026");
        }
        private void GoToLogin() { Response.Redirect(ResolveUrl("~/PublicPages/Login.aspx"), false); Context.ApplicationInstance.CompleteRequest(); }
    }
}
