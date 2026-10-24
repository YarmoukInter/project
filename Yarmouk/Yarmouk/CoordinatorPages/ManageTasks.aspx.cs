using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class ManageTasks : CoordinatorPage
    {
        protected override async Task<object> LoadDataAsync(AuthSession session)
        {
            if (session == null || session.ExpiresUtc <= DateTime.UtcNow || session.User == null
                || !session.User.IsActive || AuthPolicy.NormalizeRole(session.User.Role) != "coordinator")
                throw new UnauthorizedAccessException("Sign in with an active coordinator account.");

            var firebase = new FirebaseHelper();
            var tasksRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Tasks, session.IdToken);
            var submissionsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TaskSubmissions, session.IdToken);
            var studentsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Students, session.IdToken);
            var usersRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Users, session.IdToken);
            var placementsRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Placements, session.IdToken);
            var companiesRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Companies, session.IdToken);
            var opportunitiesRequest = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Opportunities, session.IdToken);
            await Task.WhenAll(tasksRequest, submissionsRequest, studentsRequest, usersRequest,
                placementsRequest, companiesRequest, opportunitiesRequest).ConfigureAwait(false);

            var rows = BuildRows(await tasksRequest.ConfigureAwait(false), await submissionsRequest.ConfigureAwait(false),
                await studentsRequest.ConfigureAwait(false), await usersRequest.ConfigureAwait(false),
                await placementsRequest.ConfigureAwait(false), await companiesRequest.ConfigureAwait(false),
                await opportunitiesRequest.ConfigureAwait(false));
            return new { tasks = rows };
        }

        private static List<TaskRow> BuildRows(IDictionary<string, JObject> tasks,
            IDictionary<string, JObject> submissions, IDictionary<string, JObject> students,
            IDictionary<string, JObject> users, IDictionary<string, JObject> placements,
            IDictionary<string, JObject> companies, IDictionary<string, JObject> opportunities)
        {
            var rows = new List<TaskRow>();
            if (tasks == null) return rows;
            foreach (var entry in tasks)
            {
                var task = entry.Value;
                if (task == null) continue;
                var taskId = First(Text(task, "id"), entry.Key);
                var studentUid = Text(task, "studentUid");
                JObject student = Find(students, studentUid);
                JObject user = Find(users, studentUid);
                var studentName = First(Text(user, "fullName"), Text(user, "name"),
                    Text(student, "englishName"), Text(student, "arabicName"), Text(student, "name"), "Student");
                var studentNumber = First(Text(student, "studentNumber"), Text(student, "studentId"), Text(user, "studentNumber"));
                if (string.IsNullOrWhiteSpace(Text(student, "profileImageFileName")))
                {
                    var matchedStudent = FindStudentByNumber(students, studentNumber);
                    if (matchedStudent != null) student = matchedStudent;
                }
                var profileImageUrl = GetProfileImageUrl(First(
                    Text(student, "profileImageFileName"), Text(user, "profileImageFileName")));

                var placementId = Text(task, "placementId");
                var placement = Find(placements, placementId);
                if (placement == null && placements != null)
                    placement = placements.Values.FirstOrDefault(item => item != null
                        && Text(item, "studentUid") == studentUid);
                var company = Find(companies, First(Text(task, "companyId"), Text(placement, "companyId")));
                var opportunity = Find(opportunities, First(Text(task, "opportunityId"), Text(placement, "opportunityId")));
                var taskTitle = First(Text(task, "title"), Text(task, "taskNumber"), "Untitled task");
                var submission = LatestSubmission(submissions, taskId, studentUid);
                var reviewStatus = submission == null ? "Not submitted"
                    : Friendly(First(Text(submission, "reviewStatus"), Text(task, "reviewStatus"), "Waiting review"));
                var submittedAt = Number(submission, "submittedAtUnixMs");
                rows.Add(new TaskRow
                {
                    StudentName = studentName,
                    StudentNumber = First(studentNumber, "—"),
                    StudentInitials = GetInitials(studentName),
                    ProfileImageUrl = profileImageUrl,
                    Company = First(Text(company, "name"), "Not assigned"),
                    Title = taskTitle,
                    Description = Text(task, "description"),
                    Deadline = FormatDate(Number(task, "dueAtUnixMs"), First(Text(task, "dueDate"), "Not set")),
                    TaskStatus = Friendly(First(Text(task, "status"), "To do")),
                    Submission = submission == null ? "Not submitted" : "Submitted",
                    SubmittedAt = submittedAt > 0 ? FormatDate(submittedAt, "") : "—",
                    ReviewStatus = reviewStatus,
                    CreatedAt = Number(task, "createdAtUnixMs")
                });
            }
            return rows.OrderByDescending(row => row.CreatedAt).ThenBy(row => row.StudentName, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static JObject LatestSubmission(IDictionary<string, JObject> submissions, string taskId, string studentUid)
        {
            if (submissions == null) return null;
            return submissions.Values.Where(item => item != null
                    && Text(item, "taskId") == taskId
                    && (string.IsNullOrWhiteSpace(Text(item, "studentUid")) || Text(item, "studentUid") == studentUid))
                .OrderByDescending(item => Number(item, "submittedAtUnixMs"))
                .FirstOrDefault();
        }

        private static JObject Find(IDictionary<string, JObject> records, string id)
        {
            JObject result;
            return records != null && !string.IsNullOrWhiteSpace(id) && records.TryGetValue(id, out result) ? result : null;
        }

        private static JObject FindStudentByNumber(IDictionary<string, JObject> students, string studentNumber)
        {
            if (students == null || string.IsNullOrWhiteSpace(studentNumber)) return null;
            return students.Values.FirstOrDefault(item => item != null
                && string.Equals(First(Text(item, "studentNumber"), Text(item, "studentId")),
                    studentNumber, StringComparison.OrdinalIgnoreCase));
        }

        private static string Text(JObject source, string key)
        {
            var property = source == null ? null : source.Properties()
                .FirstOrDefault(item => string.Equals(item.Name, key, StringComparison.OrdinalIgnoreCase));
            var value = property == null ? null : property.Value;
            return value == null || value.Type == JTokenType.Null ? string.Empty : value.ToString().Trim();
        }

        private static long Number(JObject source, string key)
        {
            long value;
            return long.TryParse(Text(source, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? value : 0;
        }

        private static string First(params string[] values)
        { return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty; }

        private static string Friendly(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Unknown";
            var words = status.Replace('_', ' ').Replace('-', ' ').Trim();
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words.ToLowerInvariant());
        }

        private static string FormatDate(long unixMilliseconds, string fallback)
        {
            if (unixMilliseconds <= 0) return fallback ?? string.Empty;
            try { return DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).ToLocalTime().ToString("dd MMM yyyy", CultureInfo.InvariantCulture); }
            catch (ArgumentOutOfRangeException) { return fallback ?? string.Empty; }
        }

        private static string GetInitials(string name)
        {
            var parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "—";
            return parts.Length == 1 ? parts[0].Substring(0, 1).ToUpperInvariant()
                : (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static string GetProfileImageUrl(string fileName)
        {
            var safeName = Path.GetFileName(fileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(safeName)
                || !string.Equals(safeName, fileName, StringComparison.Ordinal)
                || !new[] { ".png", ".jpg", ".jpeg" }.Contains(Path.GetExtension(safeName), StringComparer.OrdinalIgnoreCase)) return string.Empty;
            return System.Web.VirtualPathUtility.ToAbsolute("~/Uploads/StudentProfileImages/") + Uri.EscapeDataString(safeName);
        }

        private sealed class TaskRow
        {
            public string StudentName { get; set; }
            public string StudentNumber { get; set; }
            public string StudentInitials { get; set; }
            public string ProfileImageUrl { get; set; }
            public string Company { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Deadline { get; set; }
            public string TaskStatus { get; set; }
            public string Submission { get; set; }
            public string SubmittedAt { get; set; }
            public string ReviewStatus { get; set; }
            public long CreatedAt { get; set; }
        }
    }
}
