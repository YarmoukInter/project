using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;

namespace Yarmouk.Services
{
    public sealed class SupervisorWorkflowException : Exception
    {
        public SupervisorWorkflowException(string message) : base(message) { }
    }

    public sealed class SupervisorWorkspaceService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();

        public async Task<List<SupervisorStudentProgress>> GetAcademicStudentsAsync(string academicUid, string idToken)
        {
            RequireIdentity(academicUid, idToken);
            var data = await LoadDataAsync(idToken).ConfigureAwait(false);
            return BuildProgressRows(data, row =>
            {
                var student = Find(data.Students, row.StudentUid);
                return Same(Read(row.Placement, "academicSupervisorUid"), academicUid)
                    || Same(Read(row.Placement, "academicSupervisorId"), academicUid)
                    || (student != null && Same(Read(student, "academicSupervisorId"), academicUid));
            }).OrderBy(x => x.StudentName).ToList();
        }

        public async Task<List<SupervisorStudentProgress>> GetCompanyTraineesAsync(string companySupervisorUid, string idToken)
        {
            RequireIdentity(companySupervisorUid, idToken);
            var data = await LoadDataAsync(idToken).ConfigureAwait(false);
            var companyIds = GetCompanyIds(data, companySupervisorUid);
            return BuildProgressRows(data, row =>
            {
                var placementCompanyId = Read(row.Placement, "companyId");
                return Same(Read(row.Placement, "companySupervisorUid"), companySupervisorUid)
                    || companyIds.ContainsKey(placementCompanyId);
            }).Where(x => IsConfirmedPlacement(data, x.PlacementId))
              .OrderBy(x => x.StudentName).ToList();
        }

        public async Task<List<AcademicEvaluationItem>> GetAcademicEvaluationQueueAsync(string academicUid, string idToken)
        {
            RequireIdentity(academicUid, idToken);
            var data = await LoadDataAsync(idToken).ConfigureAwait(false);
            var placements = BuildProgressRows(data, row =>
            {
                var student = Find(data.Students, row.StudentUid);
                return Same(Read(row.Placement, "academicSupervisorUid"), academicUid)
                    || Same(Read(row.Placement, "academicSupervisorId"), academicUid)
                    || (student != null && Same(Read(student, "academicSupervisorId"), academicUid));
            });

            return placements.Select(row => BuildEvaluationItem(data, row, academicUid))
                .OrderBy(x => x.StudentName).ToList();
        }

        public async Task SaveAcademicEvaluationAsync(string placementId, string academicUid, string idToken,
            int score, string feedback, bool submit)
        {
            RequireIdentity(academicUid, idToken);
            if (string.IsNullOrWhiteSpace(placementId) || placementId.Length > 128)
                throw new SupervisorWorkflowException("The selected placement is invalid. Refresh the page and try again.");
            if (score < 0 || score > 100)
                throw new SupervisorWorkflowException("Enter an academic score between 0 and 100.");
            if (feedback == null) feedback = string.Empty;
            feedback = feedback.Trim();
            if (feedback.Length > 2000)
                throw new SupervisorWorkflowException("Academic feedback must be 2,000 characters or fewer.");

            var data = await LoadDataAsync(idToken).ConfigureAwait(false);
            var assigned = BuildProgressRows(data, row =>
            {
                var student = Find(data.Students, row.StudentUid);
                return Same(Read(row.Placement, "academicSupervisorUid"), academicUid)
                    || Same(Read(row.Placement, "academicSupervisorId"), academicUid)
                    || (student != null && Same(Read(student, "academicSupervisorId"), academicUid));
            }).FirstOrDefault(row => Same(row.PlacementId, placementId));
            if (assigned == null)
                throw new SupervisorWorkflowException("This placement is not assigned to your academic account.");

            var evaluation = FindEvaluation(data.Evaluations, placementId, assigned.StudentUid, academicUid);
            bool academicSubmitted = evaluation != null && ReadBool(evaluation.Value, "academicSubmitted");
            bool coordinatorApproved = evaluation != null && ReadBool(evaluation.Value, "coordinatorApproved");
            if (academicSubmitted || coordinatorApproved)
                throw new SupervisorWorkflowException("This evaluation has already been submitted and is locked.");

            bool companySubmitted = evaluation != null && IsCompanyEvaluationSubmitted(evaluation.Value);
            if (submit && !companySubmitted)
                throw new SupervisorWorkflowException("The company must submit its evaluation before you can submit the academic evaluation. You can save a draft meanwhile.");

            string evaluationId = evaluation == null ? "EVAL_" + placementId : evaluation.Id;
            if (!IsValidFirebaseKey(evaluationId))
                throw new SupervisorWorkflowException("The evaluation record key is invalid.");
            await new EvaluationService().SaveAcademicEvaluationAsync(evaluationId, placementId,
                assigned.StudentUid, academicUid, score, feedback, submit, idToken).ConfigureAwait(false);
        }

