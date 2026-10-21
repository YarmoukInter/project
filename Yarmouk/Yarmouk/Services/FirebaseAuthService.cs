using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;

namespace Yarmouk.Services
{
    public sealed class AuthException : Exception
    {
        public AuthException(string message, string code = null) : base(message) { Code = code; }
        public string Code { get; private set; }
    }

    public sealed class AuthSession
    {
        public string Uid { get; set; }
        public string Email { get; set; }
        public string IdToken { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public User User { get; set; }
    }

    public sealed class FirebaseAuthService
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private readonly HttpClient client;
        private readonly string apiKey;
        public FirebaseAuthService() : this(Client, ConfigurationManager.AppSettings["FirebaseWebApiKey"]) { }
        public FirebaseAuthService(HttpClient client, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ConfigurationErrorsException("Configure FirebaseWebApiKey on the server.");
            this.client = client;
            this.apiKey = apiKey;
        }
        private async Task<JObject> PostAsync(string action, object body)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"))
            using (var response = await client.PostAsync("https://identitytoolkit.googleapis.com/v1/accounts:" + action + "?key=" + Uri.EscapeDataString(apiKey), content).ConfigureAwait(false))
            {
                var json = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                if (!response.IsSuccessStatusCode)
                {
                    string code = (string)json.SelectToken("error.message") ?? "";
                    if (code.StartsWith("TOO_MANY_ATTEMPTS")) throw new AuthException("Too many attempts. Please try again later.", code);
                    if (code.StartsWith("WEAK_PASSWORD") || code.StartsWith("PASSWORD_DOES_NOT_MEET_REQUIREMENTS"))
                        throw new AuthException("Choose a stronger password that meets the university password policy.", code);
                    throw new AuthException("Unable to authenticate. Check your details or contact the internship coordinator.", code);
                }
                return json;
            }
        }
        public async Task<AuthSession> SignInAsync(string email, string password)
        {
            var json = await PostAsync("signInWithPassword", new { email, password, returnSecureToken = true }).ConfigureAwait(false);
            return ReadSession(json, email);
        }
        public async Task<AuthSession> SignInWithIdTokenAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken) || idToken.Length > 8192)
                throw new AuthException("The social sign-in token is missing or invalid.");
            var json = await PostAsync("lookup", new { idToken }).ConfigureAwait(false);
            var accounts = json["users"] as JArray;
            if (accounts == null || accounts.Count != 1 || (bool?)accounts[0]["disabled"] == true)
                throw new AuthException("This social account is unavailable. Contact the internship coordinator.");
            string uid = (string)accounts[0]["localId"];
            string email = (string)accounts[0]["email"];
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(email))
                throw new AuthException("Firebase did not return a complete social account.");
            return new AuthSession { Uid = uid, Email = email, IdToken = idToken,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(50) };
        }
        private static AuthSession ReadSession(JObject json, string email)
        {
            string uid = (string)json["localId"], token = (string)json["idToken"];
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token)) throw new AuthException("Authentication returned an incomplete response.");
            int seconds;
            if (!int.TryParse((string)json["expiresIn"], out seconds) || seconds <= 60) seconds = 60;
            return new AuthSession { Uid = uid, Email = (string)json["email"] ?? email, IdToken = token,
                ExpiresUtc = DateTime.UtcNow.AddSeconds(Math.Min(seconds - 30, 1800)) };
        }
        public async Task VerifyAsync(AuthSession session)
        {
            if (session == null || session.ExpiresUtc <= DateTime.UtcNow) throw new AuthException("Your session expired. Sign in again.");
            var json = await PostAsync("lookup", new { idToken = session.IdToken }).ConfigureAwait(false);
            var account = json["users"] as JArray;
            if (account == null || account.Count != 1 || (string)account[0]["localId"] != session.Uid || (bool?)account[0]["disabled"] == true)
                throw new AuthException("Your session is no longer available. Sign in again.");
        }
        public async Task<AuthSession> ChangePasswordAsync(AuthSession session, string current, string next, string confirmation)
        {
            string error = AuthPolicy.PasswordError(current, next, confirmation);
            if (error != null) throw new AuthException(error);
            // Reauthentication is mandatory; never trust an email/UID posted by the browser.
            var verified = await SignInAsync(session.Email, current).ConfigureAwait(false);
            if (verified.Uid != session.Uid) throw new AuthException("The current password could not be verified.");
            var json = await PostAsync("update", new { idToken = verified.IdToken, password = next, returnSecureToken = true }).ConfigureAwait(false);
            return ReadSession(json, session.Email);
        }
        public async Task SendRecoveryAsync(string email)
        {
            try
            {
                await PostAsync("sendOobCode", new { requestType = "PASSWORD_RESET", email }).ConfigureAwait(false);
                System.Diagnostics.Trace.TraceInformation("Firebase accepted a password recovery request.");
            }
            catch (AuthException ex)
            {
                // Never reveal whether an email belongs to an account.
                System.Diagnostics.Trace.TraceWarning("Firebase password recovery response: " + (ex.Code ?? "AUTH_ERROR"));
                if (ex.Code != null && ex.Code.StartsWith("EMAIL_NOT_FOUND", StringComparison.Ordinal)) return;
                if (ex.Code != null && ex.Code.StartsWith("USER_DISABLED", StringComparison.Ordinal)) return;
                if (ex.Code != null && ex.Code.StartsWith("OPERATION_NOT_ALLOWED", StringComparison.Ordinal))
                    throw new AuthException("Password reset is disabled in Firebase Authentication. Ask the coordinator to enable Email/Password sign-in.", ex.Code);
                if (ex.Code != null && (ex.Code.StartsWith("TOO_MANY_ATTEMPTS", StringComparison.Ordinal)
                    || ex.Code.StartsWith("RESET_PASSWORD_EXCEED_LIMIT", StringComparison.Ordinal)
                    || ex.Code.StartsWith("QUOTA_EXCEEDED", StringComparison.Ordinal)))
                    throw new AuthException("Too many password reset requests. Please try again later.", ex.Code);
                throw new AuthException("Firebase could not accept the reset request. Please try again later.", ex.Code);
            }
        }
    }
}
