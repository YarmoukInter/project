using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace Yarmouk.Helpers
{
    // Firebase database rules enforce permissions. Never use a client-supplied UID for authorization.
    public class FirebaseHelper
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly string databaseUrl;
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        public FirebaseHelper()
        {
            databaseUrl = ConfigurationManager.AppSettings["FirebaseDatabaseUrl"];
            if (string.IsNullOrWhiteSpace(databaseUrl) || !Uri.IsWellFormedUriString(databaseUrl, UriKind.Absolute)
                || new Uri(databaseUrl).Scheme != Uri.UriSchemeHttps)
                throw new ConfigurationErrorsException("Set FirebaseDatabaseUrl to a valid HTTPS URL in Web.config.");
            databaseUrl = databaseUrl.TrimEnd('/');
        }
        private string GetUrl(string path, string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken)) throw new UnauthorizedAccessException("Sign in first.");
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Database path required.");
            foreach (string segment in path.Split('/'))
                if (string.IsNullOrWhiteSpace(segment) || segment == "." || segment == ".."
                    || segment.IndexOfAny(new[] { '.', '#', '$', '[', ']', '?', '&' }) >= 0)
                    throw new ArgumentException("Invalid Firebase path.");
            return databaseUrl + "/" + path + ".json?auth=" + Uri.EscapeDataString(idToken);
        }
        private async Task<string> SendAsync(HttpMethod method, string path, string idToken, object value = null)
        {
            using (var request = new HttpRequestMessage(method, GetUrl(path, idToken)))
            {
                if (value != null)
                    request.Content = new StringContent(JsonConvert.SerializeObject(value, Settings), Encoding.UTF8, "application/json");
                using (var response = await Client.SendAsync(request).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                        throw new InvalidOperationException("Firebase HTTP error " + (int)response.StatusCode);
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }
        public async Task<T> GetAsync<T>(string path, string idToken)
        {
            string json = await SendAsync(HttpMethod.Get, path, idToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
        public async Task SaveAsync<T>(string path, T value, string idToken)
        {
            if (value == null) throw new ArgumentNullException("value");
            await SendAsync(HttpMethod.Put, path, idToken, value).ConfigureAwait(false);
        }
        public async Task UpdateAsync(string path, object changes, string idToken)
        {
            if (changes == null) throw new ArgumentNullException("changes");
            await SendAsync(new HttpMethod("PATCH"), path, idToken, changes).ConfigureAwait(false);
        }
        public async Task DeleteAsync(string path, string idToken)
        {
            await SendAsync(HttpMethod.Delete, path, idToken).ConfigureAwait(false);
        }
    }
}
