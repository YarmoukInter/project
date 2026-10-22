using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                var auth = Session == null ? null : Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null || string.IsNullOrWhiteSpace(auth.Uid) || string.IsNullOrWhiteSpace(auth.IdToken))
                {
                    GoToLogin();
                    return;
                }

                try
                {
                    var firebase = new FirebaseHelper();
                    var opportunityData = await firebase.GetAsync<JToken>(DatabasePaths.Opportunities, auth.IdToken);
                    var opportunityRows = ReadRows(opportunityData);
                    // Opportunity browsing must not disappear just because a related
                    // student, company, or application record is unreadable/missing.
                    Student student = null;
                    User user = null;
                    Dictionary<string, JObject> companyRows = new Dictionary<string, JObject>();
                    List<Application> applications = new List<Application>();
                    try { student = await new StudentService().GetStudentAsync(auth.Uid, auth.IdToken); } catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("Student opportunity context unavailable: " + ex.GetType().Name); }
                    try { user = await new StudentService().GetUserAsync(auth.Uid, auth.IdToken); } catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("User opportunity context unavailable: " + ex.GetType().Name); }
                    try { companyRows = await firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Companies, auth.IdToken) ?? companyRows; } catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("Company opportunity context unavailable: " + ex.GetType().Name); }
                    try { applications = await new ApplicationService().GetForStudentAsync(auth.Uid, auth.IdToken) ?? applications; } catch (Exception ex) { System.Diagnostics.Trace.TraceWarning("Application opportunity context unavailable: " + ex.GetType().Name); }
                    bool hasStudentProfile = student != null;
                    student = student ?? new Student();
                    bool internshipConfirmed = !string.IsNullOrWhiteSpace(student.AcceptedApplicationId);
                    var appliedIds = new HashSet<string>(applications.Select(application => application.OpportunityId), StringComparer.OrdinalIgnoreCase);
                    StudentData.Text = SafeJson(new { canApply = hasStudentProfile && user != null && !internshipConfirmed, fullName = user == null ? "" : user.FullName,
                        email = user == null ? auth.Email : user.Email,
                        studentId = First(student.StudentId, student.StudentNumber, user == null ? null : user.StudentId),
                        majorId = First(student.MajorId, user == null ? null : user.MajorId),
                        cvLink = user == null ? "" : user.CvLink });

                    var items = opportunityRows
                        .Where(entry => entry.Value != null)
                        .Select(entry =>
                        {
                            var row = entry.Value;
                            string companyId = ReadText(row, "companyId");
                            JObject company;
                            companyRows.TryGetValue(companyId ?? string.Empty, out company);
                            return new
                            {
                                id = First(ReadText(row, "id"), entry.Key),
                                companyId = companyId,
                                title = ReadText(row, "title") ?? "Internship opportunity",
                                description = ReadText(row, "description") ?? string.Empty,
                                requirements = ReadText(row, "requirements") ?? string.Empty,
                                duration = ReadText(row, "duration") ?? string.Empty,
                                requiredTrainingHours = ReadInt(row, "requiredTrainingHours"),
                                workMode = ReadText(row, "workMode") ?? string.Empty,
                                skills = ReadStrings(row, "skills", "requiredSkills"),
                                deadline = ReadText(row, "deadline") ?? string.Empty,
                                availablePositions = ReadInt(row, "availablePositions", "positions", "seats"),
                                filledPositions = ReadInt(row, "filledPositions"),
                                available = !internshipConfirmed && hasStudentProfile && user != null && CompanyIsApproved(company) && IsAvailable(row),
                                availabilityReason = internshipConfirmed ? "Internship already confirmed"
                                    : !hasStudentProfile || user == null ? "Complete student profile"
                                    : !CompanyIsApproved(company) ? "Company unavailable" : AvailabilityReason(row),
                                alreadyApplied = appliedIds.Contains(First(ReadText(row, "id"), entry.Key)),
                                majorIds = ReadStrings(row, "eligibleMajorIds", "majorIds", "majors", "majorId"),
                                studentMajorId = student.MajorId ?? string.Empty,
                                questions = ReadStrings(row, "applicationQuestions", "questions"),
                                companyName = First(ReadText(company, "name"), ReadText(row, "companyName"), "Company"),
                                companyIndustry = ReadText(company, "industry") ?? string.Empty,
                                location = First(ReadText(row, "location"), ReadText(company, "location")),
                                companyLogoUrl = ReadText(company, "logoUrl") ?? string.Empty
                            };
                        })
                        .ToList();

                    OpportunityData.Text = SafeJson(items);
                    if (!hasStudentProfile || user == null)
                        SetFeedback("Your student profile is incomplete. Opportunities are shown, but you cannot apply until your profile is available.");
                    if (items.Count == 0)
                    {
                        SetFeedback(opportunityRows.Count == 0
                            ? "No opportunity records were returned from the Firebase opportunities database path."
                            : "No valid opportunity records were returned from Firebase.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Could not load student opportunities: " + ex.GetType().Name);
                    OpportunityData.Text = "[]";
                    SetFeedback("Opportunities could not be loaded. Please refresh and try again.");
                }
            }));
        }

        protected void SubmitApplication_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var auth = Session == null ? null : Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null || string.IsNullOrWhiteSpace(auth.Uid)) { GoToLogin(); return; }
                string id = SelectedOpportunityId.Value;
                if (string.IsNullOrWhiteSpace(id) || id.Length > 128 || id.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/' }) >= 0)
                { ApplicationFeedback("Choose a valid opportunity and try again.", true); return; }
                try
                {
                    var studentService = new StudentService();
                    var student = await studentService.GetStudentAsync(auth.Uid, auth.IdToken);
                    if (student != null && !string.IsNullOrWhiteSpace(student.AcceptedApplicationId))
                    { ApplicationFeedback("You have already confirmed an internship. You cannot apply for another opportunity.", true); return; }
                    var user = await studentService.GetUserAsync(auth.Uid, auth.IdToken);
                    var opportunity = await new OpportunityService().GetByIdAsync(id, auth.IdToken);
                    if (student == null || user == null || opportunity == null || !OpportunityService.IsAvailableForStudent(opportunity, student))
                    { ApplicationFeedback("This opportunity is no longer available for your account.", true); return; }
                    var company = string.IsNullOrWhiteSpace(opportunity.CompanyId) ? null :
                        await new FirebaseHelper().GetAsync<Company>(DatabasePaths.Companies + "/" + opportunity.CompanyId, auth.IdToken);
                    if (company == null || !company.IsApproved)
                    { ApplicationFeedback("The company for this opportunity is unavailable.", true); return; }
                    var service = new ApplicationService();
                    var existing = await service.GetForStudentAsync(auth.Uid, auth.IdToken);
                    if (existing.Any(application => string.Equals(application.OpportunityId, id, StringComparison.OrdinalIgnoreCase)))
                    { ApplicationFeedback("You have already applied for this opportunity.", true); return; }
                    var questions = (opportunity.ApplicationQuestions ?? new List<string>())
                        .Where(question => !string.IsNullOrWhiteSpace(question)).Select(question => question.Trim()).ToList();
                    Dictionary<string, string> answers;
                    try { answers = JsonConvert.DeserializeObject<Dictionary<string, string>>(ApplicationAnswers.Value ?? "") ?? new Dictionary<string, string>(); }
                    catch { ApplicationFeedback("Review your answers and try again.", true); return; }
                    var verifiedAnswers = new List<string>();
                    for (int i = 0; i < questions.Count; i++)
                    {
                        string answer;
                        if (!answers.TryGetValue(i.ToString(), out answer) || string.IsNullOrWhiteSpace(answer) || answer.Trim().Length > 3000)
                        { ApplicationFeedback("Answer every company question (up to 3,000 characters each).", true); return; }
                        verifiedAnswers.Add(answer.Trim());
                    }
                    await service.SaveForStudentAsync(new Application {
                        StudentUid = auth.Uid, OpportunityId = id, CompanyId = opportunity.CompanyId,
                        Status = "pending", Answers = verifiedAnswers, ApplicationQuestions = questions
                    }, auth.Uid, auth.IdToken);
                    Response.Redirect(ResolveUrl("~/StudentPages/MyApplications.aspx?submitted=1"), false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Opportunity application failed: " + ex.GetType().Name);
                    ApplicationFeedback("The application could not be submitted. Please try again.", true);
                }
            }));
        }

        private void ApplicationFeedback(string message, bool error)
        {
            SetFeedback(message);
            if (error)
                ClientScript.RegisterStartupScript(GetType(), "reopenOpportunityModal",
                    "window.reopenOpportunityId=" + SafeJson(SelectedOpportunityId.Value) +
                    ";window.reopenOpportunityAnswers=" + SafeJson(ApplicationAnswers.Value) +
                    ";window.reopenOpportunityMessage=" + SafeJson(message) + ";", true);
        }

        private static bool CompanyIsApproved(JObject company)
        {
            if (company == null) return false;
            string approved = Normalize(ReadText(company, "isApproved", "approved", "status"));
            return approved != "false" && approved != "0" && approved != "no" &&
                approved != "inactive" && approved != "disabled" && approved != "pending" && approved != "rejected";
        }

        private static bool IsAvailable(JObject row)
        {
            return string.IsNullOrEmpty(AvailabilityReason(row));
        }

        private static string AvailabilityReason(JObject row)
        {
            // Older Firebase records use status, while newer ones use isActive.
            // Treat only an explicit inactive value as closed so records without
            // either field are not silently hidden from students.
            var activeToken = row["isActive"];
            if (activeToken != null && activeToken.Type != JTokenType.Null)
            {
                string activeValue = Normalize(activeToken.ToString());
                if (activeValue == "false" || activeValue == "0" || activeValue == "no") return "Closed";
            }
            string status = Normalize(ReadText(row, "status"));
            if (status == "inactive" || status == "closed" || status == "expired" || status == "draft" || status == "disabled" || status == "unavailable") return "Closed";
            if (status == "full") return "No seats available";

            string deadline = ReadText(row, "deadline");
            DateTime parsedDeadline;
            if (!string.IsNullOrWhiteSpace(deadline)
                && (!DateTime.TryParse(deadline, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsedDeadline)
                    || parsedDeadline.Date < DateTime.Today)) return "Deadline passed";

            int seats = ReadInt(row, "availablePositions", "positions", "seats");
            int filled = ReadInt(row, "filledPositions");
            if (seats > 0 && filled >= seats) return "No seats available";
            return string.Empty;
        }

        private static List<string> ReadStrings(JObject row, params string[] keys)
        {
            foreach (string key in keys)
            {
                var token = row == null ? null : row[key];
                var array = token as JArray;
                if (array != null)
                    return array.Where(value => value != null && value.Type != JTokenType.Null)
                        .Select(value => value.ToString().Trim()).Where(value => value.Length > 0).ToList();
                var objectValues = token as JObject;
                if (objectValues != null)
                    return objectValues.Properties()
                        .Where(property => property.Value != null && property.Value.Type != JTokenType.Null)
                        .Select(property => property.Value.Type == JTokenType.Boolean && property.Value.Value<bool>()
                            ? property.Name : property.Value.ToString().Trim())
                        .Where(value => value.Length > 0).ToList();
                if (token != null && token.Type == JTokenType.String && !string.IsNullOrWhiteSpace((string)token))
                    return ((string)token).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(value => value.Trim()).Where(value => value.Length > 0).ToList();
            }
            return new List<string>();
        }

        private static Dictionary<string, JObject> ReadRows(JToken data)
        {
            var rows = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
            var objectRows = data as JObject;
            if (objectRows != null)
            {
                foreach (var property in objectRows.Properties())
                {
                    var row = property.Value as JObject;
                    if (row != null) rows[property.Name] = row;
                }
                return rows;
            }
            var arrayRows = data as JArray;
            if (arrayRows != null)
            {
                for (int index = 0; index < arrayRows.Count; index++)
                {
                    var row = arrayRows[index] as JObject;
                    if (row == null) continue;
                    string id = First(ReadText(row, "id", "opportunityId", "key"), index.ToString(CultureInfo.InvariantCulture));
                    rows[id] = row;
                }
            }
            return rows;
        }

        private static int ReadInt(JObject row, params string[] keys)
        {
            foreach (string key in keys)
            {
                int value;
                if (row != null && row[key] != null && int.TryParse(row[key].ToString(), out value)) return value;
            }
            return 0;
        }

        private static string ReadText(JObject row, params string[] keys)
        {
            foreach (string key in keys)
            {
                var token = row == null ? null : row[key];
                if (token != null && token.Type != JTokenType.Null) return token.ToString();
            }
            return null;
        }

        private static string First(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
        }

        private void SetFeedback(string message)
        {
            OpportunityFeedback.Text = System.Web.HttpUtility.HtmlEncode(message ?? string.Empty);
        }

        private static string SafeJson(object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })
                .Replace("<", "\\u003c").Replace(">", "\\u003e").Replace("&", "\\u0026");
        }

        private void GoToLogin()
        {
            Response.Redirect(ResolveUrl("~/PublicPages/Login.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
