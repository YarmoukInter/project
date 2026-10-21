using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;
namespace Yarmouk.StudentPages
{
    public partial class Opportunities : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null) { GoToLogin(); return; }
                try
                {
                    var student = await new StudentService().GetStudentAsync(auth.Uid, auth.IdToken);
                    var opportunities = await new OpportunityService().GetAllAsync(auth.IdToken);
                    var companies = await new FirebaseHelper().GetAsync<Dictionary<string, Company>>(DatabasePaths.Companies, auth.IdToken)
                        ?? new Dictionary<string, Company>();
                    var items = opportunities.Where(o => o.IsActive)
                        .Select(o => {
                            Company company;
                            companies.TryGetValue(o.CompanyId ?? "", out company);
                            return new { id = o.Id, title = o.Title, description = o.Description,
                                requirements = o.Requirements, duration = o.Duration,
                                requiredTrainingHours = o.RequiredTrainingHours, workMode = o.WorkMode,
                                skills = o.Skills ?? new List<string>(),
                                deadline = o.Deadline, availablePositions = o.AvailablePositions,
                                majorIds = o.EligibleMajorIds ?? new List<string>(),
                                studentMajorId = student == null ? "" : student.MajorId,
                                questionsCount = o.ApplicationQuestions == null ? 0 : o.ApplicationQuestions.Count,
                                companyName = company == null ? "Company" : company.Name,
                                companyIndustry = company == null ? "" : company.Industry,
                                location = company == null ? "" : company.Location,
                                companyLogoUrl = company == null ? "" : company.LogoUrl };
                        }).ToList();
                    OpportunityData.Text = SafeJson(items);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Could not load student opportunities: " + ex.GetType().Name);
                    OpportunityData.Text = "[]";
                }
            }));
        }

        private static string SafeJson(object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })
                .Replace("<", "\\u003c").Replace(">", "\\u003e").Replace("&", "\\u0026");
        }
        private void GoToLogin() { Response.Redirect(ResolveUrl("~/PublicPages/Login.aspx"), false); Context.ApplicationInstance.CompleteRequest(); }
    }
}
