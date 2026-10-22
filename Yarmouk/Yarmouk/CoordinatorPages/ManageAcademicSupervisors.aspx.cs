using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;
namespace Yarmouk.CoordinatorPages
{
    public partial class ManageAcademicSupervisors : CoordinatorPage
    {
        protected override Task<object> LoadDataAsync(AuthSession session)
        { return Service.GetDirectoryAsync("academic", session); }
        protected override Task<string> SaveDataAsync(string action, JObject details, AuthSession session)
        {
            switch (action)
            {
                case "save": return Service.SaveDirectoryAsync("academic", details.ToObject<DirectoryEntry>(), session);
                case "active": return Service.SetActiveAsync((string)details["id"], "academic", (bool?)details["value"] == true, session);

                default: throw new ArgumentException("Unknown account action.");
            }
        }
    }
}
