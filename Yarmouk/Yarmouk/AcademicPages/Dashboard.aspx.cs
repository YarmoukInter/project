using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;

namespace Yarmouk.AcademicPages
{
    public partial class Dashboard : SupervisorPageBase
    {
        private static readonly HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                RegisterAsyncTask(new PageAsyncTask(LoadDashboardAsync));
        }

        private async Task LoadDashboardAsync()
        {
            try
            {
                var auth = GetSupervisorSession("academic");

                // Read the Firebase sections used by this dashboard with the signed-in user's token.
                var placements = await GetRecordsAsync("placements", auth.IdToken);
                var students = await GetRecordsAsync("students", auth.IdToken);
                var users = await GetRecordsAsync("users", auth.IdToken);
                var companies = await GetRecordsAsync("companies", auth.IdToken);
                var tasks = await GetRecordsAsync("tasks", auth.IdToken);
                var submissions = await GetRecordsAsync("taskSubmissions", auth.IdToken);
                var trainingLogs = await GetRecordsAsync("trainingLogs", auth.IdToken);
                var evaluations = await GetRecordsAsync("evaluations", auth.IdToken);

                var assignedStudents = students
                    .Where(item => Same(Read(item.Value, "academicSupervisorId"), auth.Uid))
                    .ToList();
                var assignedStudentIds = new HashSet<string>(assignedStudents.Select(item => item.Key), StringComparer.Ordinal);
                var assignedPlacements = placements
                    .Where(item => assignedStudentIds.Contains(Read(item.Value, "studentUid")))
                    .ToList();

                var studentRows = assignedStudents.Select(student => {
                    var placement = assignedPlacements
                        .Where(item => Same(Read(item.Value, "studentUid"), student.Key))
                        .OrderByDescending(item => IsActivePlacement(Read(item.Value, "status")))
                        .FirstOrDefault();
                    if (placement.Value == null)
                        placement = new KeyValuePair<string, JObject>(string.Empty,
                            new JObject { ["studentUid"] = student.Key, ["status"] = "No placement" });
                    return BuildStudentRow(placement, students, users, companies, tasks, submissions, trainingLogs);
                }).OrderBy(row => row.StudentName, StringComparer.OrdinalIgnoreCase).ToList();

                int assignedStudentCount = studentRows.Count;
                int activePlacementCount = assignedPlacements.Count(item => IsActivePlacement(Read(item.Value, "status")));
                int submissionsToReview = CountSubmissionsToReview(submissions, tasks, assignedStudentIds);
                int evaluationsReady = CountEvaluationsReady(evaluations, assignedPlacements, assignedStudentIds);

                AssignedStudentsCount.Text = assignedStudentCount.ToString("N0");
                ActivePlacementsCount.Text = activePlacementCount.ToString("N0");
                SubmissionsToReviewCount.Text = submissionsToReview.ToString("N0");
                EvaluationsReadyCount.Text = evaluationsReady.ToString("N0");
                CaseloadCount.Text = assignedStudentCount.ToString("N0") + " students";

                AssignedStudents.DataSource = studentRows;
                AssignedStudents.DataBind();
                StudentsEmpty.Visible = studentRows.Count == 0;
                StudentTablePanel.Visible = studentRows.Count > 0;
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Your academic supervisor session could not be verified. Sign in again.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Academic dashboard load failed: " + error.GetType().Name);
                ShowError("Dashboard data could not be loaded from Firebase. Refresh the page or contact the coordinator.");
            }
        }

