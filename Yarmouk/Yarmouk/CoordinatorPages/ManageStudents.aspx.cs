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
        protected override Task<object> LoadDataAsync(AuthSession session)
        { return Service.GetDirectoryAsync("student", session); }
        protected override Task<string> SaveDataAsync(string action, JObject details, AuthSession session)
        {
            switch (action)
            {
                case "save": return Service.SaveDirectoryAsync("student", details.ToObject<DirectoryEntry>(), session);
                case "active": return Service.SetActiveAsync((string)details["id"], "student", (bool?)details["value"] == true, session);
                case "assign": return Service.AssignSupervisorAsync((string)details["id"], (string)details["supervisor"], session);
                default: throw new ArgumentException("Unknown account action.");
            }
        }
    }
}
