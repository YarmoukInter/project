using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
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
            databaseUrl = SiteSettingsStore.GetFirebaseDatabaseUrl();
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

        // Conditional writes protect a single record from concurrent updates.
        public async Task TransactAsync(string path, string idToken, Func<JObject, bool> mutate, JObject initialValue = null)
        {
            string url = GetUrl(path, idToken);
            for (int attempt = 0; attempt < 8; attempt++)
            {
                JObject snapshot;
                string etag;
                using (var read = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    read.Headers.TryAddWithoutValidation("X-Firebase-ETag", "true");
                    using (var response = await Client.SendAsync(read).ConfigureAwait(false))
                    {
                        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Firebase HTTP error " + (int)response.StatusCode);
                        // Firebase uses the unquoted sentinel null_etag for absent
                        // records. HttpClient's typed ETag parser rejects that value.
                        System.Collections.Generic.IEnumerable<string> versions;
                        if (!response.Headers.TryGetValues("ETag", out versions))
                            throw new InvalidOperationException("Firebase did not return a transaction version.");
                        etag = System.Linq.Enumerable.FirstOrDefault(versions);
                        if (string.IsNullOrWhiteSpace(etag)) throw new InvalidOperationException("Firebase did not return a transaction version.");
                        snapshot = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }
                }
                if (!mutate(snapshot)) return;
                if (snapshot == null) snapshot = initialValue;
                using (var write = new HttpRequestMessage(HttpMethod.Put, url))
                {
                    write.Headers.TryAddWithoutValidation("if-match", etag);
                    write.Content = new StringContent(JsonConvert.SerializeObject(snapshot), Encoding.UTF8, "application/json");
                    using (var response = await Client.SendAsync(write).ConfigureAwait(false))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.PreconditionFailed) continue;
                        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Firebase HTTP error " + (int)response.StatusCode);
                        return;
                    }
                }
            }
            throw new InvalidOperationException("The record changed during saving. Please try again.");
        }
    }
}