        private static async Task<Dictionary<string, JObject>> GetRecordsAsync(string path, string idToken)
        {
            string databaseUrl = Yarmouk.Helpers.SiteSettingsStore.GetFirebaseDatabaseUrl();
            if (string.IsNullOrWhiteSpace(databaseUrl)
                || !Uri.IsWellFormedUriString(databaseUrl, UriKind.Absolute)
                || new Uri(databaseUrl).Scheme != Uri.UriSchemeHttps)
                throw new ConfigurationErrorsException("Set FirebaseDatabaseUrl to a valid HTTPS URL in Web.config.");

            databaseUrl = databaseUrl.TrimEnd('/');
            if (databaseUrl.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                databaseUrl = databaseUrl.Substring(0, databaseUrl.Length - 5);

            string url = databaseUrl + "/" + path + ".json?auth=" + Uri.EscapeDataString(idToken);
            string json = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<Dictionary<string, JObject>>(json)
                ?? new Dictionary<string, JObject>(StringComparer.Ordinal);
        }

        private static SupervisorStudentProgress BuildStudentRow(
            KeyValuePair<string, JObject> placementEntry,
            IDictionary<string, JObject> students,
            IDictionary<string, JObject> users,
            IDictionary<string, JObject> companies,
            IDictionary<string, JObject> tasks,
            IDictionary<string, JObject> submissions,
            IDictionary<string, JObject> trainingLogs)
        {
            JObject placement = placementEntry.Value;
            string studentUid = Read(placement, "studentUid");
            JObject student = Find(students, studentUid);
            string companyId = Read(placement, "companyId", Read(student, "companyId"));
            JObject user = Find(users, studentUid);
            JObject company = Find(companies, companyId);
            var studentTasks = tasks.Where(item => Same(Read(item.Value, "placementId"), placementEntry.Key)
                    || (Same(Read(item.Value, "studentUid"), studentUid)
                        && (string.IsNullOrWhiteSpace(Read(item.Value, "placementId"))
                            || Same(Read(item.Value, "placementId"), placementEntry.Key))))
                .ToList();

            int completedTasks = studentTasks.Count(item => IsCompleted(Read(item.Value, "status")));
            int submittedTasks = TrainingProgress.CountSubmitted(tasks, submissions.Values, studentUid,
                placementEntry.Key, Read(placement, "opportunityId"));
            int approvedHours = ReadInt(placement, "approvedHours");
            if (approvedHours == 0)
            {
                approvedHours = trainingLogs
                    .Where(item => Same(Read(item.Value, "studentUid"), studentUid)
                        && (string.IsNullOrWhiteSpace(Read(item.Value, "placementId"))
                            || Same(Read(item.Value, "placementId"), placementEntry.Key))
                        && IsApproved(item.Value))
                    .Sum(item => ReadInt(item.Value, "hours"));
            }
            if (approvedHours == 0) approvedHours = ReadInt(student, "approvedTrainingHours");

            int requiredHours = ReadInt(placement, "requiredHours");
            if (requiredHours == 0) requiredHours = ReadInt(student, "requiredTrainingHours");
            int progressPercent = TrainingProgress.Percent(submittedTasks);
            string name = Read(user, "fullName", Read(student, "englishName", "Student"));

            return new SupervisorStudentProgress
            {
                PlacementId = placementEntry.Key,
                StudentUid = studentUid,
                StudentName = name,
                Initials = GetInitials(name),
                ProfileImageUrl = StudentProfileImages.GetUrl(Read(student, "profileImageFileName")),
                StudentNumber = Read(student, "studentId", Read(student, "studentNumber", "—")),
                CompanyName = Read(company, "name", string.IsNullOrWhiteSpace(companyId) ? "Company not assigned" : companyId),
                PlacementStatus = Read(placement, "status", "In progress"),
                ApprovedHours = approvedHours,
                RequiredHours = requiredHours,
                ProgressPercent = progressPercent,
                TotalTasks = studentTasks.Count,
                CompletedTasks = completedTasks,
                SubmittedTasks = submittedTasks,
                TaskProgressLabel = TrainingProgress.Label(submittedTasks),
                HoursProgressLabel = requiredHours > 0
                    ? approvedHours + " / " + requiredHours + " approved hours" : approvedHours + " approved hours"
            };
        }

        private static int CountSubmissionsToReview(IDictionary<string, JObject> submissions,
            IDictionary<string, JObject> tasks, ISet<string> assignedStudentIds)
        {
            return submissions.Count(item =>
            {
                JObject submission = item.Value;
                string studentUid = Read(submission, "studentUid");
                if (string.IsNullOrWhiteSpace(studentUid))
                {
                    JObject task = Find(tasks, Read(submission, "taskId"));
                    studentUid = Read(task, "studentUid");
                }
                return assignedStudentIds.Contains(studentUid) && IsPendingReview(submission);
            });
        }

        private static int CountStudentSubmissions(IDictionary<string, JObject> submissions,
            IDictionary<string, JObject> tasks, string studentUid, string placementId)
        {
            return submissions.Count(item =>
            {
                JObject submission = item.Value;
                JObject task = Find(tasks, Read(submission, "taskId"));
                string submissionStudent = Read(submission, "studentUid", Read(task, "studentUid"));
                string submissionPlacement = Read(submission, "placementId", Read(task, "placementId"));
                return Same(submissionStudent, studentUid)
                    && (string.IsNullOrWhiteSpace(submissionPlacement) || Same(submissionPlacement, placementId));
            });
        }

        private static int CountEvaluationsReady(IDictionary<string, JObject> evaluations,
            IEnumerable<KeyValuePair<string, JObject>> placements, ISet<string> assignedStudentIds)
        {
            int count = 0;
            foreach (var evaluationEntry in evaluations)
            {
                JObject evaluation = evaluationEntry.Value;
                if (!assignedStudentIds.Contains(Read(evaluation, "studentUid"))) continue;
                if (!IsCompanySubmitted(evaluation) || ReadBool(evaluation, "academicSubmitted")
                    || ReadBool(evaluation, "coordinatorApproved")) continue;

                string placementId = Read(evaluation, "placementId");
                if (placements.Any(placement => Same(placement.Key, placementId)
                    && Same(Read(placement.Value, "studentUid"), Read(evaluation, "studentUid"))))
                    count++;
            }
            return count;
        }

        private static bool IsPendingReview(JObject submission)
        {
            string status = Normalize(Read(submission, "reviewStatus", Read(submission, "status")));
            if (status == "pending" || status == "submitted" || status == "inreview"
                || status == "underreview" || status == "awaitingreview") return true;
            if (!string.IsNullOrWhiteSpace(status)) return false;
            return !string.IsNullOrWhiteSpace(Read(submission, "submissionUrl"))
                || !string.IsNullOrWhiteSpace(Read(submission, "submittedAt"));
        }

        private static bool IsCompanySubmitted(JObject evaluation)
        {
            if (evaluation["companySubmitted"] != null && evaluation["companySubmitted"].Type != JTokenType.Null)
                return ReadBool(evaluation, "companySubmitted");
            double score;
            return double.TryParse(Read(evaluation, "companyScore"), out score);
        }

        private static bool IsApproved(JObject record)
        {
            if (record["isApproved"] != null && record["isApproved"].Type != JTokenType.Null)
                return ReadBool(record, "isApproved");
            return Normalize(Read(record, "status")) == "approved";
        }

        private static bool IsCompleted(string status)
        {
            string normalized = Normalize(status);
            return normalized == "completed" || normalized == "complete" || normalized == "done" || normalized == "approved";
        }

        private static bool IsActivePlacement(string status)
        {
            string normalized = Normalize(status);
            return normalized == "inprogress" || normalized == "active" || normalized == "ongoing";
        }

        private static JObject Find(IDictionary<string, JObject> records, string key)
        {
            JObject value;
            return records != null && !string.IsNullOrWhiteSpace(key) && records.TryGetValue(key, out value) ? value : null;
        }

        private static string Read(JObject record, string key, string fallback = "")
        {
            if (record == null || record[key] == null || record[key].Type == JTokenType.Null) return fallback;
            string value = record[key].ToString();
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static int ReadInt(JObject record, string key)
        {
            int value;
            return int.TryParse(Read(record, key), out value) ? value : 0;
        }

        private static bool ReadBool(JObject record, string key)
        {
            bool value;
            return record != null && record[key] != null
                && (record[key].Type == JTokenType.Boolean
                    ? record[key].Value<bool>() : bool.TryParse(record[key].ToString(), out value) && value);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty
                : value.Trim().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static bool Same(string left, string right)
        {
            return !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right)
                && string.Equals(left, right, StringComparison.Ordinal);
        }

        private static string GetInitials(string name)
        {
            string[] parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "ST";
            return parts.Length == 1 ? parts[0].Substring(0, 1).ToUpperInvariant()
                : (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        protected string Html(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value) ?? string.Empty);
        }

        protected string PlacementBadgeClass(object value)
        {
            return IsActivePlacement(Convert.ToString(value)) ? "status-active" : "status-upcoming";
        }

        protected string PlacementStatusLabel(object value)
        {
            string status = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(status)) return "In progress";
            status = status.Replace("_", " ").Replace("-", " ").Trim();
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(status.ToLowerInvariant());
        }

        protected string TaskReviewLabel(object value)
        {
            int count;
            return int.TryParse(Convert.ToString(value), out count) && count > 0
                ? count + (count == 1 ? " submission" : " submissions") : "No submissions yet";
        }

        private void ShowError(string message)
        {
            DashboardErrorText.Text = HttpUtility.HtmlEncode(message);
            DashboardError.Visible = true;
            StudentsEmpty.Visible = false;
            StudentTablePanel.Visible = false;
            AssignedStudentsCount.Text = "—";
            ActivePlacementsCount.Text = "—";
            SubmissionsToReviewCount.Text = "—";
            EvaluationsReadyCount.Text = "—";
            CaseloadCount.Text = "—";
        }
    }
}
