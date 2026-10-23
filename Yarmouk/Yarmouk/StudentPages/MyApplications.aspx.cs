using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;
namespace Yarmouk.StudentPages
{
    public partial class MyApplications : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["offerAccepted"] == "1") SetApplicationsMessage("Your internship offer was confirmed.", false);
            if (Request.QueryString["submitted"] == "1") SetApplicationsMessage("Application submitted successfully.", false);
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null) { GoToLogin(); return; }
                try
                {
                    var applications = await new ApplicationService().GetForStudentAsync(auth.Uid, auth.IdToken);
                    var opportunities = await new OpportunityService().GetAllAsync(auth.IdToken);
                    var student = await new StudentService().GetStudentAsync(auth.Uid, auth.IdToken);
                    var opportunityLookup = opportunities.ToDictionary(o => o.Id ?? "", o => o);
                    var companyLookup = await new FirebaseHelper().GetAsync<Dictionary<string, Company>>(DatabasePaths.Companies, auth.IdToken)
                        ?? new Dictionary<string, Company>();
                    var items = applications.Select(a => {
                        Opportunity opportunity;
                        opportunityLookup.TryGetValue(a.OpportunityId ?? "", out opportunity);
                        Company company = null;
                        if (opportunity != null) companyLookup.TryGetValue(opportunity.CompanyId ?? "", out company);
                        return new { id = a.Id, opportunityId = a.OpportunityId,
                            title = opportunity == null ? "Internship opportunity" : opportunity.Title,
                            companyName = company == null ? "Company" : company.Name,
                            companyLogoUrl = company == null ? "" : company.LogoUrl,
                            companyIndustry = company == null ? "" : company.Industry,
                            companyLocation = company == null ? "" : company.Location,
                            companyContactEmail = company == null ? "" : company.ContactEmail,
                            description = opportunity == null ? "" : opportunity.Description,
                            requirements = opportunity == null ? "" : opportunity.Requirements,
                            duration = opportunity == null ? "" : opportunity.Duration,
                            workMode = opportunity == null ? "" : opportunity.WorkMode,
                            requiredTrainingHours = opportunity == null ? 0 : opportunity.RequiredTrainingHours,
                            deadline = opportunity == null ? "" : opportunity.Deadline,
                            skills = opportunity == null ? new List<string>() : opportunity.Skills ?? new List<string>(),
                            questions = a.ApplicationQuestions != null && a.ApplicationQuestions.Count > 0
                                ? a.ApplicationQuestions : opportunity == null ? new List<string>() : opportunity.ApplicationQuestions ?? new List<string>(),
                            answers = a.Answers ?? new List<string>(),
                            status = student != null && student.AcceptedApplicationId == a.Id ? "accepted" : (a.Status ?? "pending"),
                            appliedAtUnixMs = a.AppliedAtUnixMs,
                            rejectionReason = a.RejectionReason ?? "" };
                    }).ToList();
                    ApplicationsData.Text = SafeJson(items);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Could not load student applications: " + ex.GetType().Name);
                    ApplicationsData.Text = "[]";
                }
            }));
        }

        protected void AcceptApplication_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null) { GoToLogin(); return; }
                string applicationId = AcceptApplicationId.Value;
                try
                {
                    var ownedApplications = await new ApplicationService().GetForStudentAsync(auth.Uid, auth.IdToken);
                    var application = ownedApplications.FirstOrDefault(a => a.Id == applicationId);
                    string currentStatus = (application == null ? "" : application.Status ?? "").Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
                    if (application == null || (currentStatus != "offered" && currentStatus != "offerreceived"))
                    { SetApplicationsMessage("This offer is no longer available.", true); return; }
                    var applicationService = new ApplicationService();
                    if (await applicationService.HasConfirmedInternshipAsync(auth.Uid, auth.IdToken))
                    { SetApplicationsMessage("You have already confirmed an internship offer.", true); return; }
                    var students = new StudentService();
                    var student = await students.GetStudentAsync(auth.Uid, auth.IdToken);
                    if (student == null) { SetApplicationsMessage("Your student profile could not be loaded.", true); return; }
                    if (!string.IsNullOrWhiteSpace(student.AcceptedApplicationId) && student.AcceptedApplicationId != application.Id)
                    { SetApplicationsMessage("You have already confirmed another internship offer.", true); return; }
                    var opportunity = await new OpportunityService().GetByIdAsync(application.OpportunityId, auth.IdToken);
                    if (opportunity == null)
                    { SetApplicationsMessage("The opportunity linked to this offer is no longer available.", true); return; }
                    await applicationService.ConfirmOfferAsync(application, student, opportunity, auth.Uid, auth.IdToken);
                    Response.Redirect(ResolveUrl("~/StudentPages/MyApplications.aspx?offerAccepted=1"), false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Could not accept internship offer: " + ex.GetType().Name);
                    SetApplicationsMessage("We could not confirm this offer. Please try again.", true);
                }
            }));
        }

        private static string SafeJson(object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })
                .Replace("<", "\\u003c").Replace(">", "\\u003e").Replace("&", "\\u0026");
        }
        private void SetApplicationsMessage(string message, bool error)
        {
            ApplicationsMessage.Text = "<p class=\"page-feedback " + (error ? "error" : "success") + "\" role=\"status\">" +
                System.Web.HttpUtility.HtmlEncode(message) + "</p>";
        }
        private void GoToLogin() { Response.Redirect(ResolveUrl("~/PublicPages/Login"), false); Context.ApplicationInstance.CompleteRequest(); }
    }
}
