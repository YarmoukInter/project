using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Yarmouk.Helpers
{
    public static class TrainingProgress
    {
        public const int RequiredTasks = 10;

        public static int Percent(int submittedTasks)
        {
            return Math.Max(0, Math.Min(RequiredTasks, submittedTasks)) * 10;
        }

        public static string Label(int submittedTasks)
        {
            return submittedTasks + " of " + RequiredTasks + " tasks submitted";
        }

        // Count distinct assigned tasks, so revisions and duplicate submissions never add credit.
        public static int CountSubmitted(IEnumerable<KeyValuePair<string, JObject>> tasks,
            IEnumerable<JObject> submissions, string studentUid, string placementId = null, string opportunityId = null)
        {
            var submittedIds = new HashSet<string>((submissions ?? Enumerable.Empty<JObject>())
                .Where(s => s != null && Text(s, "studentUid") == studentUid)
                .Select(s => Text(s, "taskId")), StringComparer.Ordinal);
            return (tasks ?? Enumerable.Empty<KeyValuePair<string, JObject>>())
                .Where(t => t.Value != null && Text(t.Value, "studentUid") == studentUid)
                .Where(t => string.IsNullOrEmpty(placementId) || string.IsNullOrEmpty(Text(t.Value, "placementId"))
                    || Text(t.Value, "placementId") == placementId)
                .Where(t => string.IsNullOrEmpty(opportunityId) || string.IsNullOrEmpty(Text(t.Value, "opportunityId"))
                    || Text(t.Value, "opportunityId") == opportunityId)
                .Where(t => submittedIds.Contains(t.Key) || submittedIds.Contains(Text(t.Value, "id"))
                    || IsSubmittedStatus(Text(t.Value, "status")))
                .Select(t => t.Key).Distinct(StringComparer.Ordinal).Count();
        }

        private static bool IsSubmittedStatus(string status)
        {
            string value = (status ?? "").Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
            return value == "submitted" || value == "done" || value == "inreview" || value == "underreview"
                || value == "pendingreview" || value == "completed" || value == "complete" || value == "approved";
        }

        private static string Text(JObject value, string key)
        {
            return value == null ? "" : (string)value[key] ?? "";
        }
    }
}
