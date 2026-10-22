using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CompanyPages
{
    public partial class ManageOpportunities : Page
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsApiRequest) RegisterAsyncTask(new PageAsyncTask(ProcessApiRequestAsync));
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (!IsApiRequest) base.Render(writer);
        }

        private bool IsApiRequest
        {
            get { return !string.IsNullOrWhiteSpace(Request.QueryString["action"]); }
        }

        private async Task ProcessApiRequestAsync()
        {
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            try
            {
                var auth = await VerifyCompanyAsync();
                var workflow = new CompanyWorkflowService();
                string action = Request.QueryString["action"] ?? string.Empty;
                if (string.Equals(Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                {
                    if (action != "opportunities") { WriteJson(400, new { success = false, message = "Unknown request." }); return; }
                    var opportunities = await workflow.GetOpportunitiesAsync(auth.CompanyId, auth.Session.IdToken);
                    WriteJson(200, new { success = true, opportunities = opportunities });
                    return;
                }
                if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    Response.AppendHeader("Allow", "GET, POST");
                    WriteJson(405, new { success = false, message = "Method not allowed." });
                    return;
                }
                if (!string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal))
                { WriteJson(400, new { success = false, message = "Invalid request." }); return; }
                if (Request.ContentLength > 65536)
                { WriteJson(413, new { success = false, message = "Request is too large." }); return; }

                JObject body;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8)) body = JObject.Parse(reader.ReadToEnd());
                action = (string)body["action"] ?? action;
                if (action == "saveOpportunity")
                {
                    var input = body["opportunity"] == null ? null : body["opportunity"].ToObject<CompanyOpportunityInput>();
                    var item = await workflow.SaveOpportunityAsync(auth.CompanyId, auth.Session.IdToken, input);
                    WriteJson(200, new { success = true, opportunity = item });
                }
                else if (action == "setOpportunityStatus")
                {
                    await workflow.SetOpportunityStatusAsync(auth.CompanyId, (string)body["id"], (string)body["status"], auth.Session.IdToken);
                    WriteJson(200, new { success = true });
                }
                else WriteJson(400, new { success = false, message = "Unknown request." });
            }
            catch (UnauthorizedAccessException)
            { WriteJson(401, new { success = false, message = "Your company session is no longer valid. Sign in again." }); }
            catch (AuthException)
            { WriteJson(401, new { success = false, message = "Your company session is no longer valid. Sign in again." }); }
            catch (ArgumentException error)
            { WriteJson(400, new { success = false, message = error.Message }); }
            catch (JsonException)
            { WriteJson(400, new { success = false, message = "Invalid request data." }); }
            catch (InvalidOperationException error)
            { WriteJson(409, new { success = false, message = error.Message }); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company opportunity request failed: " + error.GetType().Name);
                WriteJson(500, new { success = false, message = "Firebase could not complete this request. Check the database rules and try again." });
            }
        }

        private async Task<VerifiedCompany> VerifyCompanyAsync()
        {
            var session = Session == null ? null : Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null || string.IsNullOrWhiteSpace(session.Uid) || string.IsNullOrWhiteSpace(session.IdToken))
                throw new UnauthorizedAccessException();
            await new FirebaseAuthService().VerifyAsync(session);
            var user = await new StudentService().GetUserAsync(session.Uid, session.IdToken);
            if (user == null || !user.IsActive || !string.Equals(AuthPolicy.NormalizeRole(user.Role), "company", StringComparison.Ordinal))
                throw new UnauthorizedAccessException();
            session.User = user;

            var firebase = new FirebaseHelper();
            var userRecord = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + session.Uid, session.IdToken);
            string companyId = userRecord == null ? null : (string)userRecord["companyId"];
            if (string.IsNullOrWhiteSpace(companyId))
            {
                var companies = await firebase.GetAsync<System.Collections.Generic.Dictionary<string, JObject>>(DatabasePaths.Companies, session.IdToken);
                if (companies != null)
                {
                    var match = companies.FirstOrDefault(entry => entry.Value != null
                        && ((entry.Value["supervisorUids"] is JObject supervisors
                                && supervisors[session.Uid] != null && supervisors[session.Uid].Value<bool>())
                            || string.Equals((string)entry.Value["supervisorUid"], session.Uid, StringComparison.Ordinal)));
                    if (!string.IsNullOrWhiteSpace(match.Key)) companyId = match.Key;
                }
            }
            if (string.IsNullOrWhiteSpace(companyId)) throw new UnauthorizedAccessException();
            var company = await firebase.GetAsync<Company>(DatabasePaths.Companies + "/" + companyId, session.IdToken);
            if (company == null || !company.IsApproved) throw new UnauthorizedAccessException();
            return new VerifiedCompany { Session = session, CompanyId = companyId };
        }

        private void WriteJson(int statusCode, object value)
        {
            Response.StatusCode = statusCode;
            Response.TrySkipIisCustomErrors = true;
            Response.Write(JsonConvert.SerializeObject(value, JsonSettings));
        }

        private sealed class VerifiedCompany
        {
            public AuthSession Session { get; set; }
            public string CompanyId { get; set; }
        }
    }
}

