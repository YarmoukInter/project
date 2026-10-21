using System.Collections.Generic;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;
namespace Yarmouk.Services
{
    public class EvaluationService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        public Task<Evaluation> GetByIdAsync(string id, string idToken)
        {
            return firebase.GetAsync<Evaluation>(DatabasePaths.Evaluations + "/" + id, idToken);
        }
    }
}
