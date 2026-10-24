using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;

namespace Yarmouk.Services
{
    public sealed class CompletionInboxService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        private async Task RequireStudentAsync(string uid, string token)
        {
            CompletionService.ValidateKey(uid);
            var user = await firebase.GetAsync<JObject>("users/" + uid, token);
            if (user == null || (bool?)user["isActive"] != true || (string)user["role"] != "student")
                throw new UnauthorizedAccessException("A student account is required.");
        }
        public async Task<List<JObject>> ListAsync(string uid, string token)
        {
            await RequireStudentAsync(uid, token);
            var reports = await firebase.GetAsync<Dictionary<string, JObject>>("completionReports/" + uid, token);
            return (reports ?? new Dictionary<string, JObject>()).Where(pair => pair.Value != null && (string)pair.Value["studentUid"] == uid)
                .Select(pair => new JObject {
                    ["id"] = pair.Key, ["title"] = (string)pair.Value["title"], ["studentName"] = (string)pair.Value["studentName"],
                    ["companyName"] = (string)pair.Value["companyName"], ["finalScore"] = pair.Value["finalScore"],
                    ["approvedHours"] = pair.Value["approvedHours"], ["requiredHours"] = pair.Value["requiredHours"],
                    ["sentAtUnixMs"] = pair.Value["sentAtUnixMs"], ["read"] = pair.Value["readAtUnixMs"] != null
                }).OrderByDescending(report => (long?)report["sentAtUnixMs"] ?? 0).ToList();
        }
        private async Task<JObject> OwnReportAsync(string uid, string id, string token)
        {
            await RequireStudentAsync(uid, token);
            CompletionService.ValidateKey(id);
            var report = await firebase.GetAsync<JObject>("completionReports/" + uid + "/" + id, token);
            if (report == null || (string)report["studentUid"] != uid) throw new UnauthorizedAccessException("Report not found in your inbox.");
            return report;
        }
        public async Task<byte[]> GetPdfAsync(string uid, string id, string token)
        {
            var report = await OwnReportAsync(uid, id, token);
            return Convert.FromBase64String((string)report["dataBase64"]);
        }
        public async Task MarkReadAsync(string uid, string id, string token)
        {
            await OwnReportAsync(uid, id, token);
            await firebase.UpdateAsync("completionReports/" + uid + "/" + id,
                new { readAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, token);
        }
    }
}
