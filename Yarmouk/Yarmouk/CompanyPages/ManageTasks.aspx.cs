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
        private sealed class PlacementOption { public string Id; public string StudentUid; public string StudentName; }
        private sealed class TaskRow
        {
            public string StudentUid;
            public string StudentName;
            public string ProfileImageUrl;
            public string StudentInitials;
            public string Id;
            public string Number;
            public string Title;
            public string Status;
            public string StatusLabel;
            public string SolutionUrl;
            public string SubmissionNotes;
            public int? Grade;
            public long DueAt;
            public long CreatedAt;
            public string PlacementId;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["placementId"] != null) { RedirectToFilters(Request.QueryString["studentUid"], Request.QueryString["status"], Request.QueryString["q"]); return; }
                RegisterAsyncTask(new PageAsyncTask(() => LoadPageAsync(
                    SafeFilter(Request.QueryString["studentUid"]),
                    SafeFilter(Request.QueryString["status"]), (Request.QueryString["q"] ?? string.Empty).Trim())));
                return;
            }

            var addTask = Request.Form["addTaskAction"];
            var action = Request.Form["taskAction"];
            var selectedStudentUid = Request.Form[StudentSelector.UniqueID];

            var selectedStatus = Request.Form[TaskStatusSelector.UniqueID];
            var searchText = Request.Form[TaskSearchTextBox.UniqueID];
            if (Request.Form["filterTasksAction"] == "1")
            {
                RedirectToFilters(selectedStudentUid, selectedStatus, searchText);
                return;
            }
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                if (addTask == "1")
                    await CreateTaskAsync(selectedStudentUid);
                else if (!string.IsNullOrWhiteSpace(action))
                    await ProcessDecisionAsync(action, selectedStudentUid);
                await LoadPageAsync(selectedStudentUid, selectedStatus, searchText);
            }));
        }

        private void RedirectToFilters(string studentUid, string status, string search)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            studentUid = SafeFilter(studentUid);

            status = SafeFilter(status);
            search = (search ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(studentUid)) query["studentUid"] = studentUid;

            if (!string.IsNullOrWhiteSpace(status)) query["status"] = status;
            if (!string.IsNullOrWhiteSpace(search)) query["q"] = search.Length > 120 ? search.Substring(0, 120) : search;
            string url = ResolveUrl("~/CompanyPages/ManageTasks");
            string encoded = query.ToString();
            Response.Redirect(string.IsNullOrWhiteSpace(encoded) ? url : url + "?" + encoded, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private string GetFirebaseUrl(string path, AuthSession auth)
        {
            var databaseUrl = Yarmouk.Helpers.SiteSettingsStore.GetFirebaseDatabaseUrl();
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

        private async Task LoadPageAsync(string requestedStudentUid, string requestedStatus, string requestedSearch)
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
                var trainees = await new SupervisorWorkspaceService().GetCompanyTraineesAsync(auth.Uid, auth.IdToken);
                var assignedUids = new HashSet<string>(trainees.Select(x => x.StudentUid), StringComparer.Ordinal);
                var options = GetStudentOptions(assignedUids, students, users);
                var placementOptions = trainees.Where(item => !string.IsNullOrWhiteSpace(item.PlacementId))
                    .Select(item => new PlacementOption { Id = item.PlacementId, StudentUid = item.StudentUid, StudentName = item.StudentName })
                    .GroupBy(item => item.Id, StringComparer.Ordinal).Select(group => group.First())
                    .OrderBy(item => item.StudentName, StringComparer.CurrentCultureIgnoreCase).ToList();
                StudentChoices.Text = RenderStudentChoices(options);

                StudentSelector.Items.Clear();
                StudentSelector.Items.Add(new ListItem(options.Count == 0 ? "No assigned students" : "All trainees", ""));
                foreach (var option in options) StudentSelector.Items.Add(new ListItem(option.Name, option.Uid));
                var selected = options.FirstOrDefault(item => item.Uid == requestedStudentUid);
                if (selected != null) StudentSelector.SelectedValue = selected.Uid;



                string statusFilter = string.IsNullOrWhiteSpace(requestedStatus) ? string.Empty : NormalizeStatus(requestedStatus);
                if (TaskStatusSelector.Items.FindByValue(statusFilter) != null) TaskStatusSelector.SelectedValue = statusFilter;
                else statusFilter = string.Empty;
                string search = (requestedSearch ?? string.Empty).Trim();
                if (search.Length > 120) search = search.Substring(0, 120);
                TaskSearchTextBox.Text = search;

                if (options.Count == 0)
                {
                    TaskCount.Text = "";
                    TaskListContext.Text = "No active company placements are available.";
                    TaskTable.Text = "<p class=\"empty-state\">No trainees are currently assigned to your company.</p>";
                    return;
                }

                var placementByStudent = placementOptions.GroupBy(item => item.StudentUid, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First().Id, StringComparer.Ordinal);
                var rows = options.SelectMany(option => BuildTaskRows(option.Uid, taskData, submissionData,
                        placementByStudent.ContainsKey(option.Uid) ? placementByStudent[option.Uid] : string.Empty)
                    .Select(row => { var trainee = trainees.FirstOrDefault(item => item.StudentUid == option.Uid); row.StudentUid = option.Uid; row.StudentName = option.Name; row.ProfileImageUrl = trainee == null ? string.Empty : trainee.ProfileImageUrl; row.StudentInitials = GetInitials(option.Name); return row; }))
                    .Where(row => selected == null || row.StudentUid == selected.Uid)

                    .Where(row => string.IsNullOrWhiteSpace(statusFilter) || row.Status == statusFilter)
                    .Where(row => string.IsNullOrWhiteSpace(search) || string.Join(" ", row.StudentName, row.Number, row.Title)
                        .IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .OrderByDescending(row => row.CreatedAt).ThenBy(row => row.Id, StringComparer.Ordinal).ToList();
                TaskCount.Text = "<span class=\"task-count\">" + rows.Count + (rows.Count == 1 ? " task" : " tasks") + "</span>";
                TaskListContext.Text = selected != null || !string.IsNullOrWhiteSpace(statusFilter) || !string.IsNullOrWhiteSpace(search)
                    ? "Showing tasks that match the selected filters."
                    : "Showing every task assigned by this company, newest first.";
                TaskTable.Text = RenderTaskTable(rows, selected != null ||
                    !string.IsNullOrWhiteSpace(statusFilter) || !string.IsNullOrWhiteSpace(search));
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
                var assignedUids = new HashSet<string>((await new SupervisorWorkspaceService().GetCompanyTraineesAsync(auth.Uid, auth.IdToken)).Select(x => x.StudentUid), StringComparer.Ordinal);
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
                    int grade;
                    if (!int.TryParse(Request.Form["taskScore"], NumberStyles.None, CultureInfo.InvariantCulture, out grade)
                        || grade < 0 || grade > 10)
                    { ShowError("Enter a task evaluation from 0 to 10 before approving."); return; }
                    await PatchFirebaseObjectAsync(DatabasePaths.Tasks + "/" + entry.Name, new JObject
                    {
                        ["status"] = "completed", ["completedAtUnixMs"] = now, ["reviewedByUid"] = auth.Uid,
                        ["grade"] = grade,
                        ["instructorFeedback"] = JValue.CreateNull()
                    }, auth);
                    if (latest != null)
                        await PatchFirebaseObjectAsync(DatabasePaths.TaskSubmissions + "/" + latest.Name, new JObject
                        {
                            ["status"] = "completed", ["reviewStatus"] = "completed", ["reviewedByUid"] = auth.Uid,
                            ["reviewedAtUnixMs"] = now, ["grade"] = grade, ["instructorFeedback"] = JValue.CreateNull()
                        }, auth);
                    ShowSuccess("Task approved. It is now marked Completed for the student.");
                    return;
                }

                var feedback = (Request.Form["reviewFeedback"] ?? string.Empty).Trim();
                var deadlineText = (Request.Form["reviewDeadline"] ?? string.Empty).Trim();
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
                await PatchFirebaseObjectAsync(DatabasePaths.Tasks + "/" + entry.Name, new JObject
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
                var assignedTrainees = await new SupervisorWorkspaceService().GetCompanyTraineesAsync(auth.Uid, auth.IdToken);
                var assignedUids = new HashSet<string>(assignedTrainees.Select(x => x.StudentUid), StringComparer.Ordinal);
                var placementByStudent = assignedTrainees.Where(x => !string.IsNullOrWhiteSpace(x.PlacementId))
                    .GroupBy(x => x.StudentUid, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.First().PlacementId, StringComparer.Ordinal);
                if (targetUids.Any(uid => !assignedUids.Contains(uid)))
                { ShowError("Only students assigned to your company can receive this task."); return; }

                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var additions = new JObject();
                foreach (var uid in targetUids)
                {
                    var opportunityId = await new StudentService().GetActiveAcceptedOpportunityIdAsync(uid, auth.IdToken);
                    if (string.IsNullOrWhiteSpace(opportunityId))
                    { ShowError("Every selected trainee must have an accepted, active internship opportunity before receiving tasks."); return; }
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
                        ["placementId"] = placementByStudent.ContainsKey(uid) ? placementByStudent[uid] : string.Empty,
                        ["opportunityId"] = opportunityId,
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

        private static string GetInitials(string name)
        {
            var parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "ST";
            return parts.Length == 1 ? parts[0].Substring(0, 1).ToUpperInvariant()
                : (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
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

        private static List<TaskRow> BuildTaskRows(string studentUid, JObject tasks, JObject submissions, string fallbackPlacementId)
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
                var submitted = latest == null ? task : latest.Value as JObject;
                int grade;
                var gradeText = FirstNonEmpty(Value(task, "grade"), Value(submitted, "grade"), Value(task, "score"), Value(submitted, "score"));
                rows.Add(new TaskRow
                {
                    Id = taskId, Number = FirstNonEmpty(Value(task, "taskNumber"), taskId),
                    Title = FirstNonEmpty(Value(task, "title"), "Untitled task"), Status = status,
                    StatusLabel = LabelForStatus(status), SolutionUrl = IsGitHubSolutionUrl(url) ? url : string.Empty,
                    SubmissionNotes = FirstNonEmpty(Value(submitted, "notes"), Value(submitted, "submissionNotes"), Value(task, "submissionNotes")),
                    Grade = int.TryParse(gradeText, NumberStyles.None, CultureInfo.InvariantCulture, out grade)
                        && grade >= 0 && grade <= 10 ? (int?)grade : null,
                    DueAt = TokenLong(task, "dueAtUnixMs"), CreatedAt = TokenLong(task, "createdAtUnixMs")
                    , PlacementId = FirstNonEmpty(Value(task, "placementId"), fallbackPlacementId)
                });
            }
            return rows.OrderBy(row => row.CreatedAt).ThenBy(row => row.Id, StringComparer.Ordinal).ToList();
        }

        private static string RenderTaskTable(List<TaskRow> rows, bool hasFilters)
        {
            if (rows.Count == 0) return "<div class=\"empty-state task-empty-state\"><strong>" +
                (hasFilters ? "No tasks match these filters." : "No tasks have been created for this company yet.") +
                "</strong><span>" + (hasFilters ? "Change a filter or use Clear all to return to the complete task list." : "Use Add task to create the first trainee assignment.") + "</span></div>";
            var html = new StringBuilder("<div class=\"task-card-list\" aria-label=\"Trainee tasks\">");
            foreach (var row in rows)
            {
                var id = HttpUtility.HtmlAttributeEncode(row.Id);
                html.Append("<article class=\"task-review-card\" data-status=\"").Append(HttpUtility.HtmlAttributeEncode(row.Status)).Append("\">")
                    .Append("<div class=\"task-review-card__student\"><span class=\"task-card-label\">Student</span><span class=\"task-review-student uims-student-photo-avatar\" data-student-photo-url=\"").Append(HttpUtility.HtmlAttributeEncode(row.ProfileImageUrl)).Append("\"><span>").Append(HttpUtility.HtmlEncode(row.StudentInitials)).Append("</span></span><strong class=\"task-card-student\">").Append(HttpUtility.HtmlEncode(row.StudentName)).Append("</strong></div>")
                    .Append("<div class=\"task-review-card__task\"><span class=\"task-card-label\">Task</span><strong>").Append(HttpUtility.HtmlEncode(row.Number)).Append("</strong><span class=\"task-card-title\">").Append(HttpUtility.HtmlEncode(row.Title)).Append("</span></div>")
                    .Append("<div class=\"task-review-card__deadline\"><span class=\"task-card-label\">Deadline</span><time class=\"task-card-deadline\">").Append(row.DueAt > 0 ? HttpUtility.HtmlEncode(FormatDate(row.DueAt)) : "—").Append("</time></div>")
                    .Append("<div class=\"task-review-card__status\"><span class=\"task-card-label\">Status</span><span class=\"status-pill status-").Append(HttpUtility.HtmlAttributeEncode(row.Status)).Append("\">").Append(HttpUtility.HtmlEncode(row.StatusLabel)).Append("</span></div>")
                    .Append("<div class=\"task-review-card__grade\"><span class=\"task-card-label\">Evaluation</span><span class=\"task-grade\">").Append(row.Grade.HasValue ? row.Grade.Value.ToString(CultureInfo.InvariantCulture) : "—").Append(" <small>/ 10</small></span></div><div class=\"task-review-card__action\">");
                if (row.Status == "done" || row.Status == "completed")
                    html.Append("<button type=\"button\" class=\"review-open-button\" data-review-id=\"").Append(id)
                        .Append("\" data-review-student-uid=\"").Append(HttpUtility.HtmlAttributeEncode(row.StudentUid))
                        .Append("\" data-review-title=\"").Append(HttpUtility.HtmlAttributeEncode(row.Title))
                        .Append("\" data-review-status=\"").Append(HttpUtility.HtmlAttributeEncode(row.Status))
                        .Append("\" data-review-status-label=\"").Append(HttpUtility.HtmlAttributeEncode(row.StatusLabel))
                        .Append("\" data-review-solution=\"").Append(HttpUtility.HtmlAttributeEncode(row.SolutionUrl))
                        .Append("\" data-review-notes=\"").Append(HttpUtility.HtmlAttributeEncode(row.SubmissionNotes))
                        .Append("\" data-review-grade=\"").Append(row.Grade.HasValue ? row.Grade.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)
                        .Append("\">").Append(row.Status == "done" ? "Review submission" : "View review").Append("</button>");
                else html.Append("<span class=\"muted\">Available after submission</span>");
                html.Append("</div></article>");
            }
            html.Append("</div>");
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
            if (taskStatus != "todo") return taskStatus;
            if (latestSubmission == null)
                return TokenLong(task, "submittedAtUnixMs") > 0
                    && IsGitHubSolutionUrl(Value(task, "submissionUrl"))
                    && string.IsNullOrWhiteSpace(Value(task, "instructorFeedback")) ? "done" : taskStatus;
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

        private static string SafeFilter(string value)
        {
            value = (value ?? string.Empty).Trim();
            return value.Length <= 128 && (value.Length == 0 || IsSafeKey(value)) ? value : string.Empty;
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