namespace Yarmouk.CompanyPages
{
    // Kept in the page code-behind because this project does not include a workflow service.
    public sealed class CompanyOpportunityInput
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.Collections.Generic.List<string> Majors { get; set; }
        public int Seats { get; set; }
        public string Deadline { get; set; }
        public string Status { get; set; }
        public string Duration { get; set; }
        public int Hours { get; set; }
        public string WorkMode { get; set; }
        public string Requirements { get; set; }
        public System.Collections.Generic.List<string> Questions { get; set; }
    }

    public sealed class CompanyOpportunityItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public System.Collections.Generic.List<string> Majors { get; set; }
        public int Seats { get; set; }
        public string Deadline { get; set; }
        public string Status { get; set; }
        public int Applications { get; set; }
        public string Duration { get; set; }
        public int Hours { get; set; }
        public string WorkMode { get; set; }
        public string Description { get; set; }
        public string Requirements { get; set; }
        public System.Collections.Generic.List<string> Questions { get; set; }
    }

    public sealed class CompanyApplicationItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Major { get; set; }
        public string Email { get; set; }
        public long AppliedAtUnixMs { get; set; }
        public string OpportunityId { get; set; }
        public string Opportunity { get; set; }
        public string Status { get; set; }
        public string CvUrl { get; set; }
        public System.Collections.Generic.List<string> Answers { get; set; }
        public System.Collections.Generic.List<string> Questions { get; set; }
        public string RejectionReason { get; set; }
        public string DecisionNote { get; set; }
    }

    public sealed class CompanyWorkflowService
    {
        private readonly Yarmouk.Helpers.FirebaseHelper firebase = new Yarmouk.Helpers.FirebaseHelper();

        public async System.Threading.Tasks.Task<System.Collections.Generic.List<CompanyOpportunityItem>> GetOpportunitiesAsync(string companyId, string idToken)
        {
            var opportunities = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Opportunities, idToken);
            var applications = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Applications, idToken);
            var result = new System.Collections.Generic.List<CompanyOpportunityItem>();
            foreach (var entry in opportunities)
            {
                if (!Same(Text(entry.Value, "companyId"), companyId)) continue;
                result.Add(ToOpportunity(entry.Key, entry.Value, applications));
            }
            result.Sort((a, b) => string.Compare(a.Title, b.Title, System.StringComparison.CurrentCultureIgnoreCase));
            return result;
        }

        public async System.Threading.Tasks.Task<CompanyOpportunityItem> SaveOpportunityAsync(string companyId, string idToken, CompanyOpportunityInput input)
        {
            Validate(input);
            var company = await firebase.GetAsync<Yarmouk.Models.Company>(Yarmouk.Helpers.DatabasePaths.Companies + "/" + companyId, idToken);
            if (company == null || !company.IsApproved) throw new System.UnauthorizedAccessException();
            JObject existing = null;
            string id = input.Id;
            if (!string.IsNullOrWhiteSpace(id))
            {
                ValidateKey(id);
                existing = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, idToken);
                if (existing == null || !Same(Text(existing, "companyId"), companyId)) throw new System.UnauthorizedAccessException();
            }
            else id = "OPP" + System.Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();

            string status = Normalize(input.Status);
            if (status == "published") status = "active";
            if (status != "active" && status != "draft" && !(status == "closed" && !string.IsNullOrWhiteSpace(input.Id)))
                throw new System.ArgumentException("Choose a valid opportunity status.");
            string workMode = Normalize(input.WorkMode);
            if (workMode != "onsite" && workMode != "hybrid" && workMode != "remote") throw new System.ArgumentException("Choose a valid work mode.");

            var row = existing == null ? new JObject() : (JObject)existing.DeepClone();
            row["id"] = id;
            row["companyId"] = companyId;
            row["title"] = input.Title.Trim();
            row["description"] = input.Description.Trim();
            row["eligibleMajorIds"] = JArray.FromObject(input.Majors ?? new System.Collections.Generic.List<string>());
            row["availablePositions"] = input.Seats;
            row["isActive"] = status == "active";
            row["status"] = status;
            row["deadline"] = input.Deadline;
            row["location"] = string.IsNullOrWhiteSpace(company.Location) ? Text(existing, "location") : company.Location;
            row["applicationQuestions"] = JArray.FromObject(input.Questions ?? new System.Collections.Generic.List<string>());
            row["requirements"] = input.Requirements ?? string.Empty;
            row["duration"] = input.Duration ?? string.Empty;
            row["requiredTrainingHours"] = input.Hours;
            row["workMode"] = workMode == "onsite" ? "OnSite" : workMode == "hybrid" ? "Hybrid" : "Remote";
            await firebase.SaveAsync(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, row, idToken);
            var apps = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Applications, idToken);
            return ToOpportunity(id, row, apps);
        }

        public async System.Threading.Tasks.Task SetOpportunityStatusAsync(string companyId, string id, string status, string idToken)
        {
            ValidateKey(id);
            var row = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, idToken);
            if (row == null || !Same(Text(row, "companyId"), companyId)) throw new System.UnauthorizedAccessException();
            status = Normalize(status);
            if (status != "active" && status != "closed" && status != "draft") throw new System.ArgumentException("Choose a valid opportunity status.");
            await firebase.UpdateAsync(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + id, new { status = status, isActive = status == "active" }, idToken);
        }

        public async System.Threading.Tasks.Task<System.Collections.Generic.List<CompanyApplicationItem>> GetApplicationsAsync(string companyId, string idToken)
        {
            var opportunities = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Opportunities, idToken);
            var applications = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Applications, idToken);
            var users = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Users, idToken);
            var students = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Students, idToken);
            var majors = await GetMapAsync(Yarmouk.Helpers.DatabasePaths.Majors, idToken);
            var result = new System.Collections.Generic.List<CompanyApplicationItem>();
            foreach (var entry in applications)
            {
                var app = entry.Value;
                var opportunityId = Text(app, "opportunityId");
                JObject opportunity;
                if (!opportunities.TryGetValue(opportunityId ?? string.Empty, out opportunity) || !Same(Text(opportunity, "companyId"), companyId)) continue;
                string studentUid = Text(app, "studentUid") ?? string.Empty;
                JObject user; users.TryGetValue(studentUid, out user);
                JObject student; students.TryGetValue(studentUid, out student);
                JObject major; majors.TryGetValue(Text(student, "majorId") ?? string.Empty, out major);
                var answers = new System.Collections.Generic.List<string>();
                var questions = Strings(app["applicationQuestions"]);
                if (questions.Count == 0) questions = Strings(opportunity["applicationQuestions"]);
                var answerMap = app["answers"] as JObject;
                var answerList = app["answers"] as JArray;
                for (int i = 0; i < questions.Count; i++)
                    answers.Add(answerList != null && i < answerList.Count
                        ? answerList[i].ToString() : answerMap == null ? string.Empty : Text(answerMap, i.ToString()) ?? string.Empty);
                string status = Normalize(Text(app, "status"));
                status = status == "offered" || status == "accepted" || status == "offerreceived" ? "accepted" : status == "rejected" ? "rejected" : status == "inreview" || status == "underreview" ? "in-review" : "new";
                result.Add(new CompanyApplicationItem {
                    Id = Text(app, "id") ?? entry.Key,
                    Name = First(Text(user, "fullName"), "Student"),
                    Number = First(Text(student, "studentNumber"), Text(student, "studentId"), Text(user, "studentId"), "—"),
                    Major = First(Text(major, "name"), Text(student, "majorId"), Text(user, "majorId"), "—"),
                    Email = First(Text(user, "email"), ""),
                    AppliedAtUnixMs = Number(app, "appliedAtUnixMs"),
                    OpportunityId = opportunityId,
                    Opportunity = First(Text(opportunity, "title"), "Internship opportunity"),
                    Status = status,
                    CvUrl = First(Text(user, "cvLink"), Text(student, "cvUrl"), ""),
                    Answers = answers,
                    Questions = questions,
                    RejectionReason = Text(app, "rejectionReason") ?? string.Empty,
                    DecisionNote = status == "accepted" ? "Offer sent; waiting for student confirmation." : string.Empty
                });
            }
            result.Sort((a, b) => b.AppliedAtUnixMs.CompareTo(a.AppliedAtUnixMs));
            return result;
        }

        public async System.Threading.Tasks.Task SetApplicationStatusAsync(string companyId, string reviewerUid, string id, string status, string reason, string idToken)
        {
            ValidateKey(id);
            var app = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Applications + "/" + id, idToken);
            if (app == null) throw new System.ArgumentException("Application not found.");
            var opportunity = await firebase.GetAsync<JObject>(Yarmouk.Helpers.DatabasePaths.Opportunities + "/" + Text(app, "opportunityId"), idToken);
            if (opportunity == null || !Same(Text(opportunity, "companyId"), companyId)) throw new System.UnauthorizedAccessException();
            string normalized = Normalize(status);
            string value = normalized == "inreview" ? "InReview" : normalized == "offered" || normalized == "accepted" ? "Offered" : normalized == "rejected" ? "Rejected" : null;
            if (value == null) throw new System.ArgumentException("Choose a valid application decision.");
            string current = Normalize(Text(app, "status"));
            if (current == "accepted" || current == "rejected" || current == "offered" || current == "offerreceived") throw new System.InvalidOperationException("This application already has a final decision.");
            if (value == "Rejected" && string.IsNullOrWhiteSpace(reason)) throw new System.ArgumentException("A rejection reason is required.");
            await firebase.UpdateAsync(Yarmouk.Helpers.DatabasePaths.Applications + "/" + id, new { companyId = companyId, status = value, rejectionReason = value == "Rejected" ? reason.Trim() : string.Empty, reviewedByUid = reviewerUid, reviewedAtUnixMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, idToken);
        }

        private async System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, JObject>> GetMapAsync(string path, string token)
        {
            return await firebase.GetAsync<System.Collections.Generic.Dictionary<string, JObject>>(path, token) ?? new System.Collections.Generic.Dictionary<string, JObject>();
        }
        private static CompanyOpportunityItem ToOpportunity(string id, JObject row, System.Collections.Generic.Dictionary<string, JObject> apps)
        {
            string status = Text(row, "status");
            if (string.IsNullOrWhiteSpace(status)) status = Bool(row, "isActive") ? "active" : "closed";
            int count = 0;
            foreach (var app in apps.Values) if (Same(Text(app, "opportunityId"), id)) count++;
            return new CompanyOpportunityItem { Id = Text(row, "id") ?? id, Title = Text(row, "title") ?? string.Empty, Majors = Strings(row["eligibleMajorIds"]), Seats = Integer(row, "availablePositions"), Deadline = Text(row, "deadline") ?? string.Empty, Status = Normalize(status), Applications = count, Duration = Text(row, "duration") ?? string.Empty, Hours = Integer(row, "requiredTrainingHours"), WorkMode = Text(row, "workMode") ?? "OnSite", Description = Text(row, "description") ?? string.Empty, Requirements = Text(row, "requirements") ?? string.Empty, Questions = Strings(row["applicationQuestions"]) };
        }
        private static void Validate(CompanyOpportunityInput input)
        {
            if (input == null) throw new System.ArgumentException("Opportunity details are required.");
            if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 120) throw new System.ArgumentException("Enter an internship title up to 120 characters.");
            if (string.IsNullOrWhiteSpace(input.Description) || input.Description.Trim().Length > 1500) throw new System.ArgumentException("Enter a description up to 1500 characters.");
            if (input.Seats < 1 || input.Seats > 500) throw new System.ArgumentException("Available seats must be between 1 and 500.");
            if (input.Hours < 0 || input.Hours > 10000) throw new System.ArgumentException("Training hours must be between 0 and 10000.");
            System.DateTime deadline;
            if (!System.DateTime.TryParseExact(input.Deadline ?? string.Empty, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out deadline)) throw new System.ArgumentException("Choose a valid application deadline.");
            if (input.Majors == null || input.Majors.Count == 0) throw new System.ArgumentException("Select at least one eligible major.");
            if (input.Questions != null && input.Questions.Count > 20) throw new System.ArgumentException("Use no more than 20 application questions.");
        }
        private static string Text(JObject row, string key) { return row == null || row[key] == null ? null : row[key].Type == JTokenType.String ? (string)row[key] : row[key].ToString(); }
        private static int Integer(JObject row, string key) { int value; return int.TryParse(Text(row, key), out value) ? value : 0; }
        private static long Number(JObject row, string key) { long value; return long.TryParse(Text(row, key), out value) ? value : 0L; }
        private static bool Bool(JObject row, string key) { return row != null && row[key] != null && row[key].Value<bool>(); }
        private static System.Collections.Generic.List<string> Strings(JToken token) { return token == null || token.Type == JTokenType.Null ? new System.Collections.Generic.List<string>() : token.ToObject<System.Collections.Generic.List<string>>() ?? new System.Collections.Generic.List<string>(); }
        private static string First(params string[] values) { return System.Linq.Enumerable.FirstOrDefault(values, value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty; }
        private static string Normalize(string value) { return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant(); }
        private static bool Same(string left, string right) { return !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right) && string.Equals(left, right, System.StringComparison.Ordinal); }
        private static void ValidateKey(string value) { if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?', '&' }) >= 0) throw new System.ArgumentException("Invalid Firebase record id."); }
    }
}
