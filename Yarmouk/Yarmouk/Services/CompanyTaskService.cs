using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;

namespace Yarmouk.Services
{
    public sealed class CompanyTaskService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();

        private async Task<JObject> RequireManagedTaskAsync(string taskId, string uid, string token)
        {
            if (string.IsNullOrWhiteSpace(taskId) || taskId.Length > 256
                || taskId.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '\\', '?', '&' }) >= 0)
                throw new ArgumentException("Select a valid task.");
            var user = await new StudentService().GetUserAsync(uid, token);
            if (!AuthPolicy.ValidUser(user, uid) || AuthPolicy.NormalizeRole(user.Role) != "company")
                throw new UnauthorizedAccessException("An active company account is required.");
            var task = await firebase.GetAsync<JObject>(DatabasePaths.Tasks + "/" + taskId, token);
            if (task == null) throw new ArgumentException("This task is no longer available. Refresh the list.");
            var trainees = await new SupervisorWorkspaceService().GetCompanyTraineesAsync(uid, token);
            if (!trainees.Any(t => t.StudentUid == (string)task["studentUid"]
                && (string.IsNullOrWhiteSpace((string)task["placementId"]) || t.PlacementId == (string)task["placementId"])))
                throw new UnauthorizedAccessException("You do not have access to this task.");
            string companyId = await new CompanySupervisorService().GetCompanyIdForSupervisorAsync(uid, token);
            string taskCompany = null;
            if (!string.IsNullOrWhiteSpace((string)task["opportunityId"])) {
                var opportunity = await firebase.GetAsync<Opportunity>(DatabasePaths.Opportunities + "/" + (string)task["opportunityId"], token);
                taskCompany = opportunity == null ? null : opportunity.CompanyId;
            }
            if (string.IsNullOrWhiteSpace(taskCompany) && !string.IsNullOrWhiteSpace((string)task["placementId"])) {
                var placement = await firebase.GetAsync<Placement>(DatabasePaths.Placements + "/" + (string)task["placementId"], token);
                taskCompany = placement == null ? null : placement.CompanyId;
            }
            if (taskCompany != companyId && !(string.IsNullOrWhiteSpace(taskCompany) && (string)task["supervisorUid"] == uid))
                throw new UnauthorizedAccessException("This task belongs to another company.");
            return task;
        }

        public async Task UpdateAsync(string taskId, string uid, string token, string number,
            string title, string description, string taskUrl, string deadline)
        {
            number = (number ?? "").Trim(); title = (title ?? "").Trim();
            description = (description ?? "").Trim(); taskUrl = (taskUrl ?? "").Trim();
            if (number.Length == 0 || number.Length > 40 || number.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
                throw new ArgumentException("Enter a task number up to 40 letters, numbers, hyphens or underscores.");
            if (title.Length < 2 || title.Length > 160) throw new ArgumentException("Enter a title between 2 and 160 characters.");
            if (description.Length < 2 || description.Length > 4000) throw new ArgumentException("Enter instructions between 2 and 4,000 characters.");
            Uri link;
            if (taskUrl.Length > 2048 || !Uri.TryCreate(taskUrl, UriKind.Absolute, out link) || link.Scheme != Uri.UriSchemeHttps)
                throw new ArgumentException("Enter a valid HTTPS task link.");
            DateTime date;
            if (!DateTime.TryParseExact(deadline, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                throw new ArgumentException("Choose a valid deadline.");
            var original = await RequireManagedTaskAsync(taskId, uid, token);
            long due = new DateTimeOffset(date, TimeSpan.FromHours(3)).ToUnixTimeMilliseconds();
            long oldDue = (long?)original["dueAtUnixMs"] ?? 0;
            string oldDate = oldDue > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(oldDue).ToOffset(TimeSpan.FromHours(3)).ToString("yyyy-MM-dd") : "";
            if (deadline != oldDate && due <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                throw new ArgumentException("A changed deadline must be in the future (Jordan time).");
            await firebase.TransactAsync(DatabasePaths.Tasks + "/" + taskId, token, current => {
                if (current == null || (string)current["studentUid"] != (string)original["studentUid"]
                    || (string)current["placementId"] != (string)original["placementId"]
                    || (string)current["opportunityId"] != (string)original["opportunityId"])
                    throw new InvalidOperationException("This assignment changed. Refresh before editing it.");
                current["taskNumber"] = number; current["title"] = title;
                current["description"] = description; current["taskPdfUrl"] = taskUrl;
                // Preserve the original instant when editing a task with an unchanged date.
                current["dueAtUnixMs"] = deadline == oldDate ? oldDue : due;
                current["updatedByUid"] = uid;
                current["updatedAtUnixMs"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return true;
            });
        }

        public async Task DeleteAsync(string taskId, string uid, string token)
        {
            var task = await RequireManagedTaskAsync(taskId, uid, token);
            var submissions = await firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TaskSubmissions, token)
                ?? new Dictionary<string, JObject>();
            var related = submissions.Where(s => s.Value != null && (string)s.Value["studentUid"] == (string)task["studentUid"]
                && ((string)s.Value["taskId"] == taskId || (!string.IsNullOrEmpty((string)task["id"])
                    && (string)s.Value["taskId"] == (string)task["id"])))
                .ToDictionary(s => s.Key, s => s.Value);
            var changes = new Dictionary<string, object> {
                [DatabasePaths.DeletedTasks + "/" + taskId] = new {
                    task, submissions = related, deletedByUid = uid,
                    deletedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                },
                [DatabasePaths.Tasks + "/" + taskId] = null
            };
            foreach (string id in related.Keys) changes[DatabasePaths.TaskSubmissions + "/" + id] = null;
            // Archive and remove the active task/submissions in one multi-path save.
            await firebase.UpdateRootAsync(changes, token);
        }
    }
}
