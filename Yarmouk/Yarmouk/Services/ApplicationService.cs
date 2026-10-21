using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;
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
            var data = await firebase.GetByChildAsync<Dictionary<string, Application>>(DatabasePaths.Applications, "studentUid", verifiedUid, idToken);
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

        public Task SetStatusAsync(string applicationId, string status, string idToken)
        {
            if (string.IsNullOrWhiteSpace(applicationId)) throw new ArgumentException("Application id required.");
            return firebase.UpdateAsync(DatabasePaths.Applications + "/" + applicationId,
                new { status = status }, idToken);
        }
    }
}
