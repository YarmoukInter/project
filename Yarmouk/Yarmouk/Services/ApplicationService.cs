using System.Collections.Generic;
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
    }
}