        private async Task<WorkspaceData> LoadDataAsync(string idToken)
        {
            var placementsTask = firebase.GetAsync<JObject>(DatabasePaths.Placements, idToken);
            var studentsTask = firebase.GetAsync<JObject>(DatabasePaths.Students, idToken);
            var usersTask = firebase.GetAsync<JObject>(DatabasePaths.Users, idToken);
            var companiesTask = firebase.GetAsync<JObject>(DatabasePaths.Companies, idToken);
            var tasksTask = firebase.GetAsync<JObject>(DatabasePaths.Tasks, idToken);
            var logsTask = firebase.GetAsync<JObject>(DatabasePaths.TrainingLogs, idToken);
            var evaluationsTask = firebase.GetAsync<JObject>(DatabasePaths.Evaluations, idToken);
            var applicationsTask = firebase.GetAsync<JObject>(DatabasePaths.Applications, idToken);
            await Task.WhenAll(placementsTask, studentsTask, usersTask, companiesTask,
                tasksTask, logsTask, evaluationsTask, applicationsTask).ConfigureAwait(false);
            return new WorkspaceData
            {
                Placements = await placementsTask.ConfigureAwait(false) ?? new JObject(),
                Students = await studentsTask.ConfigureAwait(false) ?? new JObject(),
                Users = await usersTask.ConfigureAwait(false) ?? new JObject(),
                Companies = await companiesTask.ConfigureAwait(false) ?? new JObject(),
                Tasks = await tasksTask.ConfigureAwait(false) ?? new JObject(),
                TrainingLogs = await logsTask.ConfigureAwait(false) ?? new JObject(),
                Evaluations = await evaluationsTask.ConfigureAwait(false) ?? new JObject(),
                Applications = await applicationsTask.ConfigureAwait(false) ?? new JObject()
            };
        }

        private static List<SupervisorStudentProgress> BuildProgressRows(WorkspaceData data, Func<ProgressRow, bool> assigned)
        {
            var rows = new List<ProgressRow>();
            foreach (var item in data.Placements.Properties())
            {
                var placement = item.Value as JObject;
                if (placement == null) continue;
                string placementId = Read(placement, "id", item.Name);
                string studentUid = Read(placement, "studentUid");
                if (string.IsNullOrWhiteSpace(studentUid)) continue;
                var row = new ProgressRow { Placement = placement, PlacementId = placementId, StudentUid = studentUid };
                if (assigned(row)) rows.Add(row);
            }

            return rows.Select(row => EnrichProgressRow(data, row)).ToList();
        }

