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
    public partial class Progress : Page
    {
        private const int StageCount = 8;
        private readonly FirebaseHelper firebase = new FirebaseHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadProgressAsync));
        }

        private async Task LoadProgressAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null) { ShowNotice("Please sign in again to view your progress."); return; }

            try
            {
                var activeOpportunityId = await new StudentService().GetActiveAcceptedOpportunityIdAsync(auth.Uid, auth.IdToken);
                var studentTask = firebase.GetAsync<Student>(DatabasePaths.Students + "/" + auth.Uid, auth.IdToken);
                var tasksTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Tasks, auth.IdToken);
                var submissionsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TaskSubmissions, auth.IdToken);
                var logsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TrainingLogs, auth.IdToken);
                await Task.WhenAll(studentTask, tasksTask, submissionsTask, logsTask);

                var student = await studentTask;
                var tasks = (await tasksTask ?? new Dictionary<string, JObject>())
                    .Where(x => Text(x.Value, "studentUid") == auth.Uid && !string.IsNullOrEmpty(activeOpportunityId)
                        && Text(x.Value, "opportunityId") == activeOpportunityId).ToList();
                var visibleTaskIds = new HashSet<string>(tasks.Select(x => Text(x.Value, "id", x.Key)), StringComparer.Ordinal);
                var submissions = (await submissionsTask ?? new Dictionary<string, JObject>())
                    .Where(x => Text(x.Value, "studentUid") == auth.Uid && visibleTaskIds.Contains(Text(x.Value, "taskId")))
                    .GroupBy(x => Text(x.Value, "taskId")).Where(x => x.Key != "")
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(y => Number(y.Value, "submittedAtUnixMs")).First().Value);
                var logs = (await logsTask ?? new Dictionary<string, JObject>()).Values
                    .Where(x => Text(x, "studentUid") == auth.Uid).ToList();

                int required = student == null ? 0 : Math.Max(0, student.RequiredTrainingHours);
                int approved = logs.Count == 0 && student != null ? Math.Max(0, student.ApprovedTrainingHours)
                    : logs.Where(IsApproved).Sum(x => Math.Max(0, ReadInt(x, "hours")));
                int percent = required == 0 ? 0 : Math.Min(100, approved * 100 / required);

                ApprovedHours.Text = approved.ToString("N0", CultureInfo.InvariantCulture);
                RequiredHours.Text = required == 0 ? "—" : required.ToString("N0", CultureInfo.InvariantCulture);
                TargetHours.Text = RequiredHours.Text;
                TrackTarget.Text = required == 0 ? "—" : required.ToString("N0", CultureInfo.InvariantCulture);
                PlanRequiredHours.Text = RequiredHours.Text;
                PlanApprovedHours.Text = approved.ToString("N0", CultureInfo.InvariantCulture) + " hours";
                CompletionPercent.Text = required == 0 ? "—" : percent.ToString(CultureInfo.InvariantCulture);
                progressRing.Style["--progress"] = percent + "%";
                progressRing.Attributes["aria-valuenow"] = percent.ToString(CultureInfo.InvariantCulture);
                hoursProgressBar.Attributes["aria-valuemax"] = Math.Max(0, required).ToString(CultureInfo.InvariantCulture);
                hoursTrack.Style["width"] = percent + "%";

                var rows = tasks.OrderBy(x => Number(x.Value, "dueAtUnixMs")).Select(x =>
                {
                    var taskId = Text(x.Value, "id", x.Key);
                    JObject submission;
                    submissions.TryGetValue(taskId, out submission);
                    var status = TaskStatus(x.Value, submission);
                    var grade = Text(submission, "grade", Text(submission, "score"));
                    return new
                    {
                        Title = Text(x.Value, "title", "Task"), Status = status,
                        Grade = string.IsNullOrWhiteSpace(grade) ? "" : " · Grade: " + grade,
                        Feedback = Text(submission, "feedback", Text(submission, "reviewFeedback", Text(submission, "companyFeedback", "No feedback provided yet."))),
                        Submitted = submission != null || status == "Under review" || status == "Completed" || status == "Revision requested",
                        Revision = Normalize(status) == "revisionrequested"
                    };
                }).ToList();

                int submitted = rows.Count(x => x.Submitted);
                int revisions = rows.Count(x => x.Revision);
                TaskSummary.Text = rows.Count + " tasks · " + submitted + " submitted · " + revisions + " revisions requested";
                TaskUpdates.DataSource = rows.Take(8).ToList();
                TaskUpdates.DataBind();
                NoTaskUpdates.Visible = rows.Count == 0;
                MilestoneRepeater.DataSource = BuildMilestones(approved, required);
                MilestoneRepeater.DataBind();

                if (student == null) ShowNotice("Your student training record has not been created yet.");
                else if (required == 0) ShowNotice("Your required training hours have not been set yet.");
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Student progress load failed: " + error.GetType().Name);
                CompletionPercent.Text = "—";
                ApprovedHours.Text = "—";
                RequiredHours.Text = "—";
                ShowNotice("Progress data could not be loaded. Please refresh the page and try again.");
            }
        }

        private static List<ProgressMilestone> BuildMilestones(int approved, int required)
        {
            var result = new List<ProgressMilestone>();
            if (required <= 0) return result;
            for (int i = 1; i <= StageCount; i++)
            {
                int start = (required * (i - 1) + StageCount - 1) / StageCount;
                int end = (required * i + StageCount - 1) / StageCount;
                int target = end - start;
                int hours = Math.Min(target, Math.Max(0, approved - start));
                bool complete = approved >= end;
                result.Add(new ProgressMilestone
                {
                    Week = i.ToString("00", CultureInfo.InvariantCulture),
                    HoursLabel = hours.ToString(CultureInfo.InvariantCulture),
                    TargetHours = target.ToString(CultureInfo.InvariantCulture),
                    Fill = target == 0 ? "0" : (hours * 100 / target).ToString(CultureInfo.InvariantCulture),
                    CssClass = complete ? "is-complete" : (hours > 0 ? "is-current" : "is-upcoming"),
                    Marker = complete ? "✓" : i.ToString(CultureInfo.InvariantCulture)
                });
            }
            return result;
        }

        private static bool IsApproved(JObject log)
        {
            bool approved;
            if (bool.TryParse(Text(log, "isApproved"), out approved)) return approved;
            return Normalize(Text(log, "status")) == "approved";
        }
        private static string TaskStatus(JObject task, JObject submission)
        {
            var status = Normalize(submission == null ? Text(task, "status") : Text(submission, "reviewStatus", Text(submission, "status", Text(task, "status"))));
            if (submission == null && (status == "pending" || status == "assigned" || status == "notsubmitted")) return "Not started";
            if (status == "completed" || status == "complete" || status == "done" || status == "approved") return "Completed";
            if (status == "revisionrequested" || status == "needsrevision" || status == "resubmit") return "Revision requested";
            if (status == "submitted" || status == "underreview" || status == "inreview" || status == "pendingreview" || status == "pending") return "Under review";
            if (status == "inprogress" || status == "started") return "In progress";
            return "Not started";
        }
        private static string Text(JToken value, string key, string fallback = "")
        {
            var text = value == null || value[key] == null ? "" : value[key].ToString();
            return string.IsNullOrWhiteSpace(text) ? fallback : text;
        }
        private static int ReadInt(JObject value, string key)
        {
            int number;
            return int.TryParse(Text(value, key), out number) ? number : 0;
        }
        private static long Number(JObject value, string key)
        {
            long number;
            return long.TryParse(Text(value, key), out number) ? number : 0;
        }
        private static string Normalize(string value) { return (value ?? "").Trim().Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant(); }
        private void ShowNotice(string message)
        {
            NoticeText.Text = HttpUtility.HtmlEncode(message);
            ProgressNotice.Visible = true;
        }

        public sealed class ProgressMilestone
        {
            public string Week { get; set; }
            public string HoursLabel { get; set; }
            public string TargetHours { get; set; }
            public string Fill { get; set; }
            public string CssClass { get; set; }
            public string Marker { get; set; }
        }
    }
}
