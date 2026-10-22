using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.CompanyPages
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
                var userTask = firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + auth.Uid, auth.IdToken);
                var companiesTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Companies, auth.IdToken);
                var opportunitiesTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Opportunities, auth.IdToken);
                var applicationsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Applications, auth.IdToken);
                var placementsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Placements, auth.IdToken);
                var tasksTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Tasks, auth.IdToken);
                var submissionsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TaskSubmissions, auth.IdToken);
                var evaluationsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Evaluations, auth.IdToken);
                var studentsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Students, auth.IdToken);
                var usersTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Users, auth.IdToken);
                var majorsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Majors, auth.IdToken);
                await Task.WhenAll(userTask, companiesTask, opportunitiesTask, applicationsTask, placementsTask,
                    tasksTask, submissionsTask, evaluationsTask, studentsTask, usersTask, majorsTask);

                var user = await userTask;
                var companies = await companiesTask ?? new Dictionary<string, JObject>();
                string companyId = Text(user, "companyId");
                if (string.IsNullOrWhiteSpace(companyId))
                    companyId = companies.Where(x => IsSupervisor(x.Value, auth.Uid)).Select(x => x.Key).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(companyId) || !companies.ContainsKey(companyId))
                { ShowNotice("Your company is not linked to this supervisor account yet."); return; }

                var companyOpportunities = (await opportunitiesTask ?? new Dictionary<string, JObject>())
                    .Where(x => Same(Text(x.Value, "companyId"), companyId)).ToList();
                var opportunityIds = new HashSet<string>(companyOpportunities.Select(x => x.Key), StringComparer.OrdinalIgnoreCase);
                var activeOpportunities = companyOpportunities.Where(x => IsActive(x.Value)).ToList();
                var applications = (await applicationsTask ?? new Dictionary<string, JObject>())
                    .Where(x => Same(Text(x.Value, "companyId"), companyId) || opportunityIds.Contains(Text(x.Value, "opportunityId"))).ToList();
                var placements = (await placementsTask ?? new Dictionary<string, JObject>())
                    .Where(x => Same(Text(x.Value, "companyId"), companyId) || Same(Text(x.Value, "companySupervisorUid"), auth.Uid))
                    .Where(x => IsPlacementActive(x.Value)).ToList();
                var placementIds = new HashSet<string>(placements.Select(x => x.Key), StringComparer.OrdinalIgnoreCase);
                var studentIds = new HashSet<string>(placements.Select(x => Text(x.Value, "studentUid")), StringComparer.OrdinalIgnoreCase);
                var students = await studentsTask ?? new Dictionary<string, JObject>();
                var users = await usersTask ?? new Dictionary<string, JObject>();
                var majors = await majorsTask ?? new Dictionary<string, JObject>();
                var tasks = (await tasksTask ?? new Dictionary<string, JObject>()).Where(x =>
                    Same(Text(x.Value, "supervisorUid"), auth.Uid) || studentIds.Contains(Text(x.Value, "studentUid"))
                    || placementIds.Contains(Text(x.Value, "placementId"))).ToList();
                var taskIds = new HashSet<string>(tasks.Select(x => Text(x.Value, "id", x.Key)), StringComparer.OrdinalIgnoreCase);
                var submissions = (await submissionsTask ?? new Dictionary<string, JObject>()).Where(x =>
                    taskIds.Contains(Text(x.Value, "taskId")) || studentIds.Contains(Text(x.Value, "studentUid"))
                    && (string.IsNullOrWhiteSpace(Text(x.Value, "placementId")) || placementIds.Contains(Text(x.Value, "placementId")))).ToList();
                var evaluations = (await evaluationsTask ?? new Dictionary<string, JObject>()).Where(x =>
                    placementIds.Contains(Text(x.Value, "placementId")) || Same(Text(x.Value, "companySupervisorUid"), auth.Uid)).ToList();

                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var week = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeMilliseconds();
                var openTasks = tasks.Where(x => !IsTaskComplete(x.Value)).ToList();
                var dueThisWeek = openTasks.Count(x => Number(x.Value, "dueAtUnixMs") >= now && Number(x.Value, "dueAtUnixMs") <= week);
                var pendingApps = applications.Where(x => IsPending(Text(x.Value, "status"))).ToList();
                var pendingSubmissions = submissions.Where(x => IsPendingReview(x.Value)).ToList();
                var dueEvaluations = evaluations.Where(x => !Bool(x.Value, "companySubmitted") && !Bool(x.Value, "coordinatorApproved")).ToList();

                ActiveOpportunitiesCount.Text = activeOpportunities.Count.ToString(CultureInfo.InvariantCulture);
                AllOpportunitiesCount.Text = companyOpportunities.Count.ToString(CultureInfo.InvariantCulture);
                NewApplicationsCount.Text = applications.Count.ToString(CultureInfo.InvariantCulture);
                AwaitingApplicationsCount.Text = pendingApps.Count.ToString(CultureInfo.InvariantCulture);
                CurrentTraineesCount.Text = placements.Select(x => Text(x.Value, "studentUid")).Where(x => x != "").Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(CultureInfo.InvariantCulture);
                TraineeOpportunitiesCount.Text = placements.Select(x => Text(x.Value, "opportunityId")).Where(x => x != "").Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(CultureInfo.InvariantCulture);
                OpenTasksCount.Text = openTasks.Count.ToString(CultureInfo.InvariantCulture);
                TasksDueThisWeekCount.Text = dueThisWeek.ToString(CultureInfo.InvariantCulture);
                SubmissionsToReviewCount.Text = pendingSubmissions.Count.ToString(CultureInfo.InvariantCulture);
                SubmissionTraineesCount.Text = pendingSubmissions.Select(x => Text(x.Value, "studentUid")).Where(x => x != "").Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(CultureInfo.InvariantCulture);
                EvaluationsDueCount.Text = dueEvaluations.Count.ToString(CultureInfo.InvariantCulture);
                ApplicationsTabCount.Text = pendingApps.Count.ToString(CultureInfo.InvariantCulture);
                SubmissionsTabCount.Text = pendingSubmissions.Count.ToString(CultureInfo.InvariantCulture);
                EvaluationSummary.Text = dueEvaluations.Count.ToString(CultureInfo.InvariantCulture);
                EvaluationDueCaption.Text = dueEvaluations.Count.ToString(CultureInfo.InvariantCulture);
                PreviewNotice.Visible = false;

                ApplicationsRepeater.DataSource = pendingApps.OrderByDescending(x => Number(x.Value, "appliedAtUnixMs"))
                    .Take(5).Select(x => BuildApplicationRow(x, companyOpportunities, users, students, majors)).ToList();
                ApplicationsRepeater.DataBind();
                SubmissionsRepeater.DataSource = pendingSubmissions.OrderByDescending(x => Number(x.Value, "submittedAtUnixMs"))
                    .Take(5).Select(x => BuildSubmissionRow(x, tasks, users)).ToList();
                SubmissionsRepeater.DataBind();
                OpportunitiesRepeater.DataSource = companyOpportunities.OrderByDescending(x => IsActive(x.Value))
                    .Take(4).Select(x => BuildOpportunityRow(x, applications)).ToList();
                OpportunitiesRepeater.DataBind();
                TraineesRepeater.DataSource = placements.GroupBy(x => Text(x.Value, "studentUid"), StringComparer.OrdinalIgnoreCase)
                    .Where(x => x.Key != "").Take(5).Select(x => BuildTraineeRow(x.First(), tasks, users, students, majors)).ToList();
                TraineesRepeater.DataBind();
                NoApplications.Visible = pendingApps.Count == 0;
                NoSubmissions.Visible = pendingSubmissions.Count == 0;
                NoOpportunities.Visible = companyOpportunities.Count == 0;
                NoTrainees.Visible = placements.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Company dashboard load failed: " + ex.GetType().Name);
                ShowNotice("Dashboard data could not be loaded. Check Firebase access and refresh the page.");
            }
        }

        private static object BuildApplicationRow(KeyValuePair<string, JObject> item, List<KeyValuePair<string, JObject>> opportunities,
            Dictionary<string, JObject> users, Dictionary<string, JObject> students, Dictionary<string, JObject> majors)
        {
            string uid = Text(item.Value, "studentUid");
            var user = Find(users, uid); var student = Find(students, uid); var opportunity = opportunities.FirstOrDefault(x => x.Key == Text(item.Value, "opportunityId")).Value;
            var major = Find(majors, Text(student, "majorId"));
            return new { Name = Text(user, "fullName", "Applicant"), Major = Text(major, "name", Text(student, "majorId", "Student")),
                Opportunity = Text(opportunity, "title", "Opportunity"), Applied = FormatDate(Number(item.Value, "appliedAtUnixMs")), Initials = Initials(Text(user, "fullName", "A")) };
        }
        private static object BuildSubmissionRow(KeyValuePair<string, JObject> item, List<KeyValuePair<string, JObject>> tasks, Dictionary<string, JObject> users)
        {
            string taskId = Text(item.Value, "taskId"); var task = tasks.FirstOrDefault(x => Text(x.Value, "id", x.Key) == taskId).Value;
            var user = Find(users, Text(item.Value, "studentUid"));
            return new { Name = Text(user, "fullName", "Trainee"), Task = Text(task, "title", "Task"), Submitted = FormatDate(Number(item.Value, "submittedAtUnixMs")), Initials = Initials(Text(user, "fullName", "T")) };
        }
        private static object BuildOpportunityRow(KeyValuePair<string, JObject> item, List<KeyValuePair<string, JObject>> applications)
        {
            int applicants = applications.Count(x => Text(x.Value, "opportunityId") == item.Key);
            int seats = Math.Max(0, ReadInt(item.Value, "availablePositions"));
            int fill = applicants + seats == 0 ? 0 : Math.Min(100, applicants * 100 / (applicants + seats));
            return new { Title = Text(item.Value, "title", "Opportunity"), Applicants = applicants, Seats = seats, Fill = fill,
                State = IsActive(item.Value) ? "Active" : "Closed" };
        }
        private static object BuildTraineeRow(KeyValuePair<string, JObject> placement, List<KeyValuePair<string, JObject>> tasks,
            Dictionary<string, JObject> users, Dictionary<string, JObject> students, Dictionary<string, JObject> majors)
        {
            string uid = Text(placement.Value, "studentUid"); var user = Find(users, uid); var student = Find(students, uid);
            var major = Find(majors, Text(student, "majorId"));
            var traineeTasks = tasks.Where(x => Text(x.Value, "studentUid") == uid && (Text(x.Value, "placementId") == "" || Text(x.Value, "placementId") == placement.Key)).ToList();
            int completed = traineeTasks.Count(x => IsTaskComplete(x.Value)); int total = traineeTasks.Count;
            return new { Name = Text(user, "fullName", "Trainee"), Program = Text(major, "name", "Active trainee"), Initials = Initials(Text(user, "fullName", "T")),
                Completed = completed, Total = total, Percent = total == 0 ? 0 : completed * 100 / total };
        }
        private static bool IsSupervisor(JObject company, string uid)
        {
            return Same(Text(company, "supervisorUid"), uid) || Bool(company == null ? null : company["supervisorUids"] as JObject, uid);
        }
        private static bool IsActive(JObject item)
        {
            bool active; if (bool.TryParse(Text(item, "isActive"), out active)) return active;
            return Normalize(Text(item, "status")) == "active" || Normalize(Text(item, "status")) == "open";
        }
        private static bool IsPlacementActive(JObject item)
        {
            string status = Normalize(Text(item, "status"));
            return status != "completed" && status != "complete" && status != "ended" && status != "cancelled" && status != "rejected" && status != "pending";
        }
        private static bool IsPending(string status) { return Normalize(status) == "pending" || Normalize(status) == "submitted" || Normalize(status) == "underreview"; }
        private static bool IsPendingReview(JObject submission)
        {
            string status = Normalize(Text(submission, "reviewStatus", Text(submission, "status")));
            return status == "pending" || status == "submitted" || status == "pendingreview" || status == "underreview" || status == "inreview";
        }
        private static bool IsTaskComplete(JObject task)
        {
            string status = Normalize(Text(task, "status")); return status == "completed" || status == "complete" || status == "done" || status == "approved";
        }
        private static bool Bool(JObject value, string key)
        {
            bool result; return value != null && bool.TryParse(Text(value, key), out result) && result;
        }
        private static JObject Find(Dictionary<string, JObject> map, string key) { JObject value; return map != null && map.TryGetValue(key ?? "", out value) ? value : null; }
        private static string Text(JToken value, string key, string fallback = "")
        { string text = value == null || value[key] == null ? "" : value[key].ToString(); return string.IsNullOrWhiteSpace(text) ? fallback : text; }
        private static long Number(JObject value, string key) { long number; return long.TryParse(Text(value, key), out number) ? number : 0; }
        private static int ReadInt(JObject value, string key) { int number; return int.TryParse(Text(value, key), out number) ? number : 0; }
        private static bool Same(string a, string b) { return string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase); }
        private static string Normalize(string value) { return (value ?? "").Trim().Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant(); }
        private static string FormatDate(long value)
        { try { return value <= 0 ? "—" : DateTimeOffset.FromUnixTimeMilliseconds(value).LocalDateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture); } catch (ArgumentOutOfRangeException) { return "—"; } }
        private static string Initials(string name)
        { var parts = (name ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); return parts.Length > 1 ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant() : parts.Length == 1 ? parts[0][0].ToString().ToUpperInvariant() : "T"; }
        private void ShowNotice(string message) { DashboardNoticeText.Text = HttpUtility.HtmlEncode(message); DashboardNotice.Visible = true; }
    }
}
