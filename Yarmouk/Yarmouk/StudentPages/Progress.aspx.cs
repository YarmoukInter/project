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
                var placementsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Placements, auth.IdToken);
                var evaluationsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Evaluations, auth.IdToken);
                var studentTask = firebase.GetAsync<Student>(DatabasePaths.Students + "/" + auth.Uid, auth.IdToken);
                var logsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TrainingLogs, auth.IdToken);
                var notesTask = firebase.GetAsync<Dictionary<string, JObject>>(
                    DatabasePaths.StudentProgressNotes + "/" + auth.Uid, auth.IdToken);
                await Task.WhenAll(studentTask, logsTask, notesTask, tasksTask, submissionsTask, placementsTask, evaluationsTask);

                var student = await studentTask;
                var logs = (await logsTask ?? new Dictionary<string, JObject>()).Values
                    .Where(x => Text(x, "studentUid") == auth.Uid).ToList();

                int required = student == null ? 0 : Math.Max(0, student.RequiredTrainingHours);
                int approved = logs.Count == 0 && student != null ? Math.Max(0, student.ApprovedTrainingHours)
                    : logs.Where(IsApproved).Sum(x => Math.Max(0, ReadInt(x, "hours")));
                var tasks = await tasksTask ?? new Dictionary<string, JObject>();
                var submissions = await submissionsTask ?? new Dictionary<string, JObject>();
                var placements = await placementsTask ?? new Dictionary<string, JObject>();
                string placementId = FindActivePlacementId(placements, auth.Uid, activeOpportunityId);
                var assignedTasks = GetAssignedTasks(tasks, auth.Uid, activeOpportunityId, placementId);
                int completedTasks = CountCompletedTasks(assignedTasks, auth.Uid, submissions);
                bool allAssignedTasksCompleted = assignedTasks.Count > 0 && completedTasks == assignedTasks.Count;
                int percent = TrainingProgress.Percent(completedTasks);

                CompletedTaskCount.Text = completedTasks.ToString(CultureInfo.InvariantCulture);
                RequiredHours.Text = TrainingProgress.RequiredTasks.ToString(CultureInfo.InvariantCulture);
                TargetHours.Text = RequiredHours.Text;
                TrackTarget.Text = RequiredHours.Text;
                PlanRequiredHours.Text = required == 0 ? "—" : required.ToString("N0", CultureInfo.InvariantCulture);
                PlanApprovedHours.Text = approved.ToString("N0", CultureInfo.InvariantCulture) + " hours";
                CompletionPercent.Text = percent.ToString(CultureInfo.InvariantCulture);
                progressRing.Style["--progress"] = percent + "%";
                progressRing.Attributes["aria-valuenow"] = percent.ToString(CultureInfo.InvariantCulture);
                hoursProgressBar.Attributes["aria-valuemax"] = RequiredHours.Text;
                hoursProgressBar.Attributes["aria-valuenow"] = Math.Min(completedTasks, TrainingProgress.RequiredTasks).ToString(CultureInfo.InvariantCulture);
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
                MilestoneRepeater.DataSource = BuildMilestones(completedTasks, TrainingProgress.RequiredTasks);
                MilestoneRepeater.DataBind();

                NextStepCard.Visible = !allAssignedTasksCompleted;
                AcademicMeetingCard.Visible = allAssignedTasksCompleted;
                if (allAssignedTasksCompleted)
                {
                    var evaluations = await evaluationsTask ?? new Dictionary<string, JObject>();
                    BindAcademicMeeting(evaluations, auth.Uid, placementId);
                }
                bool certificateAvailable = student != null && student.CompletionCertificateAvailable
                    && !string.IsNullOrWhiteSpace(student.CompletionCertificateEvaluationId);
                CompletionCertificateLink.Visible = certificateAvailable;
                CertificatePendingLabel.Visible = !certificateAvailable;
                CompletionCertificateLink.NavigateUrl = certificateAvailable
                    ? ResolveUrl("~/CoordinatorPages/CompletionCertificate.ashx?evaluationId=" + HttpUtility.UrlEncode(student.CompletionCertificateEvaluationId))
                    : string.Empty;

                if (student == null) ShowNotice("Your student training record has not been created yet.");
                else if (required == 0) ShowNotice("Your required training hours have not been set yet.");
            }
            catch (Exception error)
            {
                try { System.Diagnostics.Trace.TraceError("Student progress load failed: " + error); }
                catch { /* Diagnostics must never interrupt page error handling. */ }
                SetLiteral(CompletionPercent, "—");
                SetLiteral(CompletedTaskCount, "—");
                SetLiteral(RequiredHours, "—");
                ShowNotice("Progress data could not be loaded. Please refresh the page and try again.");
            }
        }

        private static List<KeyValuePair<string, JObject>> GetAssignedTasks(Dictionary<string, JObject> tasks,
            string studentUid, string opportunityId, string placementId)
        {
            if (tasks == null || string.IsNullOrWhiteSpace(studentUid)) return new List<KeyValuePair<string, JObject>>();
            return tasks
                .Where(item => item.Value != null
                    && Same(Text(item.Value, "studentUid"), studentUid)
                    && IsCurrentPlacementTask(item.Value, opportunityId, placementId))
                .ToList();
        }

        private static int CountCompletedTasks(IEnumerable<KeyValuePair<string, JObject>> tasks, string studentUid,
            Dictionary<string, JObject> submissions)
        {
            return (tasks ?? Enumerable.Empty<KeyValuePair<string, JObject>>())
                .Where(task => IsTaskCompleted(task, studentUid, submissions))
                .Select(task => string.IsNullOrWhiteSpace(Text(task.Value, "id")) ? task.Key : Text(task.Value, "id"))
                .Distinct(StringComparer.Ordinal)
                .Count();
        }

        private static bool IsCurrentPlacementTask(JObject task, string opportunityId, string placementId)
        {
            string taskPlacementId = Text(task, "placementId");
            if (!string.IsNullOrWhiteSpace(taskPlacementId)) return Same(taskPlacementId, placementId);
            string taskOpportunityId = Text(task, "opportunityId");
            if (!string.IsNullOrWhiteSpace(taskOpportunityId)) return Same(taskOpportunityId, opportunityId);
            return true;
        }

        private static bool IsTaskCompleted(KeyValuePair<string, JObject> task, string studentUid,
            Dictionary<string, JObject> submissions)
        {
            if (IsCompletedTaskStatus(Text(task.Value, "status"))) return true;
            string taskId = Text(task.Value, "id", task.Key);
            var latest = (submissions ?? new Dictionary<string, JObject>())
                .Where(item => item.Value != null && Same(Text(item.Value, "taskId"), taskId)
                    && Same(Text(item.Value, "studentUid"), studentUid))
                .OrderByDescending(item => Number(item.Value, "submittedAtUnixMs"))
                .FirstOrDefault();
            return latest.Value != null && IsCompletedTaskStatus(Text(latest.Value, "reviewStatus", Text(latest.Value, "status")));
        }

        private static string FindActivePlacementId(Dictionary<string, JObject> placements, string studentUid, string opportunityId)
        {
            var match = (placements ?? new Dictionary<string, JObject>())
                .Where(item => item.Value != null && Same(Text(item.Value, "studentUid"), studentUid)
                    && (string.IsNullOrWhiteSpace(opportunityId) || Same(Text(item.Value, "opportunityId"), opportunityId))
                    && IsActivePlacementStatus(Text(item.Value, "status")))
                .OrderByDescending(item => Number(item.Value, "createdAtUnixMs"))
                .FirstOrDefault();
            return match.Value == null ? string.Empty : Text(match.Value, "id", match.Key);
        }

        private void BindAcademicMeeting(Dictionary<string, JObject> evaluations, string studentUid, string placementId)
        {
            var meeting = (evaluations ?? new Dictionary<string, JObject>())
                .Where(item => item.Value != null && Same(Text(item.Value, "studentUid"), studentUid)
                    && Same(Text(item.Value, "placementId"), placementId))
                .OrderByDescending(item => Number(item.Value, "academicUpdatedAtUnixMs"))
                .FirstOrDefault().Value;
            string date = Text(meeting, "zoomMeetingDate");
            string link = Text(meeting, "zoomMeetingUrl");
            string note = Text(meeting, "zoomMeetingAssessment");
            bool hasDate = !string.IsNullOrWhiteSpace(date);
            bool hasLink = IsSafeHttpsUrl(link);
            bool hasNote = !string.IsNullOrWhiteSpace(note);
            AcademicMeetingStatus.Text = hasDate || hasLink || hasNote
                ? "Your academic supervisor has shared the details below."
                : "You have completed all assigned tasks. Your academic supervisor will share the meeting details here.";
            AcademicMeetingDateRow.Visible = hasDate;
            AcademicMeetingNoteRow.Visible = hasNote;
            AcademicMeetingDate.Text = hasDate ? HttpUtility.HtmlEncode(FormatMeetingDate(date)) : string.Empty;
            AcademicMeetingNote.Text = hasNote ? HttpUtility.HtmlEncode(note) : string.Empty;
            AcademicMeetingLink.Visible = hasLink;
            AcademicMeetingLink.NavigateUrl = hasLink ? link : string.Empty;
        }

        private static bool IsCompletedTaskStatus(string value)
        {
            var status = Normalize(value);
            return status == "completed" || status == "complete" || status == "approved";
        }

        private static bool IsActivePlacementStatus(string value)
        {
            var status = Normalize(value);
            return status == "confirmed" || status == "accepted" || status == "inprogress" || status == "active"
                || status == "ongoing" || status == "completed";
        }

        private static bool IsSafeHttpsUrl(string value)
        {
            Uri uri;
            return Uri.TryCreate(value, UriKind.Absolute, out uri) && uri.Scheme == Uri.UriSchemeHttps;
        }

        private static string FormatMeetingDate(string value)
        {
            DateTime date;
            return DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date) ? date.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture) : value;
        }

        private static List<ProgressMilestone> BuildMilestones(int completedTasks, int required)
        {
            var result = new List<ProgressMilestone>();
            if (required <= 0) return result;
            for (int i = 1; i <= StageCount; i++)
            {
                int start = (required * (i - 1) + StageCount - 1) / StageCount;
                int end = (required * i + StageCount - 1) / StageCount;
                int target = end - start;
                int hours = Math.Min(target, Math.Max(0, completedTasks - start));
                bool complete = completedTasks >= end;
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
        private static bool Same(string left, string right) { return string.Equals(left, right, StringComparison.Ordinal); }
        private static string Normalize(string value) { return (value ?? "").Trim().Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant(); }
        private void ShowNotice(string message)
        {
            if (NoticeText != null) NoticeText.Text = HttpUtility.HtmlEncode(message);
            if (ProgressNotice != null) ProgressNotice.Visible = true;
        }

        private static void SetLiteral(System.Web.UI.WebControls.Literal control, string value)
        {
            if (control != null) control.Text = value;
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
