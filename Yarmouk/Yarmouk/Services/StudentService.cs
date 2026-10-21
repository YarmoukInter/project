using System.Collections.Generic;
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
        public async Task UpdateProfileAsync(string uid, User profile,
            List<string> softSkills, List<string> technicalSkills, string idToken)
        {
            // only these fields are editable, everything else stays untouched
            await firebase.UpdateAsync(DatabasePaths.Users + "/" + uid, new
            {
                fullName = profile.FullName,
                phone = profile.Phone,
                cvLink = profile.CvLink,
                portfolioLink = profile.PortfolioLink
            }, idToken);

            await firebase.UpdateAsync(DatabasePaths.Students + "/" + uid, new
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
    }
}