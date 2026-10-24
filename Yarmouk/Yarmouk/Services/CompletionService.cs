using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;

namespace Yarmouk.Services
{
    public sealed class CompletionService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        public static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 128 || key.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?', '&' }) >= 0)
                throw new ArgumentException("Select a valid evaluation record.");
        }

        private async Task RequireCoordinatorAsync(string uid, string token)
        {
            ValidateKey(uid);
            var user = await firebase.GetAsync<JObject>("users/" + uid, token);
            if (user == null || !Flag(user, "isActive") || AuthPolicy.NormalizeRole(Text(user, "role")) != "coordinator")
                throw new UnauthorizedAccessException("A university coordinator account is required.");
        }

        public async Task ApproveHoursAsync(string id, string uid, string token, int hours, string note)
        {
            await RequireCoordinatorAsync(uid, token);
            var review = await LoadAsync(id, token);
            if (Flag(review.Evaluation, "coordinatorApproved")) throw new InvalidOperationException("Completion is already approved; its verified hours are locked.");
            if (hours < 0 || hours > 10000) throw new ArgumentException("Enter verified hours between 0 and 10,000.");
            note = (note ?? "").Trim();
            if (note.Length == 0 || note.Length > 1000) throw new ArgumentException("Add a verification note of up to 1,000 characters describing the attendance evidence you reviewed.");
            var changes = new Dictionary<string, object> {
                { "students/" + review.StudentUid + "/approvedTrainingHours", hours },
                { "students/" + review.StudentUid + "/hoursApprovedAtUnixMs", Now() },
                { "placements/" + review.PlacementId + "/approvedHours", hours },
                { "placements/" + review.PlacementId + "/hoursApprovedByUid", uid },
                { "placements/" + review.PlacementId + "/hoursApprovedAtUnixMs", Now() },
                { "placements/" + review.PlacementId + "/hoursVerificationNote", note }
            };
            if (review.RequiredHours > 0) changes["students/" + review.StudentUid + "/requiredTrainingHours"] = review.RequiredHours;
            await firebase.UpdateRootAsync(changes, token);
        }

        public async Task ApproveCompletionAsync(string id, string uid, string token)
        {
            await RequireCoordinatorAsync(uid, token);
            var review = await LoadAsync(id, token);
            RequireComplete(review);
            if (Flag(review.Evaluation, "coordinatorApproved")) return;
            var changes = new Dictionary<string, object> {
                { "evaluations/" + id + "/coordinatorApproved", true },
                { "evaluations/" + id + "/coordinatorApprovedByUid", uid },
                { "evaluations/" + id + "/coordinatorApprovedAtUnixMs", Now() },
                { "evaluations/" + id + "/finalScore", FinalScore(review.Evaluation) },
                { "placements/" + review.PlacementId + "/status", "Completed" },
                { "students/" + review.StudentUid + "/internshipStatus", "Completed" }
            };
            await firebase.UpdateRootAsync(changes, token);
        }

        public async Task SendReportAsync(string id, string uid, string token)
        {
            await RequireCoordinatorAsync(uid, token);
            var review = await LoadAsync(id, token);
            RequireComplete(review);
            if (!Flag(review.Evaluation, "coordinatorApproved")) throw new InvalidOperationException("Approve completion before sending the report.");
            string reportPath = "completionReports/" + review.StudentUid + "/" + id;
            var existing = await firebase.GetAsync<JObject>(reportPath, token);
            if (existing != null && !string.IsNullOrWhiteSpace(Text(existing, "dataBase64"))) return;
            var report = Snapshot(review);
            report["sentAtUnixMs"] = Now();
            report["sentByUid"] = uid;
            report["dataBase64"] = Convert.ToBase64String(CompletionPdf.Create(report));
            // The student's inbox entry and the immutable PDF are the same record,
            // published atomically with the coordinator's delivery confirmation.
            await firebase.UpdateRootAsync(new Dictionary<string, object> {
                { reportPath, report },
                { "evaluations/" + id + "/reportSentAtUnixMs", report["sentAtUnixMs"].Value<long>() }
            }, token);
        }

        public async Task<byte[]> GetCoordinatorPdfAsync(string id, string uid, string token)
        {
            await RequireCoordinatorAsync(uid, token);
            var review = await LoadAsync(id, token);
            if (!Flag(review.Evaluation, "coordinatorApproved")) throw new InvalidOperationException("Approve completion before printing the final report.");
            var sent = await firebase.GetAsync<JObject>("completionReports/" + review.StudentUid + "/" + id, token);
            if (sent != null && !string.IsNullOrWhiteSpace(Text(sent, "dataBase64"))) return Convert.FromBase64String(Text(sent, "dataBase64"));
            RequireComplete(review);
            return CompletionPdf.Create(Snapshot(review));
        }

        private async Task<Review> LoadAsync(string id, string token)
        {
            ValidateKey(id);
            var evaluation = await firebase.GetAsync<JObject>("evaluations/" + id, token);
            if (evaluation == null) throw new ArgumentException("Evaluation not found.");
            string studentUid = Text(evaluation, "studentUid"), placementId = Text(evaluation, "placementId");
            ValidateKey(studentUid); ValidateKey(placementId);
            var student = await firebase.GetAsync<JObject>("students/" + studentUid, token);
            var placement = await firebase.GetAsync<JObject>("placements/" + placementId, token);
            if (student == null || placement == null || Text(placement, "studentUid") != studentUid)
                throw new InvalidOperationException("The evaluation must reference this student's matching placement.");
            var user = await firebase.GetAsync<JObject>("users/" + studentUid, token);
            string companyId = Text(placement, "companyId"), opportunityId = Text(placement, "opportunityId");
            JObject company = null, opportunity = null;
            if (!string.IsNullOrWhiteSpace(companyId)) { ValidateKey(companyId); company = await firebase.GetAsync<JObject>("companies/" + companyId, token); }
            if (!string.IsNullOrWhiteSpace(opportunityId)) { ValidateKey(opportunityId); opportunity = await firebase.GetAsync<JObject>("opportunities/" + opportunityId, token); }
            int required = Number(placement, "requiredHours");
            if (required == 0) required = Number(student, "requiredTrainingHours");
            if (required == 0) required = Number(student, "requiredHours");
            if (required == 0) required = Number(opportunity, "requiredTrainingHours");
            int approved = placement["approvedHours"] != null ? Number(placement, "approvedHours") : Number(student, "approvedTrainingHours");
            if (placement["approvedHours"] == null && approved == 0) {
                var logs = await firebase.GetAsync<Dictionary<string, JObject>>("trainingLogs", token);
                approved = (logs ?? new Dictionary<string, JObject>()).Values.Where(log => log != null && Text(log, "studentUid") == studentUid
                    && (string.IsNullOrEmpty(Text(log, "placementId")) || Text(log, "placementId") == placementId)
                    && (Flag(log, "isApproved") || Text(log, "status").Equals("approved", StringComparison.OrdinalIgnoreCase))).Sum(log => Number(log, "hours"));
            }
            return new Review { Id = id, StudentUid = studentUid, PlacementId = placementId, Student = student, User = user,
                Placement = placement, Company = company, Opportunity = opportunity, Evaluation = evaluation, RequiredHours = required, ApprovedHours = approved };
        }

        private static void RequireComplete(Review review)
        {
            if (!Flag(review.Evaluation, "companySubmitted") || !Flag(review.Evaluation, "academicSubmitted"))
                throw new InvalidOperationException("Both company and academic evaluations must be submitted first.");
            FinalScore(review.Evaluation);
            if (review.RequiredHours <= 0 || review.ApprovedHours < review.RequiredHours)
                throw new InvalidOperationException("Verify and approve the required training hours before approving completion.");
        }
        public static decimal FinalScore(JObject evaluation)
        {
            decimal company, academic;
            if (!decimal.TryParse(Text(evaluation, "companyScore"), NumberStyles.Number, CultureInfo.InvariantCulture, out company)
                || !decimal.TryParse(Text(evaluation, "academicScore"), NumberStyles.Number, CultureInfo.InvariantCulture, out academic)
                || company < 0 || company > 100 || academic < 0 || academic > 100)
                throw new InvalidOperationException("Both submitted evaluations need valid scores from 0 to 100.");
            return Math.Round(company * 0.40m + academic * 0.60m, 2, MidpointRounding.AwayFromZero);
        }
        private static JObject Snapshot(Review r)
        {
            return new JObject {
                ["evaluationId"] = r.Id, ["studentUid"] = r.StudentUid, ["placementId"] = r.PlacementId,
                ["studentName"] = First(Text(r.Student, "englishName"), Text(r.User, "fullName"), "Student"),
                ["studentNumber"] = First(Text(r.Student, "studentNumber"), Text(r.Student, "studentId"), "Not recorded"),
                ["companyName"] = First(Text(r.Company, "name"), "Not recorded"),
                ["opportunityTitle"] = First(Text(r.Opportunity, "title"), "Internship"),
                ["requiredHours"] = r.RequiredHours, ["approvedHours"] = r.ApprovedHours,
                ["companyScore"] = r.Evaluation["companyScore"], ["academicScore"] = r.Evaluation["academicScore"],
                ["finalScore"] = r.Evaluation["finalScore"] ?? JToken.FromObject(FinalScore(r.Evaluation)),
                ["approvedAtUnixMs"] = r.Evaluation["coordinatorApprovedAtUnixMs"] ?? JToken.FromObject(Now()),
                ["approvedByUid"] = Text(r.Evaluation, "coordinatorApprovedByUid"),
                ["title"] = "Your final internship report is ready"
            };
        }
        private static long Now() { return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); }
        private static string First(params string[] values) { return values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? ""; }
        private static string Text(JObject o, string key) { return o == null || o[key] == null ? "" : o[key].ToString(); }
        private static bool Flag(JObject o, string key) { bool b; return bool.TryParse(Text(o, key), out b) && b; }
        private static int Number(JObject o, string key) { int n; return int.TryParse(Text(o, key), out n) ? Math.Max(0, n) : 0; }
        private sealed class Review {
            public string Id, StudentUid, PlacementId;
            public JObject Student, User, Placement, Company, Opportunity, Evaluation;
            public int RequiredHours, ApprovedHours;
        }
    }
}
