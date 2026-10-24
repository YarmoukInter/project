using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.AcademicPages
{
    public partial class Tasks : SupervisorPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadTasksAsync));
        }

        private async System.Threading.Tasks.Task LoadTasksAsync()
        {
            try
            {
                var auth = GetSupervisorSession("academic");
                var service = new SupervisorWorkspaceService();
                var students = await service.GetAcademicStudentsAsync(auth.Uid, auth.IdToken);
                var studentsById = students.GroupBy(x => x.StudentUid).ToDictionary(x => x.Key, x => x.First());
                var profileStudents = await new FirebaseHelper().GetAsync<System.Collections.Generic.Dictionary<string, Student>>(DatabasePaths.Students, auth.IdToken)
                    ?? new System.Collections.Generic.Dictionary<string, Student>();
                var data = await new FirebaseHelper().GetAsync<JObject>(DatabasePaths.Tasks, auth.IdToken);
                var rows = (data == null ? Enumerable.Empty<JProperty>() : data.Properties())
                    .Select(x => x.Value as JObject).Where(x => x != null && studentsById.ContainsKey((string)x["studentUid"] ?? string.Empty))
                    .Select(x => new TaskItem
                    {
                        StudentName = studentsById[(string)x["studentUid"]].StudentName,
                        Initials = GetInitials(studentsById[(string)x["studentUid"]].StudentName),
                        ProfileImageUrl = StudentProfileImages.GetUrl(profileStudents.ContainsKey((string)x["studentUid"])
                            ? profileStudents[(string)x["studentUid"]].ProfileImageFileName : null),
                        Title = (string)x["title"] ?? "Untitled task",
                        Description = (string)x["description"] ?? string.Empty,
                        StatusKey = Normalize((string)x["status"]),
                        DueLabel = FormatDate((long?)x["dueAtUnixMs"]),
                        TaskUrl = SafeUrl((string)x["taskPdfUrl"])
                    }).OrderBy(x => x.StudentName).ThenBy(x => x.Title).ToList();
                foreach (var row in rows) row.StatusLabel = StatusLabel(row.StatusKey);
                TaskList.DataSource = rows;
                TaskList.DataBind();
                TasksCount.Text = rows.Count.ToString(CultureInfo.InvariantCulture);
                TasksEmpty.Visible = rows.Count == 0;
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Academic tasks load failed: " + error.GetType().Name);
                TasksErrorText.Text = HttpUtility.HtmlEncode("Tasks could not be loaded. Refresh and try again.");
                TasksError.Visible = true;
            }
        }
        private static string Normalize(string value) { return (value ?? "todo").Replace(" ", "").ToLowerInvariant(); }
        private static string GetInitials(string name)
        {
            var parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "ST";
            return parts.Length == 1 ? parts[0].Substring(0, 1).ToUpperInvariant()
                : (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }
        private static string StatusLabel(string value)
        {
            if (value == "inprogress") return "In progress";
            if (value == "completed") return "Completed";
            if (value == "done") return "Submitted";
            return "To do";
        }
        private static string FormatDate(long? value)
        { return value.HasValue && value.Value > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(value.Value).ToString("d MMM yyyy", CultureInfo.InvariantCulture) : "Not set"; }
        private static string SafeUrl(string value)
        { Uri uri; return Uri.TryCreate(value, UriKind.Absolute, out uri) && uri.Scheme == Uri.UriSchemeHttps ? uri.AbsoluteUri : string.Empty; }
        private sealed class TaskItem
        {
            public string StudentName { get; set; }
            public string Initials { get; set; }
            public string ProfileImageUrl { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string StatusKey { get; set; }
            public string StatusLabel { get; set; }
            public string DueLabel { get; set; }
            public string TaskUrl { get; set; }
        }
    }
}
