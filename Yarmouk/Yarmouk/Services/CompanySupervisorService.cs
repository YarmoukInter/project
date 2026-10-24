using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;

namespace Yarmouk.Services
{
    public sealed class CompanyProfileData
    {
        public string SupervisorUid { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string Industry { get; set; }
        public string Location { get; set; }
        public string ContactEmail { get; set; }
    }

    public sealed class CompanyEvaluationRow
    {
        public string PlacementId { get; set; }
        public string StudentUid { get; set; }
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public string MajorId { get; set; }
        public string AcademicSupervisorUid { get; set; }
        public string CompanyName { get; set; }
        public string PlacementStatus { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string EvaluationId { get; set; }
        public string Score { get; set; }
        public string Feedback { get; set; }
        public bool Submitted { get; set; }
        public bool Locked { get; set; }
        public Dictionary<string, string> Criteria { get; set; }
    }

    public sealed class CompanyReportRow
    {
        public string SubmissionId { get; set; }
        public string TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string StudentUid { get; set; }
        public string StudentName { get; set; }
        public string Notes { get; set; }
        public string SubmissionUrl { get; set; }
        public string Status { get; set; }
        public string Feedback { get; set; }
        public long SubmittedAtUnixMs { get; set; }
    }

    public sealed class CompanySupervisorService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        private static readonly string[] Levels = { "Excellent", "Very Good", "Good", "Satisfactory", "Poor", "Fail" };

        public async Task<CompanyProfileData> GetSupervisorAccountAsync(string uid, string token)
        {
            RequireIdentity(uid, token);
            var user = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + uid, token).ConfigureAwait(false);
            if (user == null || !Same(Read(user, "uid", uid), uid))
                throw new UnauthorizedAccessException("The current Firebase profile could not be verified.");
            return new CompanyProfileData
            {
                SupervisorUid = uid,
                FullName = Read(user, "fullName"),
                Email = Read(user, "email"),
                Phone = Read(user, "phone")
            };
        }

        public async Task<CompanyProfileData> GetProfileAsync(string uid, string token)
        {
            RequireIdentity(uid, token);
            JObject user = null;
            Exception userReadError = null;
            try { user = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + uid, token).ConfigureAwait(false); }
            catch (Exception error) { userReadError = error; }
            if (user != null && !Same(Read(user, "uid", uid), uid))
                throw new UnauthorizedAccessException("The current Firebase profile could not be verified.");

