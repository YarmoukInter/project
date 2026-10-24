using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class FinalCompletionReview : Page
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer { MaxJsonLength = 1024 * 1024 };

        protected string CompletionCsrf {
            get {
                if (Session["CompletionCsrf"] == null) Session["CompletionCsrf"] = Guid.NewGuid().ToString("N");
                return (string)Session["CompletionCsrf"];
            }
        }
        private bool IsEndpoint { get { return Request.QueryString["action"] != null || Request.QueryString["pdf"] != null; } }
        protected override void Render(HtmlTextWriter writer) { if (!IsEndpoint) base.Render(writer); }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsEndpoint) RegisterAsyncTask(new PageAsyncTask(ProcessActionAsync));
            else if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadReviewDataAsync));
        }
        private async Task ProcessActionAsync()
        {
            Response.Clear();
            Response.Cache.SetNoStore();
            Response.TrySkipIisCustomErrors = true;
            try {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null) throw new UnauthorizedAccessException("Please sign in again.");
                var service = new CompletionService();
                if (Request.QueryString["pdf"] != null && Request.HttpMethod == "GET") {
                    byte[] bytes = await service.GetCoordinatorPdfAsync(Request.QueryString["pdf"], auth.Uid, auth.IdToken);
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", "inline; filename=internship-final-report.pdf");
                    Response.BinaryWrite(bytes);
                    return;
                }
                if (Request.HttpMethod != "POST") throw new ArgumentException("Use POST for completion actions.");
                if (Request.Headers["X-Completion-CSRF"] != CompletionCsrf) throw new UnauthorizedAccessException("Reload the page before saving.");
                if (Request.ContentLength > 8192) throw new ArgumentException("Request too large.");
                JObject body;
                using (var reader = new System.IO.StreamReader(Request.InputStream)) body = JObject.Parse(reader.ReadToEnd());
                string id = (string)body["id"];
                switch (Request.QueryString["action"]) {
                    case "hours":
                        int hours;
                        if (!int.TryParse((string)body["hours"], out hours)) throw new ArgumentException("Enter a whole number of verified hours.");
                        await service.ApproveHoursAsync(id, auth.Uid, auth.IdToken, hours, (string)body["note"]); break;
                    case "approve": await service.ApproveCompletionAsync(id, auth.Uid, auth.IdToken); break;
                    case "send": await service.SendReportAsync(id, auth.Uid, auth.IdToken); break;
                    default: throw new ArgumentException("Unknown action.");
                }
                WriteJson(200, new { success = true });
            } catch (UnauthorizedAccessException e) { WriteJson(403, new { success = false, message = e.Message }); }
              catch (ArgumentException e) { WriteJson(400, new { success = false, message = e.Message }); }
              catch (InvalidOperationException e) { WriteJson(409, new { success = false, message = e.Message }); }
              catch (Exception e) {
                Trace.Warn("Completion", "Completion action failed", e);
                WriteJson(500, new { success = false, message = "The action could not be saved. Please try again." });
              }
        }
        private void WriteJson(int status, object value) {
            Response.StatusCode = status;
            Response.ContentType = "application/json";
            Response.Write(JsonConvert.SerializeObject(value));
        }

        private async Task LoadReviewDataAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null || string.IsNullOrWhiteSpace(auth.IdToken))
            {
                RegisterClientData(new List<CompletionReviewRow>(), "Your session has expired. Sign in again to view completion reviews.");
                return;
            }

            try
            {
                var firebase = new FirebaseHelper();
                var evaluationsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Evaluations, auth.IdToken);
                var placementsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Placements, auth.IdToken);
                var studentsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Students, auth.IdToken);
                var companiesRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Companies, auth.IdToken);
                var opportunitiesRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Opportunities, auth.IdToken);
                var logsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TrainingLogs, auth.IdToken);

                await Task.WhenAll(evaluationsRequest, placementsRequest, studentsRequest, companiesRequest, opportunitiesRequest, logsRequest);

                var rows = BuildRows(await evaluationsRequest, await placementsRequest, await studentsRequest, await companiesRequest, await opportunitiesRequest, await logsRequest);
                RegisterClientData(rows, rows.Count == 0
                    ? "No final evaluations are currently available for coordinator review."
                    : string.Empty);
            }
            catch (Exception ex)
            {
                Trace.Warn("FinalCompletionReview", "Could not load coordinator completion reviews.", ex);
                RegisterClientData(new List<CompletionReviewRow>(),
                    "Completion data could not be loaded. Check coordinator Firebase read permissions and try again.");
            }
        }

        private void RegisterClientData(IList<CompletionReviewRow> rows, string message)
        {
            string json = Serializer.Serialize(rows ?? new List<CompletionReviewRow>());
            string safeJson = HttpUtility.JavaScriptStringEncode(json);
            string safeMessage = HttpUtility.JavaScriptStringEncode(message ?? string.Empty);
            ClientScript.RegisterStartupScript(GetType(), "uimsFinalReviewData",
                "window.uimsFinalReviewData=JSON.parse(\"" + safeJson + "\");window.uimsFinalReviewMessage=\"" + safeMessage + "\";", true);
        }

        private static List<CompletionReviewRow> BuildRows(
            IDictionary<string, JObject> evaluations, IDictionary<string, JObject> placements,
            IDictionary<string, JObject> students, IDictionary<string, JObject> companies, IDictionary<string, JObject> opportunities,
            IDictionary<string, JObject> logs)
        {
            var result = new List<CompletionReviewRow>();
            if (evaluations == null) return result;

            foreach (var item in evaluations)
            {
                var evaluation = item.Value;
                if (evaluation == null) continue;
                string studentUid = Text(evaluation, "studentUid");
                if (string.IsNullOrWhiteSpace(studentUid)) continue;

                string placementId = Text(evaluation, "placementId");
                JObject placementRecord = FindPlacement(placements, placementId, studentUid);
                JObject studentRecord = Find(students, studentUid);
                string companyId = Text(placementRecord, "companyId");
                string opportunityId = Text(placementRecord, "opportunityId");
                JObject company = Find(companies, companyId);
                JObject opportunity = Find(opportunities, opportunityId);

                int requiredHours = Number(placementRecord, "requiredHours");
                if (requiredHours <= 0) requiredHours = Number(studentRecord, "requiredTrainingHours");
                if (requiredHours <= 0) requiredHours = Number(studentRecord, "requiredHours");
                if (requiredHours <= 0) requiredHours = Number(opportunity, "requiredTrainingHours");
                int approvedHours = placementRecord != null && placementRecord["approvedHours"] != null
                    ? Number(placementRecord, "approvedHours") : Number(studentRecord, "approvedTrainingHours");
                if (approvedHours <= 0 && (placementRecord == null || placementRecord["approvedHours"] == null)) approvedHours = SumApprovedHours(logs, studentUid, placementId);

                bool companySubmitted = Flag(evaluation, "companySubmitted");
                bool academicSubmitted = Flag(evaluation, "academicSubmitted");
                bool coordinatorApproved = Flag(evaluation, "coordinatorApproved");
                bool hoursConfigured = requiredHours > 0;
                bool hoursComplete = hoursConfigured && approvedHours >= requiredHours;
                bool eligible = companySubmitted && academicSubmitted && hoursComplete && placementRecord != null;
                string status = coordinatorApproved ? "approved" : eligible ? "ready" : "missing";

                string name = Text(studentRecord, "englishName");
                if (string.IsNullOrWhiteSpace(name)) name = Text(studentRecord, "fullName");
                if (string.IsNullOrWhiteSpace(name)) name = Text(studentRecord, "name");
                if (string.IsNullOrWhiteSpace(name)) name = "Student";
                string companyName = Text(company, "name");
                string opportunityTitle = Text(opportunity, "title");

                result.Add(new CompletionReviewRow
                {
                    id = item.Key,
                    reportSent = evaluation["reportSentAtUnixMs"] != null,
                    finalScore = evaluation["finalScore"] == null ? "" : Text(evaluation, "finalScore"),
                    name = name,
                    studentId = First(Text(studentRecord, "studentId"), Text(studentRecord, "studentNumber")),
                    initials = Initials(name),
                    program = Text(studentRecord, "majorId"),
                    company = string.IsNullOrWhiteSpace(companyName) ? "Training provider unavailable" : companyName,
                    opportunity = opportunityTitle,
                    status = status,
                    companySubmitted = companySubmitted,
                    academicSubmitted = academicSubmitted,
                    companyScore = Score(evaluation, "companyScore", companySubmitted),
                    academicScore = Score(evaluation, "academicScore", academicSubmitted),
                    coordinatorApproved = coordinatorApproved,
                    approvedHours = approvedHours,
                    requiredHours = requiredHours,
                    hoursConfigured = hoursConfigured,
                    placementFound = placementRecord != null,
                    checks = new[]
                    {
                        new CompletionCheck { label = "Company final evaluation", detail = companySubmitted ? "Submitted" : "Not submitted", complete = companySubmitted },
                        new CompletionCheck { label = "Academic final evaluation", detail = academicSubmitted ? "Submitted" : "Not submitted", complete = academicSubmitted },
                        new CompletionCheck { label = "Approved training hours", detail = hoursConfigured ? approvedHours.ToString(CultureInfo.InvariantCulture) + " / " + requiredHours.ToString(CultureInfo.InvariantCulture) + " hours" : "Required hours are not configured", complete = hoursComplete },
                        new CompletionCheck { label = "Placement record", detail = placementRecord != null ? "Found" : "No matching placement record", complete = placementRecord != null }
                    }
                });
            }

            result.Sort((a, b) => StatusOrder(a.status).CompareTo(StatusOrder(b.status)) != 0
                ? StatusOrder(a.status).CompareTo(StatusOrder(b.status))
                : string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private static JObject FindPlacement(IDictionary<string, JObject> placements, string placementId, string studentUid)
        {
            if (placements == null) return null;
            JObject record;
            return !string.IsNullOrWhiteSpace(placementId) && placements.TryGetValue(placementId, out record)
                && record != null && Text(record, "studentUid") == studentUid ? record : null;
        }

        private static int SumApprovedHours(IDictionary<string, JObject> logs, string studentUid, string placementId)
        {
            if (logs == null) return 0;
            return logs.Where(x => x.Value != null
                    && string.Equals(Text(x.Value, "studentUid"), studentUid, StringComparison.Ordinal)
                    && (string.IsNullOrWhiteSpace(Text(x.Value, "placementId")) || string.IsNullOrWhiteSpace(placementId)
                        || string.Equals(Text(x.Value, "placementId"), placementId, StringComparison.Ordinal))
                    && (Flag(x.Value, "isApproved") || string.Equals(Text(x.Value, "status"), "approved", StringComparison.OrdinalIgnoreCase)))
                .Sum(x => Number(x.Value, "hours"));
        }

        private static JObject Find(IDictionary<string, JObject> records, string key)
        {
            JObject value;
            return records != null && !string.IsNullOrWhiteSpace(key) && records.TryGetValue(key, out value) ? value : null;
        }

        private static string Text(JObject value, string key)
        {
            JToken token;
            return value != null && value.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out token) && token.Type != JTokenType.Null
                ? Convert.ToString(((JValue)token).Value, CultureInfo.InvariantCulture) : string.Empty;
        }

        private static int Number(JObject value, string key)
        {
            int number;
            return int.TryParse(Text(value, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out number) ? Math.Max(0, number) : 0;
        }

        private static bool Flag(JObject value, string key)
        {
            bool flag;
            return bool.TryParse(Text(value, key), out flag) && flag;
        }

        private static string Score(JObject evaluation, string key, bool submitted)
        {
            if (!submitted) return "Not submitted";
            string value = Text(evaluation, key);
            return string.IsNullOrWhiteSpace(value) ? "Submitted · score unavailable" : value;
        }

        private static string First(string first, string second) { return string.IsNullOrWhiteSpace(first) ? second : first; }
        private static int StatusOrder(string status) { return status == "ready" ? 0 : status == "missing" ? 1 : 2; }
        private static string Initials(string name)
        {
            var parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Concat(parts.Take(2).Select(x => x.Substring(0, 1))).ToUpperInvariant();
        }

        private sealed class CompletionReviewRow
        {
            public bool reportSent { get; set; } public string finalScore { get; set; }
            public string id { get; set; } public string name { get; set; } public string studentId { get; set; }
            public string initials { get; set; } public string program { get; set; } public string company { get; set; }
            public string opportunity { get; set; } public string status { get; set; } public bool companySubmitted { get; set; }
            public bool academicSubmitted { get; set; } public string companyScore { get; set; } public string academicScore { get; set; }
            public bool coordinatorApproved { get; set; } public int approvedHours { get; set; } public int requiredHours { get; set; }
            public bool hoursConfigured { get; set; } public bool placementFound { get; set; } public CompletionCheck[] checks { get; set; }
        }
        private sealed class CompletionCheck { public string label { get; set; } public string detail { get; set; } public bool complete { get; set; } }
    }
}