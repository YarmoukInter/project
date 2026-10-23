using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class Dashboard : Page
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadDashboardAsync));
        }

        private async Task LoadDashboardAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null) { ShowNotice("Please sign in again to view your dashboard."); return; }

            try
            {
                var activeOpportunityId = await new StudentService().GetActiveAcceptedOpportunityIdAsync(auth.Uid, auth.IdToken);
                var appsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Applications, auth.IdToken);
                var tasksTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Tasks, auth.IdToken);
                var submissionsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TaskSubmissions, auth.IdToken);
                var studentTask = firebase.GetAsync<Student>(DatabasePaths.Students + "/" + auth.Uid, auth.IdToken);
                var opportunitiesTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Opportunities, auth.IdToken);
                var companiesTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Companies, auth.IdToken);
                var placementsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Placements, auth.IdToken);
                await Task.WhenAll(appsTask, tasksTask, submissionsTask, studentTask, opportunitiesTask, companiesTask, placementsTask);

                var apps = (await appsTask ?? new Dictionary<string, JObject>()).Where(x => Text(x.Value, "studentUid") == auth.Uid).ToList();
                var tasks = (await tasksTask ?? new Dictionary<string, JObject>()).Where(x =>
                    Text(x.Value, "studentUid") == auth.Uid && !string.IsNullOrEmpty(activeOpportunityId)
                    && Text(x.Value, "opportunityId") == activeOpportunityId).ToList();
                var visibleTaskIds = new HashSet<string>(tasks.Select(x => Text(x.Value, "id", x.Key)), StringComparer.Ordinal);
                var submissions = (await submissionsTask ?? new Dictionary<string, JObject>()).Where(x => Text(x.Value, "studentUid") == auth.Uid
                    && visibleTaskIds.Contains(Text(x.Value, "taskId")))
                    .GroupBy(x => Text(x.Value, "taskId")).Where(x => x.Key != "")
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(y => Number(y.Value, "submittedAtUnixMs")).First().Value);
                var student = await studentTask;
                var opportunities = await opportunitiesTask ?? new Dictionary<string, JObject>();
                var companies = await companiesTask ?? new Dictionary<string, JObject>();
                var placements = await placementsTask ?? new Dictionary<string, JObject>();

                var taskRows = tasks.Select(x => new { x.Key, Data = x.Value, Submission = GetSubmission(submissions, Text(x.Value, "id", x.Key)) }).ToList();
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                long weekEnd = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeMilliseconds();
                var upcoming = taskRows.Where(x => Number(x.Data, "dueAtUnixMs") >= now
                        && Number(x.Data, "dueAtUnixMs") <= weekEnd
                        && TaskStatus(x.Data, x.Submission) != "Completed")
                    .OrderBy(x => Number(x.Data, "dueAtUnixMs")).ToList();
                var pendingTasks = taskRows.Where(x => TaskStatus(x.Data, x.Submission) != "Completed")
                    .OrderBy(x => Number(x.Data, "dueAtUnixMs") <= 0 ? long.MaxValue : Number(x.Data, "dueAtUnixMs")).ToList();
                var displayedTasks = (pendingTasks.Count > 0 ? pendingTasks : taskRows
                    .OrderByDescending(x => Number(x.Data, "dueAtUnixMs")).ToList()).Take(4).ToList();
                int overdue = pendingTasks.Count(x => Number(x.Data, "dueAtUnixMs") > 0 && Number(x.Data, "dueAtUnixMs") < now);
                int completed = taskRows.Count(x => TaskStatus(x.Data, x.Submission) == "Completed");
                int reviewed = submissions.Values.Count(IsReviewed);

                var studentName = auth.User == null ? "Student" : auth.User.FullName;
                StudentName.Text = HttpUtility.HtmlEncode(studentName);
                StudentInitials.Text = HttpUtility.HtmlEncode(Initials(studentName));
                ApplicationsCount.Text = apps.Count.ToString(CultureInfo.InvariantCulture);
                ApplicationsSummary.Text = CountStatus(apps, "pending") + " pending · " + CountStatus(apps, "accepted") + " accepted · " + CountStatus(apps, "rejected") + " rejected";
                TasksCount.Text = taskRows.Count.ToString(CultureInfo.InvariantCulture);
                TasksSummary.Text = completed + " completed · " + (taskRows.Count - completed) + " active";
                DeadlinesCount.Text = upcoming.Count.ToString(CultureInfo.InvariantCulture);
                DeadlinesMessage.Text = HttpUtility.HtmlEncode((overdue > 0 ? overdue + (overdue == 1 ? " overdue task · " : " overdue tasks · ") : "")
                    + upcoming.Count + (upcoming.Count == 1 ? " task due this week" : " tasks due this week"));
                EvaluationsCount.Text = reviewed.ToString(CultureInfo.InvariantCulture);

                UpcomingTasks.DataSource = displayedTasks.Select(x => new
                {
                    Title = Text(x.Data, "title", "Task"), Description = Text(x.Data, "description", "Training task"),
                    Deadline = FormatDate(Number(x.Data, "dueAtUnixMs")), DeadlineIso = FormatIso(Number(x.Data, "dueAtUnixMs")),
                    DeadlineNote = Number(x.Data, "dueAtUnixMs") > 0 && Number(x.Data, "dueAtUnixMs") < now
                        && TaskStatus(x.Data, x.Submission) != "Completed" ? "Overdue" : "",
                    Status = TaskStatus(x.Data, x.Submission), BadgeClass = BadgeClass(TaskStatus(x.Data, x.Submission))
                }).ToList();
                UpcomingTasks.DataBind();
                NoUpcomingTasks.Visible = taskRows.Count == 0;

                var target = student == null ? 0 : Math.Max(0, student.RequiredTrainingHours);
                var hours = student == null ? 0 : Math.Max(0, student.ApprovedTrainingHours);
                int percent = target == 0 ? 0 : Math.Min(100, hours * 100 / target);
                ApprovedHours.Text = hours.ToString("N0", CultureInfo.InvariantCulture);
                RequiredHours.Text = target == 0 ? "—" : target.ToString("N0", CultureInfo.InvariantCulture);
                DashboardProgressPercent.Text = target == 0 ? "—" : percent.ToString(CultureInfo.InvariantCulture) + "%";
                DashboardProgressRing.Style["--progress"] = percent + "%";
                DashboardProgressRing.Attributes["aria-label"] = target == 0 ? "Training target unavailable" : "Internship progress: " + percent + "%";
                TaskCompletionLabel.Text = completed + " of " + taskRows.Count + " tasks";
                DashboardTaskProgress.Attributes["value"] = completed.ToString(CultureInfo.InvariantCulture);
                DashboardTaskProgress.Attributes["max"] = Math.Max(1, taskRows.Count).ToString(CultureInfo.InvariantCulture);
                DashboardTaskProgressPercent.Text = taskRows.Count == 0 ? "0" : (completed * 100 / taskRows.Count).ToString(CultureInfo.InvariantCulture);
                InternshipStatus.Text = HttpUtility.HtmlEncode(student == null || string.IsNullOrWhiteSpace(student.InternshipStatus)
                    ? "No active training" : Humanize(student.InternshipStatus));

                BindApplications(apps, opportunities, companies);
                BindReviews(submissions, tasks);
                await BindPlacementAsync(placements, student, companies, auth.Uid, auth.IdToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Student dashboard load failed: " + ex.GetType().Name);
                ShowNotice("Dashboard data could not be loaded. Please refresh the page and try again.");
            }
        }

        private void BindApplications(List<KeyValuePair<string, JObject>> apps, Dictionary<string, JObject> opportunities, Dictionary<string, JObject> companies)
        {
            var rows = apps.OrderByDescending(x => Number(x.Value, "appliedAtUnixMs")).Take(3).Select(x =>
            {
                var opportunity = Find(opportunities, Text(x.Value, "opportunityId"));
                var company = Find(companies, Text(x.Value, "companyId", Text(opportunity, "companyId")));
                var status = Humanize(Text(x.Value, "status", "Pending"));
                return new { Company = Text(company, "name", "Company"), Opportunity = Text(opportunity, "title", "Opportunity"), Status = status, BadgeClass = BadgeClass(status) };
            }).ToList();
            RecentApplications.DataSource = rows;
            RecentApplications.DataBind();
            NoRecentApplications.Visible = rows.Count == 0;
        }

        private void BindReviews(Dictionary<string, JObject> submissions, List<KeyValuePair<string, JObject>> tasks)
        {
            var rows = submissions.OrderByDescending(x => Number(x.Value, "submittedAtUnixMs")).Select(x =>
            {
                var task = tasks.FirstOrDefault(t => Text(t.Value, "id", t.Key) == x.Key).Value;
                var status = Text(x.Value, "reviewStatus", Text(x.Value, "status", "Reviewed"));
                return new { Task = Text(task, "title", "Task"), Date = FormatDate(Number(x.Value, "submittedAtUnixMs")),
                    Feedback = Text(x.Value, "feedback", Text(x.Value, "reviewFeedback", Text(x.Value, "companyFeedback", "No feedback provided."))),
                    Status = Humanize(status), BadgeClass = BadgeClass(status), Reviewed = IsReviewed(x.Value) };
            }).Where(x => x.Reviewed).Take(3).ToList();
            RecentReviews.DataSource = rows;
            RecentReviews.DataBind();
            NoRecentReviews.Visible = rows.Count == 0;
        }

        private async Task BindPlacementAsync(Dictionary<string, JObject> placements, Student student,
            Dictionary<string, JObject> companies, string uid, string idToken)
        {
            var placement = placements.FirstOrDefault(x => Text(x.Value, "studentUid") == uid
                && !new[] { "pending", "rejected", "declined", "cancelled" }.Contains(Normalize(Text(x.Value, "status"))));
            if (placement.Value == null) return;
            var company = Find(companies, Text(placement.Value, "companyId", student == null ? "" : student.CompanyId));
            PlacementCompany.Text = HttpUtility.HtmlEncode(Text(company, "name", "Confirmed placement"));
            string companyUid = Text(placement.Value, "companySupervisorUid", Text(company, "supervisorUid"));
            string academicUid = Text(placement.Value, "academicSupervisorUid", Text(placement.Value, "academicSupervisorId",
                student == null ? "" : student.AcademicSupervisorId));
            var companyUserTask = GetSupervisorUserAsync(companyUid, idToken);
            var academicUserTask = GetSupervisorUserAsync(academicUid, idToken);
            await Task.WhenAll(companyUserTask, academicUserTask);
            CompanySupervisor.Text = SupervisorDetails(await companyUserTask,
                Text(placement.Value, "companySupervisorName"), companyUid);
            AcademicSupervisor.Text = SupervisorDetails(await academicUserTask,
                student == null ? "" : student.AcademicSupervisorName, academicUid);
        }

        private async Task<JObject> GetSupervisorUserAsync(string uid, string idToken)
        {
            if (string.IsNullOrWhiteSpace(uid)) return null;
            try { return await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + uid, idToken); }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Student dashboard supervisor lookup failed: " + ex.GetType().Name);
                return null;
            }
        }

        private static string SupervisorDetails(JObject user, string fallbackName, string uid)
        {
            string name = Text(user, "fullName", fallbackName);
            if (string.IsNullOrWhiteSpace(name)) name = string.IsNullOrWhiteSpace(uid) ? "Not assigned" : "Supervisor details unavailable";
            string email = Text(user, "email");
            string phone = Text(user, "phone");
            string details = "<strong class=\"supervisor-name\">" + HttpUtility.HtmlEncode(name) + "</strong>";
            if (!string.IsNullOrWhiteSpace(email)) details += "<span class=\"supervisor-contact\">" + HttpUtility.HtmlEncode(email) + "</span>";
            if (!string.IsNullOrWhiteSpace(phone)) details += "<span class=\"supervisor-contact\">" + HttpUtility.HtmlEncode(phone) + "</span>";
            return details;
        }

        private static JObject GetSubmission(Dictionary<string, JObject> submissions, string taskId)
        {
            JObject value;
            return submissions.TryGetValue(taskId, out value) ? value : null;
        }
        private static JObject Find(Dictionary<string, JObject> values, string key)
        {
            JObject value;
            return values != null && values.TryGetValue(key ?? "", out value) ? value : null;
        }
        private static string Text(JToken value, string key, string fallback = "")
        {
            var text = value == null || value[key] == null ? "" : value[key].ToString();
            return string.IsNullOrWhiteSpace(text) ? fallback : text;
        }
        private static long Number(JToken value, string key)
        {
            long result;
            return value != null && value[key] != null && long.TryParse(value[key].ToString(), out result) ? result : 0;
        }
        private static int CountStatus(List<KeyValuePair<string, JObject>> items, string status)
        {
            return items.Count(x => Normalize(Text(x.Value, "status")) == status);
        }
        private static string TaskStatus(JObject task, JObject submission)
        {
            var status = Normalize(submission == null ? Text(task, "status") : Text(submission, "reviewStatus", Text(submission, "status", Text(task, "status"))));
            if (submission == null && (status == "pending" || status == "assigned" || status == "notsubmitted")) return "Not started";
            if (status == "completed" || status == "complete" || status == "done" || status == "approved") return "Completed";
            if (status == "revisionrequested" || status == "needsrevision" || status == "resubmit") return "Revision requested";
            if (status == "submitted" || status == "pendingreview" || status == "underreview" || status == "inreview" || status == "pending") return "Under review";
            if (status == "inprogress" || status == "started") return "In progress";
            return "Not started";
        }
        private static bool IsReviewed(JObject submission)
        {
            var status = Normalize(Text(submission, "reviewStatus", Text(submission, "status")));
            return status == "reviewed" || status == "revisionrequested" || status == "needsrevision" || status == "resubmit"
                || Text(submission, "feedback", Text(submission, "reviewFeedback", Text(submission, "companyFeedback"))) != "";
        }
        private static string BadgeClass(string status)
        {
            var value = Normalize(status);
            if (value == "accepted" || value == "confirmed" || value == "completed" || value == "reviewed" || value == "approved") return "badge-success";
            if (value == "rejected" || value == "revisionrequested" || value == "needsrevision" || value == "resubmit") return "badge-danger";
            if (value == "pending" || value == "underreview" || value == "inprogress" || value == "submitted") return "badge-warning";
            return "badge-neutral";
        }
        private static string Normalize(string value) { return (value ?? "").Trim().Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant(); }
        private static string Humanize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Pending";
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Replace("_", " ").Replace("-", " ").ToLowerInvariant());
        }
        private static string FormatDate(long value)
        {
            try { return value <= 0 ? "—" : DateTimeOffset.FromUnixTimeMilliseconds(value).LocalDateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture); }
            catch (ArgumentOutOfRangeException) { return "—"; }
        }
        private static string FormatIso(long value)
        {
            try { return value <= 0 ? "" : DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
            catch (ArgumentOutOfRangeException) { return ""; }
        }
        private static string Initials(string name)
        {
            var parts = (name ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? "ST" : parts.Length == 1 ? parts[0].Substring(0, 1).ToUpperInvariant()
                : (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }
        private void ShowNotice(string message)
        {
            DashboardNoticeText.Text = HttpUtility.HtmlEncode(message);
            DashboardNotice.Visible = true;
        }
    }
}
