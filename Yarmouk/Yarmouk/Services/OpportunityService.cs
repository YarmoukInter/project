using System.Collections.Generic;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;
namespace Yarmouk.Services
{
    public class OpportunityService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        public async Task<List<Opportunity>> GetAllAsync(string idToken)
        {
            var data = await firebase.GetAsync<Dictionary<string, Opportunity>>(DatabasePaths.Opportunities, idToken);
            return data == null ? new List<Opportunity>() : new List<Opportunity>(data.Values);
        }
    }
}
