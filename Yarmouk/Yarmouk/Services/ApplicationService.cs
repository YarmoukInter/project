using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;
using System.Linq;
namespace Yarmouk.Services
{
    public class ApplicationService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        public Task<Application> GetByIdAsync(string id, string idToken)
        {
            return firebase.GetAsync<Application>(DatabasePaths.Applications + "/" + id, idToken);
        }

        public async Task<List<Application>> GetForStudentAsync(string verifiedUid, string idToken)
        {
            Dictionary<string, Application> data;
            try
            {
                data = await firebase.GetByChildAsync<Dictionary<string, Application>>(
                    DatabasePaths.Applications, "studentUid", verifiedUid, idToken);
            }
            catch (InvalidOperationException ex)
            {
                // Firebase returns HTTP 400 for this query until studentUid is
                // indexed. The small demo database can still be read and filtered
                // locally while the index rule is being published.
                if (ex.Message != "Firebase HTTP error 400") throw;
                data = await firebase.GetAsync<Dictionary<string, Application>>(
                    DatabasePaths.Applications, idToken);
            }
            var results = new List<Application>();
            if (data == null) return results;
            foreach (var item in data)
            {
                if (item.Value == null || item.Value.StudentUid != verifiedUid) continue;
                if (string.IsNullOrWhiteSpace(item.Value.Id)) item.Value.Id = item.Key;
                results.Add(item.Value);
            }
            results.Sort((a, b) => b.AppliedAtUnixMs.CompareTo(a.AppliedAtUnixMs));
            return results;
        }

        public async Task SaveForStudentAsync(Application application, string verifiedUid, string idToken)
        {
            if (application == null) throw new ArgumentNullException("application");
            if (application.StudentUid != verifiedUid) throw new UnauthorizedAccessException("Student mismatch.");
            if (string.IsNullOrWhiteSpace(application.Id)) application.Id = Guid.NewGuid().ToString("N");
            if (application.AppliedAtUnixMs <= 0) application.AppliedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await firebase.SaveAsync(DatabasePaths.Applications + "/" + application.Id, application, idToken);
        }

        public async Task<bool> HasConfirmedInternshipAsync(string verifiedUid, string idToken)
        {
            if (string.IsNullOrWhiteSpace(verifiedUid)) throw new UnauthorizedAccessException("Student identity required.");
            var studentTask = firebase.GetAsync<Student>(DatabasePaths.Students + "/" + verifiedUid, idToken);
            var applicationsTask = GetForStudentAsync(verifiedUid, idToken);
            var placementsTask = firebase.GetAsync<Dictionary<string, Placement>>(DatabasePaths.Placements, idToken);
            await Task.WhenAll(studentTask, applicationsTask, placementsTask).ConfigureAwait(false);
            var student = await studentTask.ConfigureAwait(false);
            if (student != null && !string.IsNullOrWhiteSpace(student.AcceptedApplicationId)) return true;
            if ((await applicationsTask.ConfigureAwait(false)).Any(item => Normalize(item.Status) == "accepted")) return true;
            var placements = await placementsTask.ConfigureAwait(false);
            return placements != null && placements.Values.Any(item => item != null && item.StudentUid == verifiedUid
                && IsConfirmedPlacementStatus(item.Status));
        }

        public async Task ConfirmOfferAsync(Application application, Student student, Opportunity opportunity,
            string verifiedUid, string idToken)
        {
            if (application == null || student == null || opportunity == null)
                throw new ArgumentException("Application, student, and opportunity are required.");
            if (application.StudentUid != verifiedUid)
                throw new UnauthorizedAccessException("Student mismatch.");

            string placementId = "PLC_" + application.Id;
            var companyTask = firebase.GetAsync<Company>(DatabasePaths.Companies + "/" + opportunity.CompanyId, idToken);
            var existingPlacementTask = firebase.GetAsync<Placement>(DatabasePaths.Placements + "/" + placementId, idToken);
            var applicationsTask = GetForStudentAsync(verifiedUid, idToken);
            await Task.WhenAll(companyTask, existingPlacementTask, applicationsTask).ConfigureAwait(false);
            var company = await companyTask.ConfigureAwait(false);
            var existingPlacement = await existingPlacementTask.ConfigureAwait(false);
            if (existingPlacement != null && (existingPlacement.StudentUid != verifiedUid || existingPlacement.OpportunityId != opportunity.Id))
                throw new UnauthorizedAccessException("The existing placement does not match this offer.");

            string studentPath = DatabasePaths.Students + "/" + verifiedUid + "/";
            string applicationPath = DatabasePaths.Applications + "/" + application.Id + "/";
            string placementPath = DatabasePaths.Placements + "/" + placementId + "/";
            var changes = new Dictionary<string, object>
            {
                { studentPath + "acceptedApplicationId", application.Id },
                { studentPath + "companyId", opportunity.CompanyId },
                { studentPath + "internshipStatus", "InProgress" },
                { studentPath + "requiredTrainingHours", Math.Max(0, opportunity.RequiredTrainingHours) },
                { applicationPath + "status", "accepted" },
                { applicationPath + "isConfirmedPlacement", true },
                { applicationPath + "placementId", placementId },
                { placementPath + "id", placementId },
                { placementPath + "applicationId", application.Id },
                { placementPath + "studentUid", verifiedUid },
                { placementPath + "companyId", opportunity.CompanyId },
                { placementPath + "opportunityId", opportunity.Id },
                { placementPath + "academicSupervisorUid", student.AcademicSupervisorId ?? string.Empty },
                { placementPath + "companySupervisorUid", company == null ? string.Empty : company.SupervisorUid ?? string.Empty },
                { placementPath + "requiredHours", Math.Max(0, opportunity.RequiredTrainingHours) }
            };
            if (existingPlacement == null)
            {
                string startDate = DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-dd");
                changes[placementPath + "status"] = "Active";
                changes[placementPath + "startDate"] = startDate;
                changes[placementPath + "createdAtUnixMs"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (string.IsNullOrWhiteSpace(student.TrainingStartDate)) changes[studentPath + "trainingStartDate"] = startDate;
            }

            foreach (var other in (await applicationsTask.ConfigureAwait(false)).Where(item => item.Id != application.Id &&
                Normalize(item.Status) != "rejected" && Normalize(item.Status) != "declined" && Normalize(item.Status) != "withdrawn"))
                changes[DatabasePaths.Applications + "/" + other.Id + "/status"] = "withdrawn";

            // Application acceptance, placement creation, and competing-offer closure are
            // one root update: no user can be accepted without a usable placement.
            await firebase.UpdateRootAsync(changes, idToken).ConfigureAwait(false);
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Replace("_", string.Empty).Replace("-", string.Empty)
                .Replace(" ", string.Empty).ToLowerInvariant();
        }

        private static bool IsConfirmedPlacementStatus(string status)
        {
            string value = Normalize(status);
            return value == "confirmed" || value == "accepted" || value == "active" ||
                value == "inprogress" || value == "ongoing" || value == "completed";
        }

        public Task SetStatusAsync(string applicationId, string status, string idToken)
        {
            if (string.IsNullOrWhiteSpace(applicationId)) throw new ArgumentException("Application id required.");
            return firebase.UpdateAsync(DatabasePaths.Applications + "/" + applicationId,
                new { status = status }, idToken);
        }
    }
}
