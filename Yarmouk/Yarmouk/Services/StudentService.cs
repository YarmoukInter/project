using System.Collections.Generic;
using System;
using System.Threading.Tasks;
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

        public Task SetAcceptedApplicationAsync(string verifiedUid, string applicationId, string idToken)
        {
            if (string.IsNullOrWhiteSpace(verifiedUid) || string.IsNullOrWhiteSpace(applicationId))
                throw new System.ArgumentException("Student and application are required.");
            return firebase.UpdateAsync(DatabasePaths.Students + "/" + verifiedUid,
                new { acceptedApplicationId = applicationId }, idToken);

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
