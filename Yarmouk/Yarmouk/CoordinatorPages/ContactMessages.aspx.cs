using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Services;
namespace Yarmouk.CoordinatorPages
{
    public partial class ContactMessages : CoordinatorPage
    {
        protected override Task<object> LoadDataAsync(AuthSession session)
        { return Service.GetMessagesAsync(session); }
        protected override Task<string> SaveDataAsync(string action, JObject details, AuthSession session)
        { return Service.UpdateMessageAsync(action, (string)details["id"], (bool?)details["value"] == true, session); }
    }
}

namespace Yarmouk.Helpers
{
    public static class CoordinatorContactSubmission
    {
        public static Task SaveContactMessageAsync(this FirebaseHelper firebase, Yarmouk.Models.ContactMessage message)
        { return new Yarmouk.CoordinatorPages.CoordinatorDatabase().SaveContactAsync(message); }
    }
}
