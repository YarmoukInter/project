using System.Threading.Tasks;
using Yarmouk.Helpers;

namespace Yarmouk.Services
{
    // Firebase rules allow this one-way flag change for the signed-in account.
    public sealed class PasswordSetupService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        public async Task CompleteAsync(string verifiedUid, string idToken)
        {
            // RTDB PATCH expects an object; this leaf stores a boolean, so replace it with PUT.
            await firebase.SaveAsync(DatabasePaths.Users + "/" + verifiedUid + "/mustChangePassword",
                false, idToken).ConfigureAwait(false);
        }
    }
}
