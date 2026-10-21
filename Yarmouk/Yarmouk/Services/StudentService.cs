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
        public Task SetAcceptedApplicationAsync(string verifiedUid, string applicationId, string idToken)
        {
            if (string.IsNullOrWhiteSpace(verifiedUid) || string.IsNullOrWhiteSpace(applicationId))
                throw new System.ArgumentException("Student and application are required.");
            return firebase.UpdateAsync(DatabasePaths.Students + "/" + verifiedUid,
                new { acceptedApplicationId = applicationId }, idToken);
        }
    }
}
