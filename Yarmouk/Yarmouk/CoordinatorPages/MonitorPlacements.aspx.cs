using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class MonitorPlacements : Page
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer
        {
            MaxJsonLength = 1024 * 1024
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            RegisterAsyncTask(new PageAsyncTask(LoadPlacementsAsync));
        }

        private async Task LoadPlacementsAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null || string.IsNullOrWhiteSpace(auth.IdToken))
            {
                RegisterClientData(new List<MonitorPlacementRow>(), "Your session has expired. Sign in again to view placement data.");
                return;
            }

            try
            {
                var firebase = new FirebaseHelper();
                var placementsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Placements, auth.IdToken);
                var studentsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Students, auth.IdToken);
                var usersRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Users, auth.IdToken);
                var companiesRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Companies, auth.IdToken);
                var opportunitiesRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Opportunities, auth.IdToken);
                var tasksRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Tasks, auth.IdToken);
                var submissionsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TaskSubmissions, auth.IdToken);
                var logsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TrainingLogs, auth.IdToken);
                var evaluationsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Evaluations, auth.IdToken);

                await Task.WhenAll(placementsRequest, studentsRequest, usersRequest, companiesRequest,
                    opportunitiesRequest, tasksRequest, submissionsRequest, logsRequest, evaluationsRequest);

                var rows = BuildRows(
                    await placementsRequest,
                    await studentsRequest,
                    await usersRequest,
                    await companiesRequest,
                    await opportunitiesRequest,
                    await tasksRequest,
                    await submissionsRequest,
                    await logsRequest,
                    await evaluationsRequest);

                RegisterClientData(rows, rows.Count == 0
                    ? "No internship placements are currently available for monitoring."
                    : string.Empty);
            }
            catch (Exception ex)
            {
                Trace.Warn("MonitorPlacements", "Could not load coordinator placement data.", ex);
                RegisterClientData(new List<MonitorPlacementRow>(),
                    "Placement data could not be loaded. Check the coordinator's Firebase read permissions and try again.");
            }
        }

        private void RegisterClientData(IList<MonitorPlacementRow> rows, string message)
        {
            string json = Serializer.Serialize(rows ?? new List<MonitorPlacementRow>());
            string safeJson = HttpUtility.JavaScriptStringEncode(json);
            string safeMessage = HttpUtility.JavaScriptStringEncode(message ?? string.Empty);
            string script = "window.uimsMonitorData=JSON.parse(\"" + safeJson + "\");" +
                "window.uimsMonitorMessage=\"" + safeMessage + "\";";
            ClientScript.RegisterStartupScript(GetType(), "uimsMonitorData", script, true);
        }

        private static List<MonitorPlacementRow> BuildRows(
            IDictionary<string, JObject> placementRecords,
            IDictionary<string, JObject> studentRecords,
            IDictionary<string, JObject> userRecords,
            IDictionary<string, JObject> companyRecords,
            IDictionary<string, JObject> opportunityRecords,
            IDictionary<string, JObject> taskRecords,
            IDictionary<string, JObject> submissionRecords,
            IDictionary<string, JObject> logRecords,
            IDictionary<string, JObject> evaluationRecords)
        {
            var rows = new List<MonitorPlacementRow>();
            if (placementRecords == null) return rows;

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var entry in placementRecords)
            {
                var source = entry.Value;
                if (source == null) continue;
                var placement = source.ToObject<Placement>();
                if (placement == null || string.IsNullOrWhiteSpace(placement.StudentUid)) continue;

                JObject studentRecord;
                JObject userRecord;
                JObject companyRecord;
                JObject opportunityRecord;
                if (studentRecords == null || !studentRecords.TryGetValue(placement.StudentUid, out studentRecord))
                    studentRecord = null;
                if (userRecords == null || !userRecords.TryGetValue(placement.StudentUid, out userRecord))
                    userRecord = null;
                if (companyRecords == null || !companyRecords.TryGetValue(placement.CompanyId ?? string.Empty, out companyRecord))
                    companyRecord = null;
                if (opportunityRecords == null || !opportunityRecords.TryGetValue(placement.OpportunityId ?? string.Empty, out opportunityRecord))
                    opportunityRecord = null;

                var student = studentRecord == null ? null : studentRecord.ToObject<Student>();
                var requiredHours = ReadInt(studentRecord, "requiredTrainingHours");
                if (requiredHours <= 0) requiredHours = ReadInt(studentRecord, "requiredHours");
                if (requiredHours <= 0) requiredHours = ReadInt(source, "requiredHours");
                if (requiredHours <= 0) requiredHours = ReadInt(opportunityRecord, "requiredTrainingHours");

                var approvedHours = ReadInt(studentRecord, "approvedTrainingHours");
                if (approvedHours <= 0)
                {
                    approvedHours = SumApprovedHours(logRecords, placement.StudentUid, entry.Key);
                }

                var assignedTasks = FindTasks(taskRecords, placement.StudentUid, entry.Key);
                var submittedCount = 0;
                var reviewPendingCount = 0;
                var overdueCount = 0;
                foreach (var task in assignedTasks)
                {
                    JObject latestSubmission = FindLatestSubmission(submissionRecords, GetText(task, "id"), placement.StudentUid);
                    if (latestSubmission != null)
                    {
                        submittedCount++;
                        string reviewStatus = GetText(latestSubmission, "reviewStatus");
                        if (string.IsNullOrWhiteSpace(reviewStatus) || IsStatus(reviewStatus, "pending"))
                            reviewPendingCount++;
                    }

                    long dueAt = ReadLong(task, "dueAtUnixMs");
                    string taskStatus = GetText(task, "status");
                    string submissionStatus = GetText(latestSubmission, "reviewStatus");
                    bool taskResolved = IsCompleted(taskStatus) || IsCompleted(submissionStatus);
                    if (dueAt > 0 && dueAt < now && !taskResolved)
                        overdueCount++;
                }

                JObject evaluation = FindEvaluation(evaluationRecords, placement.StudentUid, entry.Key);
                bool companyEvaluationSubmitted = ReadBool(evaluation, "companySubmitted");
                bool academicEvaluationSubmitted = ReadBool(evaluation, "academicSubmitted");
                string evaluationSummary = "Company " + (companyEvaluationSubmitted ? "received" : "pending")
                    + " · Academic " + (academicEvaluationSubmitted ? "received" : "pending");

                JObject supervisorRecord;
                string supervisorUid = placement.AcademicSupervisorUid;
                if (string.IsNullOrWhiteSpace(supervisorUid) && student != null)
                    supervisorUid = student.AcademicSupervisorId;
                if (userRecords == null || !userRecords.TryGetValue(supervisorUid ?? string.Empty, out supervisorRecord))
                    supervisorRecord = null;

                string companyName = GetText(companyRecord, "name");
                string opportunityTitle = GetText(opportunityRecord, "title");
                string studentName = GetText(userRecord, "fullName");
                if (string.IsNullOrWhiteSpace(studentName))
                    studentName = GetText(studentRecord, "englishName");
                if (string.IsNullOrWhiteSpace(studentName))
                    studentName = "Student";
                string status = IsCompleted(placement.Status) ? "complete"
                    : (overdueCount > 0 ? "attention" : "active");
                submittedCount = TrainingProgress.CountSubmitted(taskRecords, submissionRecords == null ? null : submissionRecords.Values,
                    placement.StudentUid, entry.Key, placement.OpportunityId);
                int progress = TrainingProgress.Percent(submittedCount);

                rows.Add(new MonitorPlacementRow
                {
                    name = studentName,
                    id = GetText(studentRecord, "studentId"),
                    initials = GetInitials(studentName),
                    profileImageUrl = StudentProfileImages.GetUrl(GetText(studentRecord, "profileImageFileName")),
                    role = string.IsNullOrWhiteSpace(opportunityTitle) ? "Internship placement" : opportunityTitle,
                    company = string.IsNullOrWhiteSpace(companyName) ? "Training provider unavailable" : companyName,
                    city = GetText(companyRecord, "location"),
                    supervisor = GetText(supervisorRecord, "fullName"),
                    progress = progress,
                    tasks = TrainingProgress.Label(submittedCount) + " · "
                        + reviewPendingCount.ToString(CultureInfo.InvariantCulture) + " awaiting review · "
                        + overdueCount.ToString(CultureInfo.InvariantCulture) + " overdue",
                    evaluations = evaluationSummary,
                    hours = approvedHours.ToString(CultureInfo.InvariantCulture) + " / "
                        + (requiredHours > 0 ? requiredHours.ToString(CultureInfo.InvariantCulture) : "—"),
                    status = status,
                    updated = FormatDate(ReadLong(source, "updatedAtUnixMs"), GetText(source, "startDate"))
                });
            }

            rows.Sort((left, right) =>
            {
                int priorityOrder = StatusPriority(left.status).CompareTo(StatusPriority(right.status));
                return priorityOrder != 0 ? priorityOrder : string.Compare(left.name, right.name, StringComparison.OrdinalIgnoreCase);
            });
            return rows;
        }

        private static List<JObject> FindTasks(IDictionary<string, JObject> taskRecords, string studentUid, string placementId)
        {
            var result = new List<JObject>();
            if (taskRecords == null) return result;
            foreach (var task in taskRecords.Values)
            {
                if (task == null || !string.Equals(GetText(task, "studentUid"), studentUid, StringComparison.Ordinal)) continue;
                string taskPlacementId = GetText(task, "placementId");
                if (!string.IsNullOrWhiteSpace(taskPlacementId) && !string.Equals(taskPlacementId, placementId, StringComparison.Ordinal))
                    continue;
                result.Add(task);
            }
            return result;
        }

        private static JObject FindLatestSubmission(IDictionary<string, JObject> submissions, string taskId, string studentUid)
        {
            JObject latest = null;
            long latestSubmittedAt = long.MinValue;
            if (submissions == null || string.IsNullOrWhiteSpace(taskId)) return null;
            foreach (var submission in submissions.Values)
            {
                if (submission == null
                    || !string.Equals(GetText(submission, "studentUid"), studentUid, StringComparison.Ordinal)
                    || !string.Equals(GetText(submission, "taskId"), taskId, StringComparison.Ordinal)) continue;
                long submittedAt = ReadLong(submission, "submittedAtUnixMs");
                if (latest == null || submittedAt > latestSubmittedAt)
                {
                    latest = submission;
                    latestSubmittedAt = submittedAt;
                }
            }
            return latest;
        }

        private static JObject FindEvaluation(IDictionary<string, JObject> evaluations, string studentUid, string placementId)
        {
            if (evaluations == null) return null;
            foreach (var evaluation in evaluations.Values)
            {
                if (evaluation == null || !string.Equals(GetText(evaluation, "studentUid"), studentUid, StringComparison.Ordinal))
                    continue;
                string evaluationPlacementId = GetText(evaluation, "placementId");
                if (string.Equals(evaluationPlacementId, placementId, StringComparison.Ordinal))
                    return evaluation;
            }
            return evaluations.Values.FirstOrDefault(evaluation => evaluation != null
                && string.Equals(GetText(evaluation, "studentUid"), studentUid, StringComparison.Ordinal)
                && string.IsNullOrWhiteSpace(GetText(evaluation, "placementId")));
        }

        private static int SumApprovedHours(IDictionary<string, JObject> logs, string studentUid, string placementId)
        {
            if (logs == null) return 0;
            int total = 0;
            foreach (var log in logs.Values)
            {
                if (log == null || !string.Equals(GetText(log, "studentUid"), studentUid, StringComparison.Ordinal))
                    continue;
                string logPlacementId = GetText(log, "placementId");
                if (!string.IsNullOrWhiteSpace(logPlacementId) && !string.Equals(logPlacementId, placementId, StringComparison.Ordinal))
                    continue;
                if (ReadBool(log, "isApproved") || IsStatus(GetText(log, "status"), "approved"))
                    total += Math.Max(0, ReadInt(log, "hours"));
            }
            return total;
        }

        private static int StatusPriority(string status)
        {
            return IsStatus(status, "attention") ? 0 : (IsStatus(status, "active") ? 1 : 2);
        }

        private static bool IsCompleted(string status)
        {
            return IsStatus(status, "completed") || IsStatus(status, "complete") || IsStatus(status, "approved");
        }

        private static bool IsStatus(string value, string expected)
        {
            return string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetText(JObject source, string key)
        {
            var token = source == null ? null : source[key];
            return token == null || token.Type == JTokenType.Null ? string.Empty : token.ToString();
        }

        private static int ReadInt(JObject source, string key)
        {
            int value;
            return int.TryParse(GetText(source, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
        }

        private static long ReadLong(JObject source, string key)
        {
            long value;
            return long.TryParse(GetText(source, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0L;
        }

        private static bool ReadBool(JObject source, string key)
        {
            bool value;
            string text = GetText(source, key);
            return bool.TryParse(text, out value) && value;
        }

        private static string GetInitials(string name)
        {
            var parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "—";
            return parts.Length == 1 ? parts[0].Substring(0, 1).ToUpperInvariant()
                : (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static string FormatDate(long unixMilliseconds, string fallback)
        {
            if (unixMilliseconds <= 0) return string.IsNullOrWhiteSpace(fallback) ? "—" : fallback;
            try
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).ToLocalTime()
                    .ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            }
            catch (ArgumentOutOfRangeException)
            {
                return string.IsNullOrWhiteSpace(fallback) ? "—" : fallback;
            }
        }

        private sealed class MonitorPlacementRow
        {
            public string name { get; set; }
            public string id { get; set; }
            public string initials { get; set; }
            public string profileImageUrl { get; set; }
            public string role { get; set; }
            public string company { get; set; }
            public string city { get; set; }
            public string supervisor { get; set; }
            public int progress { get; set; }
            public string tasks { get; set; }
            public string evaluations { get; set; }
            public string hours { get; set; }
            public string status { get; set; }
            public string updated { get; set; }
        }
    }
}