        private static SupervisorStudentProgress EnrichProgressRow(WorkspaceData data, ProgressRow row)
        {
            var student = Find(data.Students, row.StudentUid);
            var user = Find(data.Users, row.StudentUid);
            string companyId = Read(row.Placement, "companyId", Read(student, "companyId"));
            var company = Find(data.Companies, companyId);
            int approvedLogHours = 0;
            var studentTasks = new List<JObject>();
            foreach (var logItem in data.TrainingLogs.Properties())
            {
                var log = logItem.Value as JObject;
                if (log == null || !Same(Read(log, "studentUid"), row.StudentUid)) continue;
                string logPlacementId = Read(log, "placementId");
                if (!string.IsNullOrWhiteSpace(logPlacementId) && !Same(logPlacementId, row.PlacementId)) continue;
                if (IsApprovedLog(log)) approvedLogHours += ReadInt(log, "hours");
            }
            foreach (var taskItem in data.Tasks.Properties())
            {
                var task = taskItem.Value as JObject;
                if (task == null || !Same(Read(task, "studentUid"), row.StudentUid)) continue;
                string taskPlacementId = Read(task, "placementId");
                if (!string.IsNullOrWhiteSpace(taskPlacementId) && !Same(taskPlacementId, row.PlacementId)) continue;
                studentTasks.Add(task);
            }

            int approvedHours = ReadNullableInt(row.Placement, "approvedHours")
                ?? (approvedLogHours > 0 ? approvedLogHours : ReadInt(student, "approvedTrainingHours"));
            int requiredHours = ReadNullableInt(row.Placement, "requiredHours")
                ?? ReadNullableInt(student, "requiredTrainingHours")
                ?? ReadInt(student, "requiredHours");
            int completedTasks = studentTasks.Count(task => IsCompletedTask(Read(task, "status")));
            int submittedTasks = studentTasks.Count(task => IsSubmittedTask(Read(task, "status")));
            int percent = requiredHours > 0 ? (int)Math.Round(100d * approvedHours / requiredHours) :
                (studentTasks.Count == 0 ? 0 : (int)Math.Round(100d * completedTasks / studentTasks.Count));
            percent = Math.Max(0, Math.Min(100, percent));

            var evaluation = FindEvaluation(data.Evaluations, row.PlacementId, row.StudentUid, null);
            string feedback = Read(evaluation == null ? null : evaluation.Value, "companyFeedback");
            if (string.IsNullOrWhiteSpace(feedback)) feedback = Read(evaluation == null ? null : evaluation.Value, "companyComments");
            string score = Read(evaluation == null ? null : evaluation.Value, "companyScore");
            if (string.IsNullOrWhiteSpace(feedback) && !string.IsNullOrWhiteSpace(score) &&
                evaluation != null && IsCompanyEvaluationSubmitted(evaluation.Value))
                feedback = "Company evaluation submitted · " + score + "/100";

            string studentName = Read(user, "fullName", Read(student, "englishName", "Student"));
            string startDate = Read(row.Placement, "startDate", Read(student, "trainingStartDate", "—"));
            string endDate = Read(row.Placement, "endDate", "—");
            return new SupervisorStudentProgress
            {
                PlacementId = row.PlacementId,
                StudentUid = row.StudentUid,
                StudentName = studentName,
                Initials = GetInitials(studentName),
                StudentNumber = Read(student, "studentId", Read(student, "studentNumber", "—")),
                CompanyName = Read(company, "name", string.IsNullOrWhiteSpace(companyId) ? "Company not assigned" : companyId),
                PlacementStatus = Humanize(Read(row.Placement, "status", Read(student, "internshipStatus", "In progress"))),
                StartDate = startDate,
                EndDate = endDate,
                TrainingPeriodLabel = startDate == "—" && endDate == "—" ? "Not recorded" : startDate + " — " + endDate,
                CompanyFeedback = feedback,
                ApprovedHours = Math.Max(0, approvedHours),
                RequiredHours = Math.Max(0, requiredHours),
                ProgressPercent = percent,
                TotalTasks = studentTasks.Count,
                CompletedTasks = completedTasks,
                SubmittedTasks = submittedTasks,
                TaskProgressLabel = studentTasks.Count == 0 ? "No tasks assigned" : completedTasks + " of " + studentTasks.Count + " tasks complete",
                HoursProgressLabel = requiredHours > 0 ? approvedHours + " / " + requiredHours + " approved hours" : approvedHours + " approved hours",
                FeedbackLabel = string.IsNullOrWhiteSpace(feedback) ? "No company feedback submitted yet." : feedback
            };
        }

