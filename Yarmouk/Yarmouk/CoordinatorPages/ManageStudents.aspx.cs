using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;
namespace Yarmouk.CoordinatorPages
{
    public partial class ManageStudents : CoordinatorPage
    {
        private readonly FirebaseAdminAccountService adminAccounts = new FirebaseAdminAccountService();
        protected override Task<object> LoadDataAsync(AuthSession session)
        { return Service.GetDirectoryAsync("student", session); }
        protected override Task<string> SaveDataAsync(string action, JObject details, AuthSession session)
        {
            switch (action)
            {
                case "save": return Service.SaveDirectoryAsync("student", details.ToObject<DirectoryEntry>(), session);
                case "active": return Service.SetActiveAsync((string)details["id"], "student", (bool?)details["value"] == true, session);
                case "assign": return Service.AssignSupervisorAsync((string)details["id"], (string)details["supervisor"], session);
                case "credentials": return UpdateCredentialsAsync(details, session);
                case "deleteStudent": return DeleteStudentAsync(details, session);
                default: throw new ArgumentException("Unknown account action.");
            }
        }
        private async Task<string> UpdateCredentialsAsync(JObject details, AuthSession session)
        {
            string uid = (string)details["id"], email = ((string)details["email"] ?? "").Trim();
            string password = (string)details["password"] ?? "";
            await Service.EnsureManagedStudentAsync(uid, session);
            if (email.Length > 0 && !AuthPolicy.ValidEmail(email)) throw new ArgumentException("Enter a valid email address.");
            return await adminAccounts.UpdateStudentCredentialsAsync(uid, email, password);
        }
        private async Task<string> DeleteStudentAsync(JObject details, AuthSession session)
        {
            string uid = (string)details["id"];
            await Service.EnsureManagedStudentAsync(uid, session);
            return await adminAccounts.DeleteStudentAsync(uid);
        }
    }
}
