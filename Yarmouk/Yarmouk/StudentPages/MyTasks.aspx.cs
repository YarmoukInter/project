using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class MyTasks : Page
    {
        private static readonly HttpClient FirebaseClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();

        protected override void OnInit(EventArgs e)
        {
            if (TaskData == null) 
                TaskData = new System.Web.UI.WebControls.Literal();
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.Form != null)
                Page.Form.Enctype = "multipart/form-data";
            if (string.Equals(Request.QueryString["poll"], "1", StringComparison.Ordinal))
            {
                RegisterAsyncTask(new PageAsyncTask(PollTasksAsync));
                return;
            }
            if (IsPostBack)
            {
                if (string.Equals(Request.Form["MoveTaskAction"], "1", StringComparison.Ordinal))
                    RegisterAsyncTask(new PageAsyncTask(MoveTaskAsync));
                else if (string.Equals(Request.Form["StartTaskAction"], "1", StringComparison.Ordinal))
                    RegisterAsyncTask(new PageAsyncTask(StartTaskAsync));
                return;
            }

            var downloadId = Request.QueryString["download"];
            if (!string.IsNullOrWhiteSpace(downloadId))
            {
                RegisterAsyncTask(new PageAsyncTask(() => DownloadSubmissionAsync(downloadId)));
                return;
            }
            var taskPdfId = Request.QueryString["taskPdf"];
            if (!string.IsNullOrWhiteSpace(taskPdfId))
            {
                RegisterAsyncTask(new PageAsyncTask(() => DownloadTaskPdfAsync(taskPdfId)));
                return;
            }
            RegisterAsyncTask(new PageAsyncTask(LoadTasksAsync));
        }

        private async Task PollTasksAsync()
        {
            await LoadTasksAsync();
            Response.ContentType = "application/json";
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (string.Equals(Request.QueryString["poll"], "1", StringComparison.Ordinal))
            {
                writer.Write(HttpUtility.HtmlDecode(TaskData.Text ?? "[]"));
                return;
            }
            base.Render(writer);
        }

        private string GetFirebaseUrl(string path, AuthSession auth)
        {
            var databaseUrl = ConfigurationManager.AppSettings["FirebaseDatabaseUrl"];
            if (string.IsNullOrWhiteSpace(databaseUrl) || !Uri.IsWellFormedUriString(databaseUrl, UriKind.Absolute)
                || new Uri(databaseUrl).Scheme != Uri.UriSchemeHttps)
                throw new ConfigurationErrorsException("Set FirebaseDatabaseUrl to a valid HTTPS URL in Web.config.");
            if (auth == null || string.IsNullOrWhiteSpace(auth.IdToken))
                throw new UnauthorizedAccessException("Sign in first.");

            foreach (var segment in path.Split('/'))
            {
                if (string.IsNullOrWhiteSpace(segment) || segment == "." || segment == ".."
                    || segment.IndexOfAny(new[] { '.', '#', '$', '[', ']', '?', '&' }) >= 0)
                    throw new ArgumentException("Invalid Firebase path.");
            }

            return databaseUrl.TrimEnd('/') + "/" + path + ".json?auth=" + Uri.EscapeDataString(auth.IdToken);
        }

        private async Task<JObject> GetFirebaseObjectAsync(string path, AuthSession auth)
        {
            using (var response = await FirebaseClient.GetAsync(GetFirebaseUrl(path, auth)))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json) || json == "null") return null;
                return JObject.Parse(json);
            }
        }

        private async Task PutFirebaseObjectAsync(string path, JObject value, AuthSession auth)
        {
            var json = value.ToString(Formatting.None);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var response = await FirebaseClient.PutAsync(GetFirebaseUrl(path, auth), content))
            {
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task PatchFirebaseObjectAsync(string path, JObject value, AuthSession auth)
        {
            var json = value.ToString(Formatting.None);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), GetFirebaseUrl(path, auth)))
            {
                request.Content = content;
                using (var response = await FirebaseClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        private async Task LoadTasksAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null) return;

            try
            {
                var tasksRequest = GetFirebaseObjectAsync(DatabasePaths.Tasks, auth);
                var submissionsRequest = GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions, auth);
                await Task.WhenAll(tasksRequest, submissionsRequest);



                var taskData = await tasksRequest;
                var submissionData = await submissionsRequest;





                var ownSubmissions = new Dictionary<string, JObject>();
                if (submissionData != null)
                {
                    foreach (var item in submissionData.Properties())
                    {
                        var submission = item.Value as JObject;
                        if (submission == null || (string)submission["studentUid"] != auth.Uid) continue;
                        var taskId = (string)submission["taskId"] ?? string.Empty;
                        JObject previous;
                        if (!ownSubmissions.TryGetValue(taskId, out previous)
                            || ((long?)submission["submittedAtUnixMs"] ?? 0) > ((long?)previous["submittedAtUnixMs"] ?? 0))
                            ownSubmissions[taskId] = submission;
                    }
                }

                var viewModels = new List<object>();
                if (taskData != null)
                {
                    foreach (var item in taskData.Properties())
                    {
                        var task = item.Value.ToObject<TrainingTask>();
                        if (task == null || !string.Equals(task.StudentUid, auth.Uid, StringComparison.Ordinal)) continue;
                        var taskId = string.IsNullOrWhiteSpace(task.Id) ? item.Name : task.Id;
                        string taskPdfBase64;
                        string taskPdfExternalUrl;
                        string taskPdfFileName;
                        var hasTaskPdf = TryGetTaskPdf(item.Value as JObject, out taskPdfBase64, out taskPdfExternalUrl, out taskPdfFileName);
                        var taskPdfUrl = !hasTaskPdf ? string.Empty : (!string.IsNullOrWhiteSpace(taskPdfBase64)
                            ? ResolveUrl("~/StudentPages/MyTasks.aspx?taskPdf=" + HttpUtility.UrlEncode(taskId)) : taskPdfExternalUrl);
                        JObject submission;
                        ownSubmissions.TryGetValue(taskId, out submission);
                        var taskRecord = item.Value as JObject;
                        var taskFeedback = taskRecord == null ? string.Empty : (string)(taskRecord["instructorFeedback"] ?? taskRecord["reviewNotes"] ?? taskRecord["feedback"]);
                        if (string.IsNullOrWhiteSpace(taskFeedback) && submission != null)
                            taskFeedback = (string)(submission["instructorFeedback"] ?? submission["reviewNotes"] ?? submission["feedback"]);
                        var status = GetStudentStatus(submission, task.Status, taskFeedback);
                        viewModels.Add(new
                        {
                            id = taskId,
                            title = task.Title ?? "Untitled task",
                            description = task.Description ?? "No task details were provided.",
                            taskPdfUrl = taskPdfUrl,
                            taskPdfFileName = taskPdfFileName,
                            dueAtUnixMs = task.DueAtUnixMs,
                            status = status,
                            statusLabel = GetStatusLabel(status),


                            submittedAtUnixMs = submission == null ? 0L : ((long?)submission["submittedAtUnixMs"] ?? 0L),
                            submissionId = submission == null ? "" : (string)submission["id"],
                            submissionUrl = submission == null ? (taskRecord == null ? "" : (string)taskRecord["submissionUrl"]) : (string)submission["submissionUrl"],
                            notes = submission == null ? (taskRecord == null ? "" : (string)taskRecord["submissionNotes"]) : (string)submission["notes"],
                            reviewNotes = !string.IsNullOrWhiteSpace(taskFeedback) ? taskFeedback : (submission == null ? "" : (string)(submission["instructorFeedback"] ?? submission["reviewNotes"] ?? submission["feedback"]))
                        });
                    }
                }

                TaskData = TaskData ?? new System.Web.UI.WebControls.Literal();
                TaskData.Text = HttpUtility.HtmlAttributeEncode(serializer.Serialize(viewModels));
            }
            catch (Exception)
            {
                PageMessage.Text = "<div class=\"tasks-alert\" role=\"alert\">Your tasks could not be loaded right now. Please refresh the page and try again.</div>";
                TaskData = TaskData ?? new System.Web.UI.WebControls.Literal();
                TaskData.Text = HttpUtility.HtmlAttributeEncode("[]");
            }
        }

        private static JToken FindTaskPdfProperty(JObject task, params string[] names)
        {
            if (task == null) return null;
            foreach (var name in names)
            {
                var property = task.Properties().FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
                if (property != null && property.Value != null && property.Value.Type != JTokenType.Null)
                    return property.Value;
            }
            return null;
        }

        private static string TaskPdfString(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null) return string.Empty;
            return token.Type == JTokenType.String ? ((string)token ?? string.Empty).Trim() : string.Empty;
        }

        private static string NormalizePdfBase64(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            value = value.Trim();
            const string dataPrefix = "data:application/pdf;base64,";
            if (value.StartsWith(dataPrefix, StringComparison.OrdinalIgnoreCase))
                value = value.Substring(dataPrefix.Length);
            return value;
        }

        private static bool TryGetTaskPdf(JObject task, out string base64, out string externalUrl, out string fileName)
        {
            base64 = TaskPdfString(FindTaskPdfProperty(task, "taskPdfBase64", "assignmentPdfBase64", "pdfBase64", "attachmentBase64", "fileBase64", "documentBase64", "pdfData"));
            externalUrl = TaskPdfString(FindTaskPdfProperty(task, "taskPdfUrl", "assignmentPdfUrl", "pdfUrl", "attachmentUrl", "fileUrl", "documentUrl", "downloadUrl"));
            fileName = TaskPdfString(FindTaskPdfProperty(task, "taskPdfFileName", "assignmentPdfFileName", "pdfFileName", "attachmentFileName", "fileName", "documentName"));

            var attachment = FindTaskPdfProperty(task, "taskPdf", "assignmentPdf", "referencePdf", "attachment", "attachments", "pdf");
            if (attachment is JArray)
                attachment = attachment.FirstOrDefault();
            var attachmentObject = attachment as JObject;
            if (attachmentObject != null)
            {
                if (string.IsNullOrWhiteSpace(base64))
                    base64 = TaskPdfString(FindTaskPdfProperty(attachmentObject, "base64", "data", "content", "pdfBase64", "fileBase64"));
                if (string.IsNullOrWhiteSpace(externalUrl))
                    externalUrl = TaskPdfString(FindTaskPdfProperty(attachmentObject, "url", "downloadUrl", "fileUrl", "pdfUrl"));
                if (string.IsNullOrWhiteSpace(fileName))
                    fileName = TaskPdfString(FindTaskPdfProperty(attachmentObject, "fileName", "name", "originalName"));
            }
            else if (string.IsNullOrWhiteSpace(base64) && string.IsNullOrWhiteSpace(externalUrl))
            {
                var rawValue = TaskPdfString(attachment);
                if (Uri.IsWellFormedUriString(rawValue, UriKind.Absolute)) externalUrl = rawValue;
                else if (!string.IsNullOrWhiteSpace(rawValue)) base64 = NormalizePdfBase64(rawValue);
            }

            base64 = NormalizePdfBase64(base64);
            if (!string.IsNullOrWhiteSpace(externalUrl))
            {
                Uri parsed;
                if (!Uri.TryCreate(externalUrl, UriKind.Absolute, out parsed)
                    || (parsed.Scheme != Uri.UriSchemeHttps && parsed.Scheme != Uri.UriSchemeHttp))
                    externalUrl = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(fileName)) fileName = "task-requirements.pdf";
            else fileName = Path.GetFileName(fileName);
            return !string.IsNullOrWhiteSpace(base64) || !string.IsNullOrWhiteSpace(externalUrl);
        }

        private async Task DownloadTaskPdfAsync(string taskId)
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null || string.IsNullOrWhiteSpace(taskId)
                || taskId.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?' }) >= 0)
            {
                CompleteNotFoundResponse();
                return;
            }

            try
            {
                var tasks = await GetFirebaseObjectAsync(DatabasePaths.Tasks, auth);
                var entry = tasks == null ? null : tasks.Properties().FirstOrDefault(item =>
                    item.Name == taskId || string.Equals((string)item.Value["id"], taskId, StringComparison.Ordinal));
                var task = entry == null ? null : entry.Value as JObject;
                if (task == null || (string)task["studentUid"] != auth.Uid)
                {
                    CompleteNotFoundResponse();
                    return;
                }

                string base64;
                string externalUrl;
                string fileName;
                if (!TryGetTaskPdf(task, out base64, out externalUrl, out fileName) || string.IsNullOrWhiteSpace(base64))
                {
                    CompleteNotFoundResponse();
                    return;
                }

                var pdfBytes = Convert.FromBase64String(base64);
                if (pdfBytes.Length < 5 || Encoding.ASCII.GetString(pdfBytes, 0, 5) != "%PDF-")
                {
                    CompleteNotFoundResponse();
                    return;
                }
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "inline; filename=\"task-requirements.pdf\"");
                Response.BinaryWrite(pdfBytes);
                Response.Flush();
                Response.SuppressContent = true;
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception)
            {
                CompleteNotFoundResponse();
            }
        }
        private async Task DownloadSubmissionAsync(string submissionId)
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null || string.IsNullOrWhiteSpace(submissionId) || submissionId.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?' }) >= 0)
            {
                CompleteNotFoundResponse();
                return;
            }

            try
            {
                var submission = await GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions + "/" + submissionId, auth);
                if (submission == null || (string)submission["studentUid"] != auth.Uid)
                {
                    CompleteNotFoundResponse();
                    return;
                }

                var taskId = (string)submission["taskId"];
                if (string.IsNullOrWhiteSpace(taskId) || taskId.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?' }) >= 0)
                {
                    CompleteNotFoundResponse();
                    return;
                }

                var encodedPdf = (string)submission["pdfBase64"];
                if (string.IsNullOrWhiteSpace(encodedPdf))
                {
                    CompleteNotFoundResponse();
                    return;
                }
                var pdfBytes = Convert.FromBase64String(encodedPdf);
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=\"task-submission.pdf\"");
                Response.BinaryWrite(pdfBytes);
                Response.Flush();
                Response.SuppressContent = true;
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception)
            {
                CompleteNotFoundResponse();
            }
        }

        private void CompleteNotFoundResponse()
        {
            Response.StatusCode = 404;
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }

        private static bool IsGitHubSolutionUrl(string value)
        {
            Uri uri;
            if (!Uri.TryCreate((value ?? string.Empty).Trim(), UriKind.Absolute, out uri) || uri.Scheme != Uri.UriSchemeHttps)
                return false;
            var host = uri.Host.ToLowerInvariant();
            return (host == "github.com" || host == "www.github.com" || host == "gist.github.com") && uri.AbsolutePath.Trim('/').Length > 0;
        }

        private static bool HasValidSubmission(JObject submission)
        {
            if (submission == null || string.IsNullOrWhiteSpace((string)submission["notes"])) return false;
            return IsGitHubSolutionUrl((string)submission["submissionUrl"])
                || !string.IsNullOrWhiteSpace((string)submission["pdfBase64"]);
        }
        private async Task StartTaskAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            var taskId = (SelectedTaskId.Value ?? string.Empty).Trim();
            if (auth == null || string.IsNullOrWhiteSpace(taskId)) return;
            try
            {
                var tasks = await GetFirebaseObjectAsync(DatabasePaths.Tasks, auth);
                var entry = tasks == null ? null : tasks.Properties().FirstOrDefault(item => item.Name == taskId || (string)item.Value["id"] == taskId);
                var task = entry == null ? null : entry.Value as JObject;
                if (task == null || (string)task["studentUid"] != auth.Uid) return;
                var currentStatus = ((string)task["status"] ?? "").Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
                var submissions = await GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions, auth);
                var hasValidSubmission = submissions != null && submissions.Properties()
                    .Select(item => item.Value as JObject)
                    .Any(item => item != null && (string)item["taskId"] == taskId && (string)item["studentUid"] == auth.Uid
                        && HasValidSubmission(item));
                var isSubmittedStatus = currentStatus == "done" || currentStatus == "submitted" || currentStatus == "underreview"
                    || currentStatus == "inreview" || currentStatus == "pendingreview";
                if (currentStatus == "complete" || currentStatus == "completed" || currentStatus == "approved"
                    || (isSubmittedStatus && hasValidSubmission)
                    || (currentStatus != "pending" && currentStatus != "assigned" && currentStatus != "todo"
                        && currentStatus != "revisionrequested" && currentStatus != "needsrevision" && currentStatus != "resubmit"
                        && !(isSubmittedStatus && !hasValidSubmission))) return;
                await PatchFirebaseObjectAsync(DatabasePaths.Tasks + "/" + entry.Name,
                    JObject.FromObject(new { status = "InProgress" }), auth);
                ShowSubmissionSuccess("Task moved to In Progress.");
            }
            catch (Exception) { ShowSubmissionError("The task status could not be updated. Please try again."); }
            await LoadTasksAsync();
        }

        private async Task MoveTaskAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            var taskId = (SelectedTaskId.Value ?? string.Empty).Trim();
            var targetStatus = (TargetTaskStatus.Value ?? string.Empty).Trim();
            if (auth == null || string.IsNullOrWhiteSpace(taskId)
                || taskId.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?' }) >= 0
                || (targetStatus != "todo" && targetStatus != "inProgress"))
            {
                ShowSubmissionError("The task could not be moved. Refresh the page and try again.");
                await LoadTasksAsync();
                return;
            }

            try
            {
                var tasks = await GetFirebaseObjectAsync(DatabasePaths.Tasks, auth);
                var entry = tasks == null ? null : tasks.Properties().FirstOrDefault(item => item.Name == taskId || (string)item.Value["id"] == taskId);
                var task = entry == null ? null : entry.Value as JObject;
                if (task == null || (string)task["studentUid"] != auth.Uid)
                {
                    ShowSubmissionError("This task is not assigned to your account.");
                    await LoadTasksAsync();
                    return;
                }

                var currentStatus = ((string)task["status"] ?? string.Empty).Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
                var submissions = await GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions, auth);
                var latestSubmission = submissions == null ? null : submissions.Properties()
                    .Select(item => item.Value as JObject)
                    .Where(item => item != null && (string)item["taskId"] == taskId && (string)item["studentUid"] == auth.Uid
                        && HasValidSubmission(item))
                    .OrderByDescending(item => (long?)item["submittedAtUnixMs"] ?? 0L)
                    .FirstOrDefault();
                var taskFeedback = (string)(task["instructorFeedback"] ?? task["reviewNotes"] ?? task["feedback"] ?? (latestSubmission == null ? null : (latestSubmission["instructorFeedback"] ?? latestSubmission["reviewNotes"] ?? latestSubmission["feedback"])));
                var taskRequestsRevision = currentStatus == "todo" || currentStatus == "revision" || currentStatus == "revisionrequested"
                    || currentStatus == "needsrevision" || currentStatus == "resubmit";


                var latestReviewStatus = ((string)(latestSubmission == null ? null : (latestSubmission["reviewStatus"] ?? latestSubmission["status"])) ?? string.Empty)
                    .Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
                var revisionWasRequested = (taskRequestsRevision && !string.IsNullOrWhiteSpace(taskFeedback))
                    || latestReviewStatus == "revisionrequested" || latestReviewStatus == "needsrevision" || latestReviewStatus == "resubmit" || latestReviewStatus == "revision" || latestReviewStatus == "todo";
                if (latestSubmission != null && !revisionWasRequested)
                {
                    ShowSubmissionError("A submitted task can only be moved after the supervisor requests a revision.");
                    await LoadTasksAsync();
                    return;
                }
                var isSubmittedStatus = currentStatus == "done" || currentStatus == "submitted" || currentStatus == "underreview"
                    || currentStatus == "inreview" || currentStatus == "pendingreview";
                var isSupervisorCompleted = currentStatus == "complete" || currentStatus == "completed" || currentStatus == "approved";
                var isLockedStatus = isSupervisorCompleted || (isSubmittedStatus && latestSubmission != null);
                if (isLockedStatus && !revisionWasRequested)
                {
                    ShowSubmissionError("Tasks in Done or Complete cannot be moved by the student.");
                    await LoadTasksAsync();
                    return;
                }

                await PatchFirebaseObjectAsync(DatabasePaths.Tasks + "/" + entry.Name,
                    JObject.FromObject(new { status = targetStatus == "inProgress" ? "InProgress" : "ToDo" }), auth);
                ShowSubmissionSuccess(targetStatus == "inProgress" ? "Task moved to In Progress." : "Task moved to To do.");
            }
            catch (Exception)
            {
                ShowSubmissionError("Could not save the task status to Firebase. Check the database rules and try again.");
            }
            await LoadTasksAsync();
        }

        protected void SubmitTask_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(SubmitTaskAsync));
        }

        private async Task SubmitTaskAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null) return;

            var taskId = (SelectedTaskId.Value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(taskId) || taskId.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?' }) >= 0)
            {
                ShowSubmissionError("Choose a valid task before submitting.");
                await LoadTasksAsync();
                return;
            }

            try
            {
                var tasks = await GetFirebaseObjectAsync(DatabasePaths.Tasks, auth);
                var taskEntry = tasks == null ? null : tasks.Properties().FirstOrDefault(item =>
                {
                    var entry = item.Value.ToObject<TrainingTask>();
                    return item.Name == taskId || (entry != null && entry.Id == taskId);
                });
                var taskRecord = taskEntry == null ? null : taskEntry.Value as JObject;
                var task = taskRecord == null ? null : taskRecord.ToObject<TrainingTask>();
                if (task == null || task.StudentUid != auth.Uid)
                {
                    ShowSubmissionError("This task is not assigned to your account.");
                    await LoadTasksAsync();
                    return;
                }

                var solutionUrl = (GitHubSolutionUrl.Text ?? string.Empty).Trim();
                var submissionNotes = (SubmissionNotes.Text ?? string.Empty).Trim();
                if (!IsGitHubSolutionUrl(solutionUrl))
                {
                    ShowSubmissionError("Enter a valid HTTPS GitHub repository or gist link.");
                    await LoadTasksAsync();
                    return;
                }
                if (submissionNotes.Length == 0)
                {
                    ShowSubmissionError("Add a note for your supervisor before submitting.");
                    await LoadTasksAsync();
                    return;
                }

                var submittedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (task.DueAtUnixMs > 0 && submittedAtUnixMs > task.DueAtUnixMs)
                {
                    ShowSubmissionError("The deadline has passed. This task can no longer be submitted.");
                    await LoadTasksAsync();
                    return;
                }

                var taskStatus = ((string)taskRecord["status"] ?? string.Empty).Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
                var taskFeedback = (string)(taskRecord["instructorFeedback"] ?? taskRecord["reviewNotes"] ?? taskRecord["feedback"]);


                var submissions = await GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions, auth);
                var latest = submissions == null ? null : submissions.Properties()
                    .Select(item => item.Value as JObject)
                    .Where(item => item != null && (string)item["taskId"] == taskId && (string)item["studentUid"] == auth.Uid)
                    .OrderByDescending(item => (long?)item["submittedAtUnixMs"] ?? 0L)
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(taskFeedback) && latest != null)
                    taskFeedback = (string)(latest["instructorFeedback"] ?? latest["reviewNotes"] ?? latest["feedback"]);
                var latestReviewStatus = ((string)(latest == null ? null : (latest["reviewStatus"] ?? latest["status"])) ?? string.Empty)
                    .Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
                var revisionRequested = ((taskStatus == "todo" || taskStatus == "revision" || taskStatus == "revisionrequested" || taskStatus == "needsrevision" || taskStatus == "resubmit")
                        && !string.IsNullOrWhiteSpace(taskFeedback))
                    || latestReviewStatus == "revisionrequested" || latestReviewStatus == "needsrevision" || latestReviewStatus == "resubmit" || latestReviewStatus == "revision";
                if (taskStatus != "inprogress" && !revisionRequested)
                {
                    ShowSubmissionError("Move the task to In Progress before submitting it.");
                    await LoadTasksAsync();
                    return;
                }
                if (HasValidSubmission(latest) && !revisionRequested)
                {
                    ShowSubmissionError("This task is already submitted and waiting for supervisor review.");
                    await LoadTasksAsync();
                    return;
                }
                if (string.Equals(GetStudentStatus(latest, task.Status, taskFeedback), "completed", StringComparison.Ordinal))
                {
                    ShowSubmissionError("This task is already marked complete.");
                    await LoadTasksAsync();
                    return;
                }

                var studentData = await GetFirebaseObjectAsync(DatabasePaths.Students + "/" + auth.Uid, auth);
                var student = studentData == null ? null : studentData.ToObject<Student>();
                var submissionId = Guid.NewGuid().ToString("N");
                var submission = JObject.FromObject(new
                {
                    id = submissionId,
                    taskId = taskId,
                    studentUid = auth.Uid,
                    submissionUrl = solutionUrl,
                    notes = submissionNotes,
                    submittedAtUnixMs = submittedAtUnixMs,
                    status = "done",
                    studentName = auth.User == null ? auth.Email : auth.User.FullName,
                    studentNumber = student == null ? "" : student.StudentId
                });
                await PutFirebaseObjectAsync(DatabasePaths.TaskSubmissions + "/" + submissionId, submission, auth);
                await PatchFirebaseObjectAsync(DatabasePaths.Tasks + "/" + taskEntry.Name,
                    JObject.FromObject(new
                    {
                        status = "done",
                        submissionUrl = solutionUrl,
                        submissionNotes = submissionNotes,
                        submittedAtUnixMs = submittedAtUnixMs,
                        instructorFeedback = ""
                    }), auth);

                GitHubSolutionUrl.Text = string.Empty;
                SubmissionNotes.Text = string.Empty;
                ShowSubmissionSuccess("Your GitHub solution was submitted successfully and is now under review.");
            }
            catch (Exception)
            {
                ShowSubmissionError("Your submission could not be saved. Please try again.");
            }

            await LoadTasksAsync();
        }

        private void ShowSubmissionError(string message)
        {
            PageMessage.Text = "<div class=\"tasks-alert\" role=\"alert\">" + HttpUtility.HtmlEncode(message) + "</div>";
        }

        private void ShowSubmissionSuccess(string message)
        {
            PageMessage.Text = "<div class=\"tasks-success\" role=\"status\">" + HttpUtility.HtmlEncode(message) + "</div>";
        }

        private static string GetStudentStatus(JObject submission, string taskStatus, string taskFeedback = null)
        {
            var normalizedTaskStatus = (taskStatus ?? string.Empty).Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
            if (normalizedTaskStatus == "completed" || normalizedTaskStatus == "complete" || normalizedTaskStatus == "approved") return "completed";
            if (normalizedTaskStatus == "inprogress" || normalizedTaskStatus == "progress") return "inProgress";
            var taskRequestsRevision = normalizedTaskStatus == "todo" || normalizedTaskStatus == "revision" || normalizedTaskStatus == "revisionrequested"
                || normalizedTaskStatus == "needsrevision" || normalizedTaskStatus == "resubmit";
            if (taskRequestsRevision && !string.IsNullOrWhiteSpace(taskFeedback)) return "revisionRequested";
            if (!HasValidSubmission(submission)) return "assigned";
            var status = (string)(submission["reviewStatus"] ?? submission["status"]);
            if (string.IsNullOrWhiteSpace(status)) return "inReview";
            status = status.Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
            if (status == "inprogress" || status == "progress") return "inProgress";
            if (status == "pending" || status == "submitted" || status == "done" || status == "underreview" || status == "inreview" || status == "pendingreview") return "done";
            if (status == "assigned" || status == "notsubmitted" || status == "unresolved" || status == "revision" || status == "revisionrequested" || status == "needsrevision" || status == "resubmit" || status == "todo") return "revisionRequested";
            if (status == "completed" || status == "complete" || status == "approved") return "completed";
            return "inReview";
        }

        private static string GetStatusLabel(string status)
        {
            switch (status)
            {
                case "completed": return "Completed";
                case "inReview": return "Under review";
                case "revisionRequested": return "Revision requested";
                default: return "Awaiting submission";
            }
        }
    }
}