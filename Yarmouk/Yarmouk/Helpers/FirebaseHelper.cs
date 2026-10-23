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
        private string GetUrl(string path, string idToken, string query = null, bool allowRootUpdate = false)
        {
            if (string.IsNullOrWhiteSpace(idToken)) throw new UnauthorizedAccessException("Sign in first.");
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Database path required.");
            if (allowRootUpdate && path == "/")
                return databaseUrl + "/.json?auth=" + Uri.EscapeDataString(idToken);
            foreach (string segment in path.Split('/'))
                if (string.IsNullOrWhiteSpace(segment) || segment == "." || segment == ".."
                    || segment.IndexOfAny(new[] { '.', '#', '$', '[', ']', '?', '&' }) >= 0)
                    throw new ArgumentException("Invalid Firebase path.");
            return databaseUrl + "/" + path + ".json?" + (string.IsNullOrEmpty(query) ? "" : query + "&") + "auth=" + Uri.EscapeDataString(idToken);
        }
        private async Task<string> SendAsync(HttpMethod method, string path, string idToken, object value = null, string query = null, bool allowRootUpdate = false)
        {
            using (var request = new HttpRequestMessage(method, GetUrl(path, idToken, query, allowRootUpdate)))
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
        public async Task<T> GetByChildAsync<T>(string path, string child, string value, string idToken)
        {
            if (string.IsNullOrWhiteSpace(child)) throw new ArgumentException("Child field required.");
            foreach (char character in child)
                if (!char.IsLetterOrDigit(character) && character != '_') throw new ArgumentException("Invalid Firebase child field.");
            string query = "orderBy=" + Uri.EscapeDataString("\"" + child + "\"") +
                "&equalTo=" + Uri.EscapeDataString(JsonConvert.SerializeObject(value));
            string json = await SendAsync(HttpMethod.Get, path, idToken, null, query).ConfigureAwait(false);
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
        public async Task UpdateRootAsync(System.Collections.Generic.IDictionary<string, object> changes, string idToken)
        {
            if (changes == null || changes.Count == 0) throw new ArgumentException("Updates are required.");
            await SendAsync(new HttpMethod("PATCH"), "/", idToken, changes, allowRootUpdate: true).ConfigureAwait(false);
        }
        public async Task DeleteAsync(string path, string idToken)
        {
            await SendAsync(HttpMethod.Delete, path, idToken).ConfigureAwait(false);
        }
    }
}
