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
        private const int StageCount = TrainingProgress.RequiredTasks;
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
                var tasksTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Tasks, auth.IdToken);
                var submissionsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TaskSubmissions, auth.IdToken);
                var studentTask = firebase.GetAsync<Student>(DatabasePaths.Students + "/" + auth.Uid, auth.IdToken);
                var logsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TrainingLogs, auth.IdToken);
                var notesTask = firebase.GetAsync<Dictionary<string, JObject>>(
                    DatabasePaths.StudentProgressNotes + "/" + auth.Uid, auth.IdToken);
                await Task.WhenAll(studentTask, logsTask, notesTask, tasksTask, submissionsTask);

                var student = await studentTask;
                var logs = (await logsTask ?? new Dictionary<string, JObject>()).Values
                    .Where(x => Text(x, "studentUid") == auth.Uid).ToList();

                int required = student == null ? 0 : Math.Max(0, student.RequiredTrainingHours);
                int approved = logs.Count == 0 && student != null ? Math.Max(0, student.ApprovedTrainingHours)
                    : logs.Where(IsApproved).Sum(x => Math.Max(0, ReadInt(x, "hours")));
                var tasks = (await tasksTask ?? new Dictionary<string, JObject>())
                    .Where(t => !string.IsNullOrEmpty(activeOpportunityId) && Text(t.Value, "opportunityId") == activeOpportunityId);
                int submitted = TrainingProgress.CountSubmitted(tasks,
                    (await submissionsTask ?? new Dictionary<string, JObject>()).Values, auth.Uid);
                int percent = TrainingProgress.Percent(submitted);

                ApprovedHours.Text = submitted.ToString(CultureInfo.InvariantCulture);
                RequiredHours.Text = TrainingProgress.RequiredTasks.ToString(CultureInfo.InvariantCulture);
                TargetHours.Text = RequiredHours.Text;
                TrackTarget.Text = RequiredHours.Text;
                PlanRequiredHours.Text = required == 0 ? "—" : required.ToString("N0", CultureInfo.InvariantCulture);
                PlanApprovedHours.Text = approved.ToString("N0", CultureInfo.InvariantCulture) + " hours";
                CompletionPercent.Text = percent.ToString(CultureInfo.InvariantCulture);
                progressRing.Style["--progress"] = percent + "%";
                progressRing.Attributes["aria-valuenow"] = percent.ToString(CultureInfo.InvariantCulture);
                hoursProgressBar.Attributes["aria-valuemax"] = RequiredHours.Text;
                hoursProgressBar.Attributes["aria-valuenow"] = Math.Min(submitted, TrainingProgress.RequiredTasks).ToString(CultureInfo.InvariantCulture);
                hoursTrack.Style["width"] = percent + "%";

                var notes = (await notesTask ?? new Dictionary<string, JObject>()).Values
                    .Where(x => Text(x, "studentUid") == auth.Uid && !string.IsNullOrWhiteSpace(Text(x, "body")))
                    .OrderByDescending(x => Number(x, "createdAtUnixMs"))
                    .Select(x => new
                    {
                        Author = Text(x, "academicSupervisorName", "Academic supervisor"),
                        Body = Text(x, "body"),
                        Date = FormatNoteDate(Number(x, "createdAtUnixMs")),
                        DateIso = FormatNoteIso(Number(x, "createdAtUnixMs"))
                    }).ToList();
                ProgressNotes.DataSource = notes;
                ProgressNotes.DataBind();
                NoProgressNotes.Visible = notes.Count == 0;
                MilestoneRepeater.DataSource = BuildMilestones(submitted, TrainingProgress.RequiredTasks);
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
        private static string FormatNoteDate(long value)
        {
            try { return value <= 0 ? "Date unavailable" : DateTimeOffset.FromUnixTimeMilliseconds(value).LocalDateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture); }
            catch (ArgumentOutOfRangeException) { return "Date unavailable"; }
        }
        private static string FormatNoteIso(long value)
        {
            try { return value <= 0 ? "" : DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
            catch (ArgumentOutOfRangeException) { return ""; }
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