        private static AcademicEvaluationItem BuildEvaluationItem(WorkspaceData data, SupervisorStudentProgress row, string academicUid)
        {
            var evaluation = FindEvaluation(data.Evaluations, row.PlacementId, row.StudentUid, academicUid);
            var record = evaluation == null ? null : evaluation.Value;
            bool academicSubmitted = ReadBool(record, "academicSubmitted");
            bool companySubmitted = record != null && IsCompanyEvaluationSubmitted(record);
            bool coordinatorApproved = ReadBool(record, "coordinatorApproved");
            string companyScore = Read(record, "companyScore");
            bool hasAcademicScore = HasNumber(record, "academicScore");
            string academicScore = hasAcademicScore ? Read(record, "academicScore") : string.Empty;
            string proposed = "Waiting for company evaluation";
            if (companySubmitted && hasAcademicScore)
            {
                double academicWeight = ReadDouble(record, "academicWeight", 0.6);
                double companyWeight = ReadDouble(record, "companyWeight", 0.4);
                double totalWeight = academicWeight + companyWeight;
                if (totalWeight <= 0) { academicWeight = 0.6; companyWeight = 0.4; totalWeight = 1; }
                double score = ReadDouble(record, "academicScore", 0) * academicWeight / totalWeight
                    + ReadDouble(record, "companyScore", 0) * companyWeight / totalWeight;
                proposed = coordinatorApproved && HasNumber(record, "finalScore")
                    ? Read(record, "finalScore") + " / 100 (approved)"
                    : score.ToString("0.##") + " / 100 · awaiting university approval";
            }
            return new AcademicEvaluationItem
            {
                PlacementId = row.PlacementId,
                EvaluationId = evaluation == null ? string.Empty : evaluation.Id,
                StudentUid = row.StudentUid,
                StudentName = row.StudentName,
                StudentNumber = row.StudentNumber,
                CompanyName = row.CompanyName,
                PlacementStatus = row.PlacementStatus,
                AcademicScoreInput = academicScore,
                AcademicScoreLabel = hasAcademicScore ? academicScore + " / 100" : "Not entered",
                AcademicFeedback = Read(record, "academicFeedback"),
                CompanyScoreLabel = HasNumber(record, "companyScore") ? companyScore + " / 100" : "Not received",
                CompanyStatusLabel = companySubmitted ? "Company evaluation received" : "Waiting for company evaluation",
                ApprovalStatusLabel = coordinatorApproved ? "University approval complete" : academicSubmitted ? "Waiting for university approval" : "Not submitted",
                ProposedFinalScoreLabel = proposed,
                AcademicSubmitted = academicSubmitted,
                CoordinatorApproved = coordinatorApproved,
                CanSave = !academicSubmitted && !coordinatorApproved,
                CanSubmit = !academicSubmitted && !coordinatorApproved && companySubmitted,
                EvaluationStatusLabel = coordinatorApproved ? "Approved" : academicSubmitted ? "Submitted" : companySubmitted ? "Ready for your evaluation" : "Awaiting company"
            };
        }

        private static Dictionary<string, bool> GetCompanyIds(WorkspaceData data, string supervisorUid)
        {
            var companyIds = new Dictionary<string, bool>(StringComparer.Ordinal);
            var user = Find(data.Users, supervisorUid);
            string directCompanyId = Read(user, "companyId");
            if (!string.IsNullOrWhiteSpace(directCompanyId)) companyIds[directCompanyId] = true;
            foreach (var item in data.Companies.Properties())
            {
                var company = item.Value as JObject;
                if (company == null) continue;
                var supervisorUids = company["supervisorUids"] as JObject;
                bool listed = supervisorUids != null && ReadBool(supervisorUids, supervisorUid);
                listed = listed || Same(Read(company, "supervisorUid"), supervisorUid);
                if (listed) companyIds[item.Name] = true;
            }
            return companyIds;
        }

        private static bool IsConfirmedPlacement(WorkspaceData data, string placementId)
        {
            var placement = Find(data.Placements, placementId);
            if (placement == null) return false;
            string status = Normalize(Read(placement, "status"));
            if (status == "pending" || status == "rejected" || status == "declined" || status == "cancelled") return false;
            if (status == "confirmed" || status == "accepted" || status == "inprogress" || status == "active" || status == "ongoing" || status == "completed") return true;
            string studentUid = Read(placement, "studentUid");
            foreach (var item in data.Applications.Properties())
            {
                var application = item.Value as JObject;
                if (application == null || !Same(Read(application, "studentUid"), studentUid)) continue;
                if (Same(Read(application, "id"), Read(placement, "applicationId")) || Same(item.Name, Read(placement, "applicationId")))
                    return ReadBool(application, "isConfirmedPlacement") || Normalize(Read(application, "status")) == "accepted";
            }
            return true;
        }

