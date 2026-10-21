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
            var opportunities = new List<Opportunity>();
            if (data == null) return opportunities;
            foreach (var item in data)
            {
                if (item.Value == null) continue;
                if (string.IsNullOrWhiteSpace(item.Value.Id)) item.Value.Id = item.Key;
                opportunities.Add(item.Value);
            }
            return opportunities;
        }

        public async Task<Opportunity> GetByIdAsync(string id, string idToken)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var opportunity = await firebase.GetAsync<Opportunity>(DatabasePaths.Opportunities + "/" + id, idToken);
            if (opportunity != null && string.IsNullOrWhiteSpace(opportunity.Id)) opportunity.Id = id;
            return opportunity;
        }
    }
}
