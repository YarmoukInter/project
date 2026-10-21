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
        private const int MaxPdfBytes = 3 * 1024 * 1024;
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
            if (IsPostBack) return;

            var downloadId = Request.QueryString["download"];
            if (!string.IsNullOrWhiteSpace(downloadId))
            {
                RegisterAsyncTask(new PageAsyncTask(() => DownloadSubmissionAsync(downloadId)));
                return;
            }
            RegisterAsyncTask(new PageAsyncTask(LoadTasksAsync));
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

        private async Task LoadTasksAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null) return;

            try
            {
                var tasksRequest = GetFirebaseObjectAsync(DatabasePaths.Tasks, auth);
                var submissionsRequest = GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions, auth);
                var studentRequest = GetFirebaseObjectAsync(DatabasePaths.Students + "/" + auth.Uid, auth);
                await Task.WhenAll(tasksRequest, submissionsRequest, studentRequest);

                var taskData = await tasksRequest;
                var submissionData = await submissionsRequest;
                var studentData = await studentRequest;
                var student = studentData == null ? null : studentData.ToObject<Student>();
                StudentName.Text = HttpUtility.HtmlEncode(auth.User == null ? auth.Email : auth.User.FullName);
                StudentNumber.Text = HttpUtility.HtmlEncode(student == null ? "—" : student.StudentId);

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
                        JObject submission;
                        ownSubmissions.TryGetValue(taskId, out submission);
                        var status = GetStudentStatus(submission, task.Status);
                        viewModels.Add(new
                        {
                            id = taskId,
                            title = task.Title ?? "Untitled task",
                            description = task.Description ?? "No task details were provided.",
                            dueAtUnixMs = task.DueAtUnixMs,
                            status = status,
                            statusLabel = GetStatusLabel(status),
                            studentName = auth.User == null ? auth.Email : auth.User.FullName,
                            studentNumber = student == null ? "—" : student.StudentId,
                            submittedAtUnixMs = submission == null ? 0L : ((long?)submission["submittedAtUnixMs"] ?? 0L),
                            submissionId = submission == null ? "" : (string)submission["id"],
                            notes = submission == null ? "" : (string)submission["notes"]
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

                var filePath = Server.MapPath("~/App_Data/TaskSubmissions/" + auth.Uid + "/" + taskId + "/" + submissionId + ".pdf");
                if (!File.Exists(filePath))
                {
                    CompleteNotFoundResponse();
                    return;
                }

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "attachment; filename=\"task-submission.pdf\"");
                Response.TransmitFile(filePath);
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
                var task = taskEntry == null ? null : taskEntry.Value.ToObject<TrainingTask>();
                if (task == null || task.StudentUid != auth.Uid)
                {
                    ShowSubmissionError("This task is not assigned to your account.");
                    await LoadTasksAsync();
                    return;
                }

                if (PdfUpload == null || !PdfUpload.HasFile)
                {
                    ShowSubmissionError("Choose the PDF file you want to submit.");
                    await LoadTasksAsync();
                    return;
                }

                var pdfStream = PdfUpload.PostedFile.InputStream;
                var signature = new byte[5];
                var signatureLength = pdfStream.Read(signature, 0, signature.Length);
                pdfStream.Position = 0;
                if (!string.Equals(Path.GetExtension(PdfUpload.FileName), ".pdf", StringComparison.OrdinalIgnoreCase)
                    || PdfUpload.PostedFile.ContentLength <= 0 || PdfUpload.PostedFile.ContentLength > MaxPdfBytes
                    || signatureLength != 5 || Encoding.ASCII.GetString(signature) != "%PDF-")
                {
                    ShowSubmissionError("Upload a valid PDF file no larger than 3 MB.");
                    await LoadTasksAsync();
                    return;
                }

                var submissions = await GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions, auth);
                JObject latest = null;
                if (submissions != null)
                {
                    latest = submissions.Properties()
                        .Select(item => item.Value as JObject)
                        .Where(item => item != null && (string)item["taskId"] == taskId && (string)item["studentUid"] == auth.Uid)
                        .OrderByDescending(item => (long?)item["submittedAtUnixMs"] ?? 0)
                        .FirstOrDefault();
                }
                if (string.Equals(GetStudentStatus(latest, task.Status), "completed", StringComparison.Ordinal))
                {
                    ShowSubmissionError("This task is already marked complete.");
                    await LoadTasksAsync();
                    return;
                }

                var submissionId = Guid.NewGuid().ToString("N");
                var relativeFolder = "~/App_Data/TaskSubmissions/" + auth.Uid + "/" + taskId;
                var folder = Server.MapPath(relativeFolder);
                Directory.CreateDirectory(folder);
                PdfUpload.SaveAs(Path.Combine(folder, submissionId + ".pdf"));

                var studentData = await GetFirebaseObjectAsync(DatabasePaths.Students + "/" + auth.Uid, auth);
                var student = studentData == null ? null : studentData.ToObject<Student>();
                var submission = JObject.FromObject(new
                {
                    id = submissionId,
                    taskId = taskId,
                    studentUid = auth.Uid,
                    submissionUrl = "",
                    notes = (SubmissionNotes.Text ?? string.Empty).Trim(),
                    submittedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    status = "submitted",
                    studentName = auth.User == null ? auth.Email : auth.User.FullName,
                    studentNumber = student == null ? "" : student.StudentId
                });
                await PutFirebaseObjectAsync(DatabasePaths.TaskSubmissions + "/" + submissionId, submission, auth);

                SubmissionNotes.Text = string.Empty;
                ShowSubmissionSuccess("Your PDF was submitted successfully and is now under review.");
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

        private static string GetStudentStatus(JObject submission, string taskStatus)
        {
            var status = submission == null ? taskStatus : (string)submission["status"];
            if (string.IsNullOrWhiteSpace(status)) return submission == null ? "assigned" : "inReview";
            status = status.Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
            if (status == "assigned" || status == "pending" || status == "notsubmitted" || status == "unresolved") return "assigned";
            if (status == "submitted" || status == "underreview" || status == "inreview" || status == "pendingreview") return "inReview";
            if (status == "completed" || status == "done" || status == "approved") return "completed";
            if (status == "revisionrequested" || status == "needsrevision" || status == "resubmit") return "revisionRequested";
            return submission == null ? "assigned" : "inReview";
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