        private static EvaluationRecord FindEvaluation(JObject evaluations, string placementId, string studentUid, string academicUid)
        {
            foreach (var item in evaluations.Properties())
            {
                var record = item.Value as JObject;
                if (record == null || !Same(Read(record, "studentUid"), studentUid)) continue;
                if (!Same(Read(record, "placementId"), placementId)) continue;
                string assignedAcademic = Read(record, "academicSupervisorUid");
                if (!string.IsNullOrWhiteSpace(academicUid) && !string.IsNullOrWhiteSpace(assignedAcademic) && !Same(assignedAcademic, academicUid)) continue;
                return new EvaluationRecord { Id = item.Name, Value = record };
            }
            return null;
        }

        private static bool IsCompanyEvaluationSubmitted(JObject record)
        {
            if (record == null) return false;
            if (record["companySubmitted"] != null && record["companySubmitted"].Type != JTokenType.Null)
                return ReadBool(record, "companySubmitted");
            return HasNumber(record, "companyScore");
        }

        private static bool IsApprovedLog(JObject log)
        {
            if (log["isApproved"] != null && log["isApproved"].Type != JTokenType.Null) return ReadBool(log, "isApproved");
            return Normalize(Read(log, "status")) == "approved";
        }

        private static bool IsCompletedTask(string status)
        {
            string value = Normalize(status);
            return value == "completed" || value == "complete" || value == "done" || value == "approved";
        }

        private static bool IsSubmittedTask(string status)
        {
            string value = Normalize(status);
            return value == "submitted" || value == "inreview" || value == "underreview";
        }

        private static JObject Find(JObject root, string key)
        {
            return root == null || string.IsNullOrWhiteSpace(key) ? null : root[key] as JObject;
        }

        private static string Read(JObject obj, string key, string fallback = "")
        {
            if (obj == null || obj[key] == null || obj[key].Type == JTokenType.Null) return fallback;
            string value = obj[key].ToString();
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static bool ReadBool(JObject obj, string key)
        {
            bool value;
            return obj != null && obj[key] != null && (obj[key].Type == JTokenType.Boolean
                ? obj[key].Value<bool>() : bool.TryParse(obj[key].ToString(), out value) && value);
        }

        private static int? ReadNullableInt(JObject obj, string key)
        {
            int value;
            return obj != null && obj[key] != null && obj[key].Type != JTokenType.Null
                && int.TryParse(obj[key].ToString(), out value) ? (int?)value : null;
        }

        private static int ReadInt(JObject obj, string key)
        {
            return ReadNullableInt(obj, key) ?? 0;
        }

        private static double ReadDouble(JObject obj, string key, double fallback)
        {
            double value;
            return obj != null && obj[key] != null && double.TryParse(obj[key].ToString(), out value) ? value : fallback;
        }

        private static bool HasNumber(JObject obj, string key)
        {
            double value;
            return obj != null && obj[key] != null && obj[key].Type != JTokenType.Null && double.TryParse(obj[key].ToString(), out value);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().Replace(" ", string.Empty).Replace("_", string.Empty).ToLowerInvariant();
        }

        private static string Humanize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "In progress" : value.Replace("_", " ").Trim();
        }

        private static string GetInitials(string name)
        {
            var parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "ST";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private static bool Same(string left, string right)
        {
            return !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right)
                && string.Equals(left, right, StringComparison.Ordinal);
        }

        private static bool IsValidFirebaseKey(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Length <= 128
                && value.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/', '?', '&' }) < 0;
        }

        private static void RequireIdentity(string uid, string idToken)
        {
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(idToken))
                throw new UnauthorizedAccessException("A verified supervisor session is required.");
        }

        private sealed class WorkspaceData
        {
            public JObject Placements { get; set; }
            public JObject Students { get; set; }
            public JObject Users { get; set; }
            public JObject Companies { get; set; }
            public JObject Tasks { get; set; }
            public JObject TrainingLogs { get; set; }
            public JObject Evaluations { get; set; }
            public JObject Applications { get; set; }
        }

        private sealed class ProgressRow
        {
            public JObject Placement { get; set; }
            public string PlacementId { get; set; }
            public string StudentUid { get; set; }
        }

        private sealed class EvaluationRecord
        {
            public string Id { get; set; }
            public JObject Value { get; set; }
        }
    }
}
