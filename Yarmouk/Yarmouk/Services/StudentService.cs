using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
namespace Yarmouk.Services
{
    public class StudentService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        // verifiedUid must come from server-verified authentication.
        public Task<Student> GetStudentAsync(string verifiedUid, string idToken)
        {
            return firebase.GetAsync<Student>(DatabasePaths.Students + "/" + verifiedUid, idToken);
        }
        public Task<User> GetUserAsync(string verifiedUid, string idToken)
        {
            return firebase.GetAsync<User>(DatabasePaths.Users + "/" + verifiedUid, idToken);
        }

        public Task<Major> GetMajorAsync(string majorId, string idToken)
        {
            return firebase.GetAsync<Major>(DatabasePaths.Majors + "/" + majorId, idToken);
        }
        public Task UpdateProfileAsync(string uid, User profile, string idToken)
        {
            // only these fields are editable, everything else stays untouched
            return firebase.UpdateAsync(DatabasePaths.Users + "/" + uid, new
            {
                fullName = profile.FullName,
                phone = profile.Phone,
                cvLink = profile.CvLink,
                portfolioLink = profile.PortfolioLink
            }, idToken);

        }

        public Task UpdateSkillsAsync(string verifiedUid, List<string> softSkills,
            List<string> technicalSkills, string idToken)
        {
            if (string.IsNullOrWhiteSpace(verifiedUid))
                throw new System.ArgumentException("Student UID is required.");
            if (softSkills == null || technicalSkills == null)
                throw new System.ArgumentNullException("Skill lists are required.");
            return firebase.UpdateAsync(DatabasePaths.Students + "/" + verifiedUid, new
            {
                softSkills = softSkills,
                technicalSkills = technicalSkills
            }, idToken);
        }

        public Task UpdateProfileImageFileNameAsync(string verifiedUid, string fileName, string idToken)
        {
            if (string.IsNullOrWhiteSpace(verifiedUid))
                throw new ArgumentException("Student UID is required.");
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Profile image file name is required.");
            return firebase.UpdateAsync(DatabasePaths.Students + "/" + verifiedUid, new
            {
                profileImageFileName = fileName
            }, idToken);
        }

        public async Task SetAcceptedApplicationAsync(string verifiedUid, string applicationId, string idToken)
        {
            if (string.IsNullOrWhiteSpace(verifiedUid) || string.IsNullOrWhiteSpace(applicationId))
                throw new System.ArgumentException("Student and application are required.");
            var student = await GetStudentAsync(verifiedUid, idToken);
            var application = await new ApplicationService().GetByIdAsync(applicationId, idToken);
            if (student == null || application == null || application.StudentUid != verifiedUid)
                throw new UnauthorizedAccessException("This offer does not belong to the signed-in student.");
            if (!string.IsNullOrEmpty(student.AcceptedApplicationId) && student.AcceptedApplicationId != applicationId)
                throw new ArgumentException("You have already confirmed another internship offer.");
            string status = (application.Status ?? "").Replace("_", "").Replace("-", "").Replace(" ", "").ToLowerInvariant();
            if (status != "offered" && status != "offerreceived" && !(status == "accepted" && student.AcceptedApplicationId == applicationId))
                throw new ArgumentException("This offer is no longer available.");
            var opportunity = await new OpportunityService().GetByIdAsync(application.OpportunityId, idToken);
            if (opportunity == null || string.IsNullOrWhiteSpace(opportunity.CompanyId))
                throw new ArgumentException("The internship opportunity is unavailable.");
            var company = await firebase.GetAsync<Company>(DatabasePaths.Companies + "/" + opportunity.CompanyId, idToken);
            if (company == null || !company.IsApproved)
                throw new ArgumentException("The internship company is unavailable.");

            // A stable key makes retrying confirmation safe, including older accepted offers
            // whose confirmation never created a placement.
            string placementId = "PLACEMENT_" + applicationId;
            var placement = await firebase.GetAsync<JObject>(DatabasePaths.Placements + "/" + placementId, idToken);
            if (placement != null && ((string)placement["studentUid"] != verifiedUid
                || (string)placement["opportunityId"] != application.OpportunityId))
                throw new UnauthorizedAccessException("The existing placement does not match this offer.");
            string studentPath = DatabasePaths.Students + "/" + verifiedUid + "/";
            string applicationPath = DatabasePaths.Applications + "/" + applicationId + "/";
            string placementPath = DatabasePaths.Placements + "/" + placementId + "/";
            var changes = new Dictionary<string, object> {
                { studentPath + "acceptedApplicationId", applicationId },
                { studentPath + "companyId", opportunity.CompanyId },
                { studentPath + "requiredTrainingHours", Math.Max(0, opportunity.RequiredTrainingHours) },
                { applicationPath + "status", "accepted" },
                { applicationPath + "isConfirmedPlacement", true },
                { applicationPath + "placementId", placementId },
                { placementPath + "id", placementId },
                { placementPath + "studentUid", verifiedUid },
                { placementPath + "applicationId", applicationId },
                { placementPath + "opportunityId", application.OpportunityId },
                { placementPath + "companyId", opportunity.CompanyId },
                { placementPath + "academicSupervisorUid", student.AcademicSupervisorId ?? "" },
                { placementPath + "companySupervisorUid", company.SupervisorUid ?? "" }
            };
            if (placement == null)
            {
                string startDate = DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-dd");
                changes[placementPath + "status"] = "Active";
                changes[placementPath + "startDate"] = startDate;
                changes[placementPath + "requiredHours"] = Math.Max(0, opportunity.RequiredTrainingHours);
                changes[placementPath + "createdAtUnixMs"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                changes[studentPath + "internshipStatus"] = "InProgress";
                if (string.IsNullOrEmpty(student.TrainingStartDate)) changes[studentPath + "trainingStartDate"] = startDate;
            }
            // Confirm the offer and establish its placement together, without partial saves.
            await firebase.UpdateRootAsync(changes, idToken);
        }

        // Tasks belong only to the internship offer confirmed by this student.
        public async Task<string> GetActiveAcceptedOpportunityIdAsync(string verifiedUid, string idToken)
        {
            var student = await GetStudentAsync(verifiedUid, idToken);
            if (student == null || string.IsNullOrWhiteSpace(student.AcceptedApplicationId)) return null;
            var application = await new ApplicationService().GetByIdAsync(student.AcceptedApplicationId, idToken);
            if (application == null || !string.Equals(application.StudentUid, verifiedUid, StringComparison.Ordinal)
                || (!string.Equals(application.Status, "accepted", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(application.Status, "offered", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(application.Status, "offerreceived", StringComparison.OrdinalIgnoreCase))
                || string.IsNullOrWhiteSpace(application.OpportunityId)) return null;
            var opportunity = await firebase.GetAsync<Opportunity>(DatabasePaths.Opportunities + "/" + application.OpportunityId, idToken);
            return opportunity != null && opportunity.IsActive ? application.OpportunityId : null;
        }
    }
}
