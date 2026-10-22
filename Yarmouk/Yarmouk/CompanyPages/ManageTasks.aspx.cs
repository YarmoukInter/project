using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.CompanyPages
{
    public partial class ManageTasks : Page
    {
        private static readonly HttpClient FirebaseClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        private sealed class StudentOption { public string Uid; public string Name; }
        private sealed class TaskRow
        {
            public string Id;
            public string Number;
            public string Title;
            public string Status;
            public string StatusLabel;
            public string SolutionUrl;
            public long DueAt;
            public long CreatedAt;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RegisterAsyncTask(new PageAsyncTask(() => LoadPageAsync(null)));
                return;
            }

            var addTask = Request.Form["addTaskAction"];
            var action = Request.Form["taskAction"];
            var selectedStudentUid = Request.Form[StudentSelector.UniqueID];
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                if (addTask == "1")
                    await CreateTaskAsync(selectedStudentUid);
                else if (!string.IsNullOrWhiteSpace(action))
                    await ProcessDecisionAsync(action, selectedStudentUid);
                await LoadPageAsync(selectedStudentUid);
            }));
        }

        private string GetFirebaseUrl(string path, AuthSession auth)
        {
            var databaseUrl = ConfigurationManager.AppSettings["FirebaseDatabaseUrl"];
            if (string.IsNullOrWhiteSpace(databaseUrl) || !Uri.IsWellFormedUriString(databaseUrl, UriKind.Absolute)
                || new Uri(databaseUrl).Scheme != Uri.UriSchemeHttps)
                throw new ConfigurationErrorsException("FirebaseDatabaseUrl must be a valid HTTPS URL.");
            if (auth == null || string.IsNullOrWhiteSpace(auth.IdToken)) throw new UnauthorizedAccessException("Please sign in again.");
            foreach (var segment in path.Split('/'))
                if (string.IsNullOrWhiteSpace(segment) || segment == "." || segment == ".."
                    || segment.IndexOfAny(new[] { '.', '#', '$', '[', ']', '?', '&' }) >= 0)
                    throw new ArgumentException("Invalid Firebase path.");
            return databaseUrl.TrimEnd('/') + "/" + path + ".json?auth=" + Uri.EscapeDataString(auth.IdToken);
        }

        private async Task<JObject> GetFirebaseObjectAsync(string path, AuthSession auth)
        {
            using (var response = await FirebaseClient.GetAsync(GetFirebaseUrl(path, auth)))
            {
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return string.IsNullOrWhiteSpace(json) || json == "null" ? null : JObject.Parse(json);
            }
        }

        private async Task PatchFirebaseObjectAsync(string path, JObject value, AuthSession auth)
        {
            using (var content = new StringContent(value.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json"))
            using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), GetFirebaseUrl(path, auth)))
            {
                request.Content = content;
                using (var response = await FirebaseClient.SendAsync(request)) response.EnsureSuccessStatusCode();
            }
        }

        private async Task LoadPageAsync(string requestedStudentUid)
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null) { ShowError("Your sign-in has expired. Sign in again."); return; }
            try
            {
                var companiesTask = GetFirebaseObjectAsync(DatabasePaths.Companies, auth);
                var studentsTask = GetFirebaseObjectAsync(DatabasePaths.Students, auth);
                var usersTask = GetFirebaseObjectAsync(DatabasePaths.Users, auth);
                var tasksTask = GetFirebaseObjectAsync(DatabasePaths.Tasks, auth);
                var submissionsTask = GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions, auth);
                await Task.WhenAll(companiesTask, studentsTask, usersTask, tasksTask, submissionsTask);

                var companies = await companiesTask;
                var students = await studentsTask;
                var users = await usersTask;
                var taskData = await tasksTask;
                var submissionData = await submissionsTask;
                var companyId = GetCompanyIdBySupervisorUid(auth.Uid, companies);
                if (companyId.Length == 0) { ShowError("Your signed-in account is not linked to a company in Firebase."); return; }
                var assignedUids = GetAssignedStudentUids(companyId, students);
                var options = GetStudentOptions(assignedUids, students, users);
                StudentChoices.Text = RenderStudentChoices(options);

                StudentSelector.Items.Clear();
                StudentSelector.Items.Add(new ListItem(options.Count == 0 ? "No assigned students" : "Choose a student", ""));
                foreach (var option in options) StudentSelector.Items.Add(new ListItem(option.Name, option.Uid));
                var selected = options.FirstOrDefault(item => item.Uid == requestedStudentUid) ?? options.FirstOrDefault();
                if (selected != null) StudentSelector.SelectedValue = selected.Uid;

                if (selected == null)
                {
                    TaskCount.Text = "";
                    TaskTable.Text = "<p class=\"empty-state\">No trainees are currently assigned to your company.</p>";
                    return;
                }

                var rows = BuildTaskRows(selected.Uid, taskData, submissionData);
                TaskCount.Text = "<span class=\"task-count\">" + rows.Count + (rows.Count == 1 ? " task" : " tasks") + "</span>";
                TaskTable.Text = RenderTaskTable(rows);
            }
            catch (Exception)
            {
                ShowError("Tasks could not be loaded from Firebase. Refresh the page and try again.");
                TaskTable.Text = "";
                TaskCount.Text = "";
            }
        }

        private async Task ProcessDecisionAsync(string actionValue, string selectedStudentUid)
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            var parts = (actionValue ?? string.Empty).Split(new[] { '|' }, 2);
            if (auth == null || parts.Length != 2 || (parts[0] != "complete" && parts[0] != "revision"))
            { ShowError("This task action is not valid. Refresh and try again."); return; }
            var action = parts[0];
            var taskId = parts[1];
            if (!IsSafeKey(taskId) || string.IsNullOrWhiteSpace(selectedStudentUid))
            { ShowError("Choose a student and try again."); return; }

            try
            {
                var companiesTask = GetFirebaseObjectAsync(DatabasePaths.Companies, auth);
                var studentsTask = GetFirebaseObjectAsync(DatabasePaths.Students, auth);
                var usersTask = GetFirebaseObjectAsync(DatabasePaths.Users, auth);
                var tasksTask = GetFirebaseObjectAsync(DatabasePaths.Tasks, auth);
                var submissionsTask = GetFirebaseObjectAsync(DatabasePaths.TaskSubmissions, auth);
                await Task.WhenAll(companiesTask, studentsTask, usersTask, tasksTask, submissionsTask);
                var companies = await companiesTask;
                var students = await studentsTask;
                var users = await usersTask;
                var taskData = await tasksTask;
                var submissionData = await submissionsTask;
                var companyId = GetCompanyIdBySupervisorUid(auth.Uid, companies);
                if (companyId.Length == 0) { ShowError("Your signed-in account is not linked to a company in Firebase."); return; }
                var assignedUids = GetAssignedStudentUids(companyId, students);
                if (!assignedUids.Contains(selectedStudentUid)) { ShowError("You do not have access to this student's tasks."); return; }

                var entry = FindRecord(taskData, taskId);
                var task = entry == null ? null : entry.Value as JObject;
                if (task == null || Value(task, "studentUid") != selectedStudentUid)
                { ShowError("This task is no longer available for this student."); return; }
                var latest = FindLatestSubmission(submissionData, taskId, selectedStudentUid);
                if (ResolveStatus(task, latest) != "done")
                { ShowError("Only tasks awaiting review can be approved or returned for changes."); return; }
                var solutionUrl = latest == null ? Value(task, "submissionUrl") : Value(latest.Value as JObject, "submissionUrl");
                if (!IsGitHubSolutionUrl(solutionUrl)) { ShowError("A valid GitHub solution link was not found for this submission."); return; }
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (action == "complete")
                {
                    await PatchFirebaseObjectAsync(DatabasePaths.Tasks + "/" + taskId, new JObject
                    {
                        ["status"] = "completed", ["completedAtUnixMs"] = now, ["reviewedByUid"] = auth.Uid,
                        ["instructorFeedback"] = JValue.CreateNull()
                    }, auth);
                    if (latest != null)
                        await PatchFirebaseObjectAsync(DatabasePaths.TaskSubmissions + "/" + latest.Name, new JObject
                        {
                            ["status"] = "completed", ["reviewStatus"] = "completed", ["reviewedByUid"] = auth.Uid,
                            ["reviewedAtUnixMs"] = now, ["instructorFeedback"] = JValue.CreateNull()
                        }, auth);
                    ShowSuccess("Task approved. It is now marked Completed for the student.");
                    return;
                }

                var feedback = (Request.Form["feedback_" + taskId] ?? string.Empty).Trim();
                var deadlineText = (Request.Form["deadline_" + taskId] ?? string.Empty).Trim();
                DateTime newDeadline;
                if (feedback.Length < 2 || feedback.Length > 2000)
                { ShowError("Enter revision feedback between 2 and 2,000 characters."); return; }
                if (!DateTime.TryParseExact(deadlineText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out newDeadline))
                { ShowError("Choose the new deadline for the revised task."); return; }
                var newDueAt = new DateTimeOffset(DateTime.SpecifyKind(newDeadline.Date, DateTimeKind.Unspecified), TimeSpan.FromHours(3)).ToUnixTimeMilliseconds();
                if (newDueAt <= now) { ShowError("The new deadline must be in the future (Jordan time)."); return; }

                var oldDueAt = TokenLong(task, "dueAtUnixMs");
                var history = task["deadlineHistory"] as JArray ?? new JArray();
                if (oldDueAt > 0)
                    history.Add(new JObject { ["dueAtUnixMs"] = oldDueAt, ["recordedAtUnixMs"] = now, ["changedByUid"] = auth.Uid });
                await PatchFirebaseObjectAsync(DatabasePaths.Tasks + "/" + taskId, new JObject
                {
                    ["status"] = "todo", ["dueAtUnixMs"] = newDueAt, ["deadlineHistory"] = history,
                    ["instructorFeedback"] = feedback, ["revisionRequestedAtUnixMs"] = now, ["reviewedByUid"] = auth.Uid
                }, auth);
                if (latest != null)
                    await PatchFirebaseObjectAsync(DatabasePaths.TaskSubmissions + "/" + latest.Name, new JObject
                    {
                        ["status"] = "revision", ["reviewStatus"] = "revision", ["instructorFeedback"] = feedback,
                        ["reviewedByUid"] = auth.Uid, ["reviewedAtUnixMs"] = now
                    }, auth);
                ShowSuccess("Revision requested. The old submission and deadline are preserved; the task is back in To do with the new deadline.");
            }
            catch (Exception)
            {
                ShowError("The decision could not be saved to Firebase. Refresh and try again.");
            }
        }

        private async Task CreateTaskAsync(string selectedStudentUid)
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null) { ShowError("Your sign-in has expired. Sign in again."); return; }
            try
            {
                var number = (Request.Form["newTaskNumber"] ?? string.Empty).Trim();
                var title = (Request.Form["newTaskTitle"] ?? string.Empty).Trim();
                var deadlineText = (Request.Form["newTaskDeadline"] ?? string.Empty).Trim();
                var description = (Request.Form["newTaskDescription"] ?? string.Empty).Trim();
                var taskUrl = (Request.Form["newTaskUrl"] ?? string.Empty).Trim();
                var selectedUids = Request.Form.GetValues("targetStudentUid") ?? new string[0];
                DateTime deadline;

                if (!IsValidTaskNumber(number)) { ShowError("Use a task number with letters, numbers, hyphens, or underscores (up to 40 characters)."); return; }
                if (title.Length < 2 || title.Length > 160) { ShowError("Enter a task title between 2 and 160 characters."); return; }
                if (description.Length < 2 || description.Length > 4000) { ShowError("Enter task instructions between 2 and 4,000 characters."); return; }
                if (!DateTime.TryParseExact(deadlineText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out deadline))
                { ShowError("Choose a valid task deadline."); return; }
                var dueAt = new DateTimeOffset(DateTime.SpecifyKind(deadline.Date, DateTimeKind.Unspecified), TimeSpan.FromHours(3)).ToUnixTimeMilliseconds();
                if (dueAt <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) { ShowError("The task deadline must be in the future (Jordan time)."); return; }
                if (!IsHttpUrl(taskUrl)) { ShowError("Enter a valid HTTPS task link."); return; }

                var targetUids = selectedUids.Where(uid => !string.IsNullOrWhiteSpace(uid)).Distinct(StringComparer.Ordinal).ToList();
                if (targetUids.Count == 0) { ShowError("Select at least one trainee student."); return; }

                var companiesTask = GetFirebaseObjectAsync(DatabasePaths.Companies, auth);
                var studentsTask = GetFirebaseObjectAsync(DatabasePaths.Students, auth);
                var usersTask = GetFirebaseObjectAsync(DatabasePaths.Users, auth);
                await Task.WhenAll(companiesTask, studentsTask, usersTask);
                var companies = await companiesTask;
                var students = await studentsTask;
                var users = await usersTask;
                var companyId = GetCompanyIdBySupervisorUid(auth.Uid, companies);
                if (companyId.Length == 0) { ShowError("Your signed-in account is not linked to a company in Firebase."); return; }
                var assignedUids = GetAssignedStudentUids(companyId, students);
                if (targetUids.Any(uid => !assignedUids.Contains(uid)))
                { ShowError("Only students assigned to your company can receive this task."); return; }

                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var additions = new JObject();
                foreach (var uid in targetUids)
                {
                    var safeUid = new string(uid.Where(char.IsLetterOrDigit).ToArray());
                    var suffix = safeUid.Length > 6 ? safeUid.Substring(safeUid.Length - 6) : safeUid;
                    var recordId = number + "-" + suffix + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                    additions[recordId] = new JObject
                    {
                        ["id"] = recordId,
                        ["taskNumber"] = number,
                        ["title"] = title,
                        ["description"] = description,
                        ["taskPdfUrl"] = taskUrl,
                        ["dueAtUnixMs"] = dueAt,
                        ["status"] = "todo",
                        ["studentUid"] = uid,
                        ["studentName"] = GetStudentName(uid, students, users),
                        ["supervisorUid"] = auth.Uid,
                        ["createdAtUnixMs"] = now
                    };
                }
                await PatchFirebaseObjectAsync(DatabasePaths.Tasks, additions, auth);
                ShowSuccess("Task " + number + " was added to " + targetUids.Count + (targetUids.Count == 1 ? " student" : " students") + " in To do.");
            }
            catch (Exception)
            {
                ShowError("The task could not be saved to Firebase. Check the connection and try again.");
            }
        }

        private static string RenderStudentChoices(List<StudentOption> options)
        {
            if (options.Count == 0) return "<p class=\"picker-empty\">No trainee students are assigned to this company.</p>";
            var html = new StringBuilder();
            foreach (var option in options)
            {
                html.Append("<label class=\"trainee-option\"><input type=\"checkbox\" name=\"targetStudentUid\" value=\"")
                    .Append(HttpUtility.HtmlAttributeEncode(option.Uid)).Append("\" /><span>")
                    .Append(HttpUtility.HtmlEncode(option.Name)).Append("</span></label>");
            }
            return html.ToString();
        }

        private static string GetCompanyIdBySupervisorUid(string supervisorUid, JObject companies)
        {
            if (companies == null || string.IsNullOrWhiteSpace(supervisorUid)) return string.Empty;
            foreach (var companyEntry in companies.Properties())
            {
                var company = companyEntry.Value as JObject;
                if (company == null) continue;
                var supervisorUids = GetToken(company, "supervisorUids") as JObject;
                var isSupervisor = Value(company, "supervisorUid") == supervisorUid
                    || (supervisorUids != null && (bool?)GetToken(supervisorUids, supervisorUid) == true);
                if (!isSupervisor) continue;
                return FirstNonEmpty(Value(company, "companyId"), Value(company, "id"), companyEntry.Name).Trim();
            }
            return string.Empty;
        }

        private static HashSet<string> GetAssignedStudentUids(string companyId, JObject students)
        {
            var normalizedCompanyId = (companyId ?? string.Empty).Trim();
            var studentUids = new HashSet<string>(StringComparer.Ordinal);
            if (normalizedCompanyId.Length == 0 || students == null) return studentUids;
            if (students != null)
                foreach (var item in students.Properties())
                {
                    var student = item.Value as JObject;
                    if (student == null) continue;
                    var internshipStatus = NormalizeRecordStatus(Value(student, "internshipStatus"));
                    var studentCompanyId = Value(student, "companyId").Trim();
                    if (string.Equals(studentCompanyId, normalizedCompanyId, StringComparison.OrdinalIgnoreCase)
                        && (internshipStatus.Length == 0 || internshipStatus == "inprogress" || internshipStatus == "active" || internshipStatus == "accepted"))
                        studentUids.Add(FirstNonEmpty(Value(student, "uid"), Value(student, "studentUid"), Value(student, "userUid"), item.Name));
                }
            return studentUids;
        }

        private static List<StudentOption> GetStudentOptions(HashSet<string> assignedUids, JObject students, JObject users)
        {
            var result = new List<StudentOption>();
            if (students == null) return result;
            foreach (var uid in assignedUids)
            {
                var studentRecord = FindStudentRecord(students, uid);
                if (studentRecord == null) continue;
                result.Add(new StudentOption { Uid = uid, Name = GetStudentName(uid, students, users) });
            }
            return result.OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private static string GetStudentName(string uid, JObject students, JObject users)
        {
            var studentRecord = FindStudentRecord(students, uid);
            var student = studentRecord == null ? null : studentRecord.Value as JObject;
            var linkedUid = FirstNonEmpty(Value(student, "uid"), Value(student, "studentUid"), Value(student, "userUid"), uid);
            var userRecord = FindRecord(users, linkedUid);
            if (userRecord == null && users != null)
                userRecord = users.Properties().FirstOrDefault(item =>
                    Value(item.Value as JObject, "uid") == linkedUid || Value(item.Value as JObject, "studentUid") == linkedUid);
            var user = userRecord == null ? null : userRecord.Value as JObject;
            var fullName = FirstNonEmpty(Value(student, "fullName"), Value(student, "studentName"),
                Value(student, "englishName"), Value(student, "displayName"), Value(student, "name"),
                Value(user, "fullName"), Value(user, "studentName"), Value(user, "displayName"), Value(user, "name"));
            if (string.IsNullOrWhiteSpace(fullName))
                fullName = (FirstNonEmpty(Value(student, "firstName"), Value(user, "firstName")) + " " +
                    FirstNonEmpty(Value(student, "lastName"), Value(user, "lastName"))).Trim();
            return FirstNonEmpty(fullName, uid);
        }

        private static JProperty FindStudentRecord(JObject students, string uid)
        {
            if (students == null || string.IsNullOrWhiteSpace(uid)) return null;
            return FindRecord(students, uid) ?? students.Properties().FirstOrDefault(item =>
            {
                var student = item.Value as JObject;
                return Value(student, "uid") == uid || Value(student, "studentUid") == uid || Value(student, "userUid") == uid;
            });
        }

        private static List<TaskRow> BuildTaskRows(string studentUid, JObject tasks, JObject submissions)
        {
            var rows = new List<TaskRow>();
            if (tasks == null) return rows;
            foreach (var item in tasks.Properties())
            {
                var task = item.Value as JObject;
                if (task == null || Value(task, "studentUid") != studentUid) continue;
                var taskId = FirstNonEmpty(Value(task, "id"), item.Name);
                var latest = FindLatestSubmission(submissions, taskId, studentUid);
                var status = ResolveStatus(task, latest);
                var url = latest == null ? Value(task, "submissionUrl") : Value(latest.Value as JObject, "submissionUrl");
                rows.Add(new TaskRow
                {
                    Id = taskId, Number = FirstNonEmpty(Value(task, "taskNumber"), taskId),
                    Title = FirstNonEmpty(Value(task, "title"), "Untitled task"), Status = status,
                    StatusLabel = LabelForStatus(status), SolutionUrl = IsGitHubSolutionUrl(url) ? url : string.Empty,
                    DueAt = TokenLong(task, "dueAtUnixMs"), CreatedAt = TokenLong(task, "createdAtUnixMs")
                });
            }
            return rows.OrderBy(row => row.CreatedAt).ThenBy(row => row.Id, StringComparer.Ordinal).ToList();
        }

        private static string RenderTaskTable(List<TaskRow> rows)
        {
            if (rows.Count == 0) return "<p class=\"empty-state\">This student has no tasks yet.</p>";
            var html = new StringBuilder("<div class=\"table-scroll\"><table class=\"tasks-table\"><thead><tr><th>Task</th><th>Deadline</th><th>Status</th><th>Student solution</th><th>Review decision</th></tr></thead><tbody>");
            foreach (var row in rows)
            {
                var id = HttpUtility.HtmlAttributeEncode(row.Id);
                html.Append("<tr><td><strong>").Append(HttpUtility.HtmlEncode(row.Number)).Append("</strong><span class=\"task-title\">").Append(HttpUtility.HtmlEncode(row.Title)).Append("</span></td>");
                html.Append("<td>").Append(row.DueAt > 0 ? HttpUtility.HtmlEncode(FormatDate(row.DueAt)) : "—").Append("</td>");
                html.Append("<td><span class=\"status-pill status-").Append(HttpUtility.HtmlAttributeEncode(row.Status)).Append("\">").Append(HttpUtility.HtmlEncode(row.StatusLabel)).Append("</span></td><td>");
                if ((row.Status == "done" || row.Status == "completed") && !string.IsNullOrWhiteSpace(row.SolutionUrl))
                    html.Append("<a class=\"solution-link\" href=\"").Append(HttpUtility.HtmlAttributeEncode(row.SolutionUrl)).Append("\" target=\"_blank\" rel=\"noopener noreferrer\">Open GitHub solution</a>");
                else if (row.Status == "done" || row.Status == "completed") html.Append("<span class=\"muted\">No GitHub link saved</span>");
                else html.Append("<span class=\"muted\">Not submitted</span>");
                html.Append("</td><td>");
                if (row.Status == "done")
                {
                    html.Append("<div class=\"review-actions\"><button class=\"approve-button\" type=\"submit\" name=\"taskAction\" value=\"complete|").Append(id).Append("\" onclick=\"return confirmTaskApproval()\">Accept and complete</button>");
                    html.Append("<div class=\"revision-form\"><label for=\"feedback_").Append(id).Append("\">Revision notes</label><textarea id=\"feedback_").Append(id).Append("\" name=\"feedback_").Append(id).Append("\" maxlength=\"2000\" rows=\"2\" placeholder=\"Explain what needs to change\"></textarea>");
                    html.Append("<label for=\"deadline_").Append(id).Append("\">New deadline</label><input type=\"date\" id=\"deadline_").Append(id).Append("\" name=\"deadline_").Append(id).Append("\" />");
                    html.Append("<button class=\"revision-button\" type=\"submit\" name=\"taskAction\" value=\"revision|").Append(id).Append("\" onclick=\"return validateRevision(this)\">Request changes</button></div></div>");
                }
                else html.Append("<span class=\"muted\">Available after submission</span>");
                html.Append("</td></tr>");
            }
            html.Append("</tbody></table></div>");
            return html.ToString();
        }

        private static JProperty FindRecord(JObject records, string key)
        {
            return records == null ? null : records.Properties().FirstOrDefault(item => item.Name == key || Value(item.Value as JObject, "id") == key);
        }

        private static JProperty FindLatestSubmission(JObject submissions, string taskId, string studentUid)
        {
            return submissions == null ? null : submissions.Properties()
                .Where(item => Value(item.Value as JObject, "taskId") == taskId && Value(item.Value as JObject, "studentUid") == studentUid)
                .OrderByDescending(item => TokenLong(item.Value as JObject, "submittedAtUnixMs")).FirstOrDefault();
        }

        private static string ResolveStatus(JObject task, JProperty latestSubmission)
        {
            var taskStatus = NormalizeStatus(Value(task, "status"));
            if (taskStatus != "todo" || latestSubmission == null) return taskStatus;
            var rawSubmissionStatus = FirstNonEmpty(Value(latestSubmission.Value as JObject, "reviewStatus"), Value(latestSubmission.Value as JObject, "status"))
                .Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
            if (rawSubmissionStatus == "completed" || rawSubmissionStatus == "complete" || rawSubmissionStatus == "approved") return "completed";
            if (rawSubmissionStatus == "pending" || rawSubmissionStatus == "submitted" || rawSubmissionStatus == "done"
                || rawSubmissionStatus == "underreview" || rawSubmissionStatus == "inreview" || rawSubmissionStatus == "pendingreview") return "done";
            if (rawSubmissionStatus == "inprogress" || rawSubmissionStatus == "progress") return "inProgress";
            return taskStatus;
        }

        private static JToken GetToken(JObject obj, string name)
        {
            return obj == null ? null : obj.Properties().FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        private static string Value(JObject obj, string name)
        {
            var token = GetToken(obj, name);
            return token == null || token.Type == JTokenType.Null ? string.Empty : (string)token ?? string.Empty;
        }

        private static long TokenLong(JObject obj, string name)
        {
            long value;
            return long.TryParse(Value(obj, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0L;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
        }

        private static bool IsSafeKey(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?' }) < 0;
        }

        private static bool IsValidTaskNumber(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Length <= 40
                && value.All(character => char.IsLetterOrDigit(character) || character == '-' || character == '_');
        }

        private static bool IsHttpUrl(string value)
        {
            Uri uri;
            return Uri.TryCreate((value ?? string.Empty).Trim(), UriKind.Absolute, out uri)
                && uri.Scheme == Uri.UriSchemeHttps;
        }

        private static string NormalizeRecordStatus(string value)
        {
            return (value ?? string.Empty).Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
        }

        private static bool IsGitHubSolutionUrl(string value)
        {
            Uri uri;
            if (!Uri.TryCreate((value ?? string.Empty).Trim(), UriKind.Absolute, out uri) || uri.Scheme != Uri.UriSchemeHttps) return false;
            var host = uri.Host.ToLowerInvariant();
            return (host == "github.com" || host == "www.github.com" || host == "gist.github.com") && uri.AbsolutePath.Trim('/').Length > 0;
        }

        private static string NormalizeStatus(string value)
        {
            var normalized = (value ?? string.Empty).Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
            if (normalized == "completed" || normalized == "complete" || normalized == "approved") return "completed";
            if (normalized == "done" || normalized == "submitted" || normalized == "underreview" || normalized == "inreview" || normalized == "pendingreview") return "done";
            if (normalized == "inprogress" || normalized == "progress") return "inProgress";
            return "todo";
        }

        private static string LabelForStatus(string status)
        {
            if (status == "done") return "Done · Under review";
            if (status == "completed") return "Completed";
            if (status == "inProgress") return "In Progress";
            return "To do";
        }

        private static string FormatDate(long unixMs)
        {
            try { return DateTimeOffset.FromUnixTimeMilliseconds(unixMs).ToOffset(TimeSpan.FromHours(3)).ToString("dd MMM yyyy", CultureInfo.InvariantCulture); }
            catch (ArgumentOutOfRangeException) { return "—"; }
        }

        private void ShowError(string message)
        {
            PageMessage.Text = "<div class=\"page-alert error\" role=\"alert\">" + HttpUtility.HtmlEncode(message) + "</div>";
        }

        private void ShowSuccess(string message)
        {
            PageMessage.Text = "<div class=\"page-alert success\" role=\"status\">" + HttpUtility.HtmlEncode(message) + "</div>";
        }
    }
}