            string companyId = Read(user, "companyId", ConfigurationManager.AppSettings["CompanyProfileId"] ?? "DEMO_COMPANY");
            JObject company = null;
            if (!string.IsNullOrWhiteSpace(companyId))
            {
                try { company = await firebase.GetAsync<JObject>(DatabasePaths.Companies + "/" + companyId, token).ConfigureAwait(false); }
                catch when (!string.IsNullOrWhiteSpace(Read(user, "companyId"))) { throw; }
            }
            if (company == null)
            {
                var linked = await firebase.GetByChildAsync<Dictionary<string, JObject>>(
                    DatabasePaths.Companies, "supervisorUid", uid, token).ConfigureAwait(false);
                var first = linked == null ? default(KeyValuePair<string, JObject>) : linked.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(first.Key)) { companyId = first.Key; company = first.Value; }
            }
            if (company == null && userReadError != null) throw userReadError;
            if (company != null)
            {
                bool linkedToSupervisor = Same(Read(company, "supervisorUid"), uid)
                    || (company["supervisorUids"] is JObject supervisorUids && supervisorUids[uid] != null && supervisorUids[uid].Value<bool>())
                    || Same(Read(user, "companyId"), companyId);
                if (!linkedToSupervisor)
                    throw new UnauthorizedAccessException("The selected Firebase company record is not linked to this supervisor UID.");
            }
            return new CompanyProfileData
            {
                SupervisorUid = uid,
                FullName = Read(user, "fullName"),
                Email = Read(user, "email"),
                Phone = Read(user, "phone"),
                CompanyId = companyId,
                CompanyName = First(Read(company, "name"), Read(company, "companyName")),
                CompanyPhone = First(Read(company, "Phone"), Read(company, "phone"), Read(company, "companyPhone")),
                Industry = First(Read(company, "industry"), Read(company, "sector")),
                Location = First(Read(company, "location"), Read(company, "address"), Read(company, "city")),
                ContactEmail = First(Read(company, "contactEmail"), Read(company, "companyEmail"), Read(company, "email"))
            };
        }

        public async Task SaveProfileAsync(string uid, string token, string fullName, string phone)
        {
            RequireIdentity(uid, token);
            fullName = (fullName ?? string.Empty).Trim();
            phone = (phone ?? string.Empty).Trim();
            if (fullName.Length < 2 || fullName.Length > 120)
                throw new ArgumentException("Enter a name between 2 and 120 characters.");
            if (phone.Length > 32)
                throw new ArgumentException("Phone number must be 32 characters or fewer.");
            var verified = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + uid, token).ConfigureAwait(false);
            if (verified == null || !Same(Read(verified, "uid", uid), uid))
                throw new UnauthorizedAccessException("The current Firebase profile could not be verified.");
            // PATCH the user object with both editable fields. A PATCH to an individual
            // scalar leaf is rejected by the Realtime Database REST API; this preserves
            // all other user fields while updating only fullName and phone.
            await firebase.UpdateAsync(DatabasePaths.Users + "/" + uid,
                new Dictionary<string, object>
                {
                    { "fullName", fullName },
                    { "phone", phone }
                }, token).ConfigureAwait(false);
        }

        public async Task SaveCompanyProfileAsync(string uid, string token, string companyName,
            string companyPhone, string industry, string location, string contactEmail)
        {
            RequireIdentity(uid, token);
            companyName = (companyName ?? string.Empty).Trim();
            companyPhone = (companyPhone ?? string.Empty).Trim();
            industry = (industry ?? string.Empty).Trim();
            location = (location ?? string.Empty).Trim();
            contactEmail = (contactEmail ?? string.Empty).Trim();

            if (companyName.Length < 2 || companyName.Length > 160)
                throw new ArgumentException("Enter a company name between 2 and 160 characters.");
            if (companyPhone.Length > 32 || !Regex.IsMatch(companyPhone, @"^[0-9+() .-]*$"))
                throw new ArgumentException("Enter a valid company phone using digits and common phone symbols (32 characters maximum).");
            if (industry.Length > 120) throw new ArgumentException("Industry must be 120 characters or fewer.");
            if (location.Length > 200) throw new ArgumentException("Location must be 200 characters or fewer.");
            if (contactEmail.Length > 254) throw new ArgumentException("Company contact email must be 254 characters or fewer.");
            if (contactEmail.Length > 0)
            {
                try { new MailAddress(contactEmail); }
                catch (FormatException) { throw new ArgumentException("Enter a valid company contact email."); }
            }

            var profile = await GetProfileAsync(uid, token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(profile.CompanyId))
                throw new UnauthorizedAccessException("No company record is linked to this supervisor account.");

            // PATCH only the editable company fields; leave its supervisor links and all other data intact.
            await firebase.UpdateAsync(DatabasePaths.Companies + "/" + profile.CompanyId,
                new Dictionary<string, object>
                {
                    { "name", companyName },
                    { "Phone", companyPhone },
                    { "industry", industry },
                    { "location", location },
                    { "contactEmail", contactEmail }
                }, token).ConfigureAwait(false);
        }

        public async Task<string> GetCompanyIdForSupervisorAsync(string uid, string token)
        {
            RequireIdentity(uid, token);
            var user = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + uid, token).ConfigureAwait(false);
            string companyId = Read(user, "companyId");
            if (!string.IsNullOrWhiteSpace(companyId))
            {
                var company = await firebase.GetAsync<JObject>(DatabasePaths.Companies + "/" + companyId, token).ConfigureAwait(false);
                bool linked = Same(Read(company, "supervisorUid"), uid)
                    || Same(Read(user, "companyId"), companyId)
                    || (company?["supervisorUids"] is JObject companySupervisorSet
                        && companySupervisorSet[uid] != null && companySupervisorSet[uid].Value<bool>());
                if (company != null && linked) return companyId;
            }

            var companies = await firebase.GetAsync<Dictionary<string, JObject>>(
                DatabasePaths.Companies, token).ConfigureAwait(false);
            var match = (companies ?? new Dictionary<string, JObject>()).FirstOrDefault(x => x.Value != null
                && (Same(Read(x.Value, "supervisorUid"), uid)
                    || (x.Value["supervisorUids"] is JObject candidateSupervisorSet
                        && candidateSupervisorSet[uid] != null && candidateSupervisorSet[uid].Value<bool>())));
            companyId = string.IsNullOrWhiteSpace(match.Key) ? string.Empty : Read(match.Value, "id", match.Key);
            if (string.IsNullOrWhiteSpace(companyId))
                throw new UnauthorizedAccessException("The signed-in company supervisor is not linked to a company.");
            return companyId;
        }

        public async Task<List<CompanyEvaluationRow>> GetEvaluationQueueAsync(string uid, string companyId, string token)
        {
            RequireIdentity(uid, token);
            if (string.IsNullOrWhiteSpace(companyId))
                throw new UnauthorizedAccessException("A verified company ID is required.");

            var allStudents = await firebase.GetAsync<Dictionary<string, JObject>>(
                DatabasePaths.Students, token).ConfigureAwait(false);

            var assignedStudents = (allStudents ?? new Dictionary<string, JObject>())
                .Where(x => x.Value != null
                    && Same(Read(x.Value, "companyId"), companyId)
                    && !string.IsNullOrWhiteSpace(First(Read(x.Value, "studentId"), Read(x.Value, "studentNumber"))))
                .ToList();

            var placements = await firebase.GetAsync<Dictionary<string, JObject>>(
                DatabasePaths.Placements, token).ConfigureAwait(false) ?? new Dictionary<string, JObject>();
            var evaluationResults = await firebase.GetAsync<Dictionary<string, JObject>>(
                DatabasePaths.Evaluations, token).ConfigureAwait(false) ?? new Dictionary<string, JObject>();
            var company = await firebase.GetAsync<JObject>(
                DatabasePaths.Companies + "/" + companyId, token).ConfigureAwait(false);
            var allTasks = await firebase.GetAsync<Dictionary<string, JObject>>(
                DatabasePaths.Tasks, token).ConfigureAwait(false) ?? new Dictionary<string, JObject>();
            var allSubmissions = await firebase.GetAsync<Dictionary<string, JObject>>(
                DatabasePaths.TaskSubmissions, token).ConfigureAwait(false) ?? new Dictionary<string, JObject>();

            var result = new List<CompanyEvaluationRow>();
            foreach (var studentItem in assignedStudents)
            {
                var student = studentItem.Value;
                var studentUid = Read(student, "uid", studentItem.Key);
                if (string.IsNullOrWhiteSpace(studentUid)) continue;
                string studentId = First(Read(student, "studentId"), Read(student, "studentNumber"));

                var placementItem = placements
                    .Where(x => x.Value != null
                        && (string.IsNullOrWhiteSpace(Read(x.Value, "companyId"))
                            || Same(Read(x.Value, "companyId"), companyId))
                        && (Same(Read(x.Value, "studentUid"), studentItem.Key)
                            || Same(Read(x.Value, "studentUid"), studentUid)
                            || (!string.IsNullOrWhiteSpace(studentId)
                                && Same(First(Read(x.Value, "studentId"), Read(x.Value, "studentNumber")), studentId))))
                    .OrderByDescending(x => Same(Read(x.Value, "studentUid"), studentItem.Key))
                    .ThenByDescending(x => Same(Read(x.Value, "studentUid"), studentUid))
                    .FirstOrDefault();
                var placement = placementItem.Value;
                var placementId = string.IsNullOrWhiteSpace(placementItem.Key) ? string.Empty : Read(placement, "id", placementItem.Key);

                // خذ الـ uid الصحيح من الـ placement (بيحل مشكلة uid الغلط عند w15CYl2A…)
                if (placement != null && !string.IsNullOrWhiteSpace(Read(placement, "studentUid")))
                    studentUid = Read(placement, "studentUid");

                // A final evaluation can only be opened after every task assigned to this
                // trainee for this company placement has been approved as completed.
                // Students without an assigned task do not appear in the evaluation queue.
                var assignedTasks = allTasks.Where(x => x.Value != null
                        && Same(Read(x.Value, "studentUid"), studentUid)
                        && (string.IsNullOrWhiteSpace(Read(x.Value, "companyId"))
                            || Same(Read(x.Value, "companyId"), companyId))
                        && (string.IsNullOrWhiteSpace(Read(x.Value, "placementId"))
                            || Same(Read(x.Value, "placementId"), placementId))
                        && (string.IsNullOrWhiteSpace(Read(x.Value, "supervisorUid"))
                            || Same(Read(x.Value, "supervisorUid"), uid)))
                    .ToList();
                if (assignedTasks.Count == 0 || assignedTasks.Any(task => !IsTaskCompleted(task, studentUid, allSubmissions)))
                    continue;

                var studentNumber = First(Read(student, "studentNumber"), Read(student, "studentId"));
                var majorId = Read(student, "majorId");
                var studentName = First(Read(student, "englishName"), Read(student, "fullName"));
                if (string.IsNullOrWhiteSpace(studentName))
                {
                    var user = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + studentUid, token).ConfigureAwait(false);
                    studentName = First(Read(user, "fullName"), Read(user, "englishName"));
                }

                JObject record = null;
                string evaluationId = string.Empty;
                foreach (var evaluationItem in evaluationResults)
                {
                    var candidate = evaluationItem.Value;
                    if (!string.IsNullOrWhiteSpace(placementId) && candidate != null
                        && Same(Read(candidate, "placementId"), placementId)
                        && Same(Read(candidate, "studentUid"), studentUid))
                    { record = candidate; evaluationId = evaluationItem.Key; break; }
                }

                bool submitted = ReadBool(record, "companySubmitted");
                bool locked = submitted || ReadBool(record, "coordinatorApproved");
                var criteria = new Dictionary<string, string>(StringComparer.Ordinal);
                var savedCriteria = record == null ? null : record["companyCriteria"];
                var criteriaEntries = savedCriteria is JObject criteriaObject
                    ? criteriaObject.Properties().Select(x => new KeyValuePair<string, JToken>(x.Name, x.Value))
                    : savedCriteria is JArray criteriaArray
                        ? criteriaArray.Select((value, index) => new KeyValuePair<string, JToken>(index.ToString(CultureInfo.InvariantCulture), value))
                        : Enumerable.Empty<KeyValuePair<string, JToken>>();
                foreach (var criterion in criteriaEntries)
                    {
                        int number;
                        string value = criterion.Value == null || criterion.Value.Type == JTokenType.Null ? string.Empty : criterion.Value.ToString();
                        if (Levels.Contains(value, StringComparer.Ordinal)
                            || (int.TryParse(criterion.Key, out number) && (number == 11 || number == 12 || number == 13) && value == "N/A"))
                            criteria[criterion.Key] = value;
                    }

                result.Add(new CompanyEvaluationRow
                {
                    PlacementId = placementId,
                    StudentUid = studentUid,
                    StudentName = studentName,
                    StudentNumber = studentNumber,
                    ProfileImageUrl = StudentProfileImages.GetUrl(Read(student, "profileImageFileName")),
                    MajorId = majorId,
                    // The academic evaluation queue is keyed from students/{uid}/academicSupervisorId.
                    // Do not enable company submission from a placement-only fallback that the academic queue cannot see.
                    AcademicSupervisorUid = Read(student, "academicSupervisorId"),
                    CompanyName = First(Read(company, "name"), Read(student, "companyName")),
                    PlacementStatus = placement == null ? "Placement not linked" : Read(placement, "status"),
                    StartDate = Read(placement, "startDate"),
                    EndDate = Read(placement, "endDate"),
                    EvaluationId = evaluationId,
                    Score = HasValue(record, "companyScore") && (submitted || HasValue(record, "companyUpdatedAtUnixMs")) ? Read(record, "companyScore") : string.Empty,
                    Feedback = First(Read(record, "companyFeedback"), Read(record, "companyComments")),
                    Submitted = submitted,
                    Locked = locked,
                    Criteria = criteria
                });
            }
            return result.OrderBy(x => x.StudentName).ToList();
        }

        public async Task SaveCompanyEvaluationAsync(string uid, string companyId, string token, string placementId,
            int? score, string feedback, IDictionary<string, string> criteria, bool submit)
        {
            RequireIdentity(uid, token);
            if (string.IsNullOrWhiteSpace(placementId) || placementId.Length > 128)
                throw new ArgumentException("The selected placement is invalid.");
            if (score.HasValue && (score.Value < 0 || score.Value > 100))
                throw new ArgumentOutOfRangeException("score", "The company form asks for an overall grade from 0 to 100.");
            feedback = (feedback ?? string.Empty).Trim();
            if (feedback.Length > 4000) throw new ArgumentException("Observations must be 4,000 characters or fewer.");
            criteria = criteria ?? new Dictionary<string, string>();
            foreach (var pair in criteria)
            {
                if (!int.TryParse(pair.Key, out var number) || number < 1 || number > 18
                    || (!Levels.Contains(pair.Value, StringComparer.Ordinal)
                        && !((number == 11 || number == 12 || number == 13) && pair.Value == "N/A")))
                    throw new ArgumentException("One or more criterion ratings are invalid.");
            }

            var rows = await GetEvaluationQueueAsync(uid, companyId, token).ConfigureAwait(false);
            var selected = rows.FirstOrDefault(x => Same(x.PlacementId, placementId));
            if (selected == null) throw new UnauthorizedAccessException("This placement is not assigned to your company.");
            if (selected.Locked) throw new InvalidOperationException("This evaluation has already been submitted or approved and is locked.");
            if (submit && (criteria.Count != 18 || !score.HasValue))
                throw new ArgumentException("Complete all 18 criteria and the overall grade before submission.");
            if (submit && string.IsNullOrWhiteSpace(selected.AcademicSupervisorUid))
                throw new ArgumentException("This student has no academic supervisor UID in students/{studentUid}/academicSupervisorId.");

            string id = selected.EvaluationId;
            if (string.IsNullOrWhiteSpace(id)) id = "EVAL_" + placementId;
            if (id.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/' }) >= 0)
                throw new ArgumentException("The evaluation record key is invalid.");
            long now = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            var values = new Dictionary<string, object>
            {
                { "id", id }, { "placementId", placementId }, { "studentUid", selected.StudentUid },
                { "companySupervisorUid", uid }, { "companyFeedback", feedback },
                { "companyId", companyId }, { "academicSupervisorUid", selected.AcademicSupervisorUid },
                { "companyCriteria", criteria }, { "companySubmitted", submit }, { "companyUpdatedAtUnixMs", now }
            };
            if (score.HasValue) values["companyScore"] = score.Value;
            if (submit) values["companySubmittedAtUnixMs"] = now;
            await firebase.UpdateAsync(DatabasePaths.Evaluations + "/" + id, values, token).ConfigureAwait(false);
        }

        public async Task<List<CompanyReportRow>> GetReportsAsync(string uid, string companyId, string token, string placementId)
        {
            RequireIdentity(uid, token);
            var placement = (await GetEvaluationQueueAsync(uid, companyId, token).ConfigureAwait(false))
                .FirstOrDefault(x => Same(x.PlacementId, placementId));
            if (placement == null) throw new UnauthorizedAccessException("This placement is not assigned to your company.");

            if (string.IsNullOrWhiteSpace(placement.PlacementId))
                return new List<CompanyReportRow>();

            var taskResults = await firebase.GetAsync<Dictionary<string, JObject>>(
                DatabasePaths.Tasks, token).ConfigureAwait(false);
            var tasks = taskResults ?? new Dictionary<string, JObject>();
            var allowedTasks = tasks.Where(x => x.Value != null
                    && Same(Read(x.Value, "studentUid"), placement.StudentUid)
                    && (string.IsNullOrWhiteSpace(Read(x.Value, "supervisorUid"))
                        || Same(Read(x.Value, "supervisorUid"), uid)))
                .ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal);
            var submissions = await firebase.GetAsync<Dictionary<string, JObject>>(
                DatabasePaths.TaskSubmissions, token).ConfigureAwait(false);
            if (submissions == null) return new List<CompanyReportRow>();

            return submissions.Where(x => x.Value != null && Same(Read(x.Value, "studentUid"), placement.StudentUid)
                    && allowedTasks.ContainsKey(Read(x.Value, "taskId")))
                .Select(x =>
                {
                    var task = allowedTasks[Read(x.Value, "taskId")];
                    return new CompanyReportRow
                    {
                        SubmissionId = Read(x.Value, "id", x.Key),
                        TaskId = Read(x.Value, "taskId"),
                        TaskTitle = Read(task, "title", "Student submission"),
                        StudentUid = placement.StudentUid,
                        StudentName = placement.StudentName,
                        Notes = Read(x.Value, "notes"),
                        SubmissionUrl = Read(x.Value, "submissionUrl"),
                        Status = Read(x.Value, "status", Read(x.Value, "reviewStatus", "Submitted")),
                        Feedback = Read(x.Value, "reviewFeedback"),
                        SubmittedAtUnixMs = ReadLong(x.Value, "submittedAtUnixMs")
                    };
                }).OrderByDescending(x => x.SubmittedAtUnixMs).ToList();
        }

        public async Task SaveReportReviewAsync(string uid, string companyId, string token, string placementId,
            string submissionId, string status, string feedback)
        {
            RequireIdentity(uid, token);
            if (status != "Approved" && status != "RevisionRequested")
                throw new ArgumentException("Choose approval or request a revision.");
            feedback = (feedback ?? string.Empty).Trim();
            if (feedback.Length > 2000) throw new ArgumentException("Feedback must be 2,000 characters or fewer.");
            if (status == "RevisionRequested" && feedback.Length == 0)
                throw new ArgumentException("Add feedback describing the requested revision.");
            var report = (await GetReportsAsync(uid, companyId, token, placementId).ConfigureAwait(false))
                .FirstOrDefault(x => Same(x.SubmissionId, submissionId));
            if (report == null) throw new UnauthorizedAccessException("This submission is not assigned to your company.");
            if (string.Equals(report.Status, "Approved", StringComparison.OrdinalIgnoreCase)
                || string.Equals(report.Status, "RevisionRequested", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This submission has already been reviewed. Review the student's latest resubmission instead.");
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await firebase.UpdateAsync(DatabasePaths.TaskSubmissions + "/" + submissionId, new Dictionary<string, object>
            {
                { "status", status }, { "reviewStatus", status }, { "reviewFeedback", feedback },
                { "reviewedByUid", uid }, { "reviewedAtUnixMs", now }
            }, token).ConfigureAwait(false);
        }

        private static long ReadLong(JObject value, string key)
        { long parsed; return long.TryParse(Read(value, key), out parsed) ? parsed : 0L; }

        private static void AddRecords(IDictionary<string, JObject> destination, IDictionary<string, JObject> source)
        {
            if (source == null) return;
            foreach (var item in source) if (!destination.ContainsKey(item.Key)) destination[item.Key] = item.Value;
        }

        private static void RequireIdentity(string uid, string token)
        {
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedAccessException("A verified company supervisor session is required.");
        }
        private static JObject Find(JObject root, string key) { return root == null || string.IsNullOrWhiteSpace(key) ? null : root[key] as JObject; }
        private static string Read(JObject value, string key, string fallback = "")
        { var item = value == null ? null : value[key]; return item == null || item.Type == JTokenType.Null || string.IsNullOrWhiteSpace(item.ToString()) ? fallback : item.ToString(); }
        private static bool ReadBool(JObject value, string key)
        { bool parsed; var item = value == null ? null : value[key]; return item != null && (item.Type == JTokenType.Boolean ? item.Value<bool>() : bool.TryParse(item.ToString(), out parsed) && parsed); }
        private static bool HasValue(JObject value, string key) { var item = value == null ? null : value[key]; double number; return item != null && item.Type != JTokenType.Null && double.TryParse(item.ToString(), out number); }
        private static bool Same(string a, string b) { return string.Equals(a, b, StringComparison.Ordinal); }
        private static string First(params string[] values) { return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty; }

        private static bool IsTaskCompleted(KeyValuePair<string, JObject> task, string studentUid,
            IDictionary<string, JObject> submissions)
        {
            if (IsCompletedStatus(Read(task.Value, "status"))) return true;
            var taskId = Read(task.Value, "id", task.Key);
            var latestSubmission = (submissions ?? new Dictionary<string, JObject>())
                .Where(x => x.Value != null && Same(Read(x.Value, "taskId"), taskId)
                    && Same(Read(x.Value, "studentUid"), studentUid))
                .OrderByDescending(x => ReadLong(x.Value, "submittedAtUnixMs"))
                .FirstOrDefault();
            return latestSubmission.Value != null && IsCompletedStatus(First(
                Read(latestSubmission.Value, "reviewStatus"), Read(latestSubmission.Value, "status")));
        }

        private static bool IsCompletedStatus(string value)
        {
            var normalized = (value ?? string.Empty).Replace("_", string.Empty).Replace("-", string.Empty)
                .Replace(" ", string.Empty).ToLowerInvariant();
            return normalized == "completed" || normalized == "complete" || normalized == "approved";
        }
    }
}
