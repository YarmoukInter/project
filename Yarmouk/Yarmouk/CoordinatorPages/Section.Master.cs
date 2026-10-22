using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class SectionMaster : MasterPage
    {
        protected string NotificationConfigJson
        {
            get
            {
                var session = Session[AuthGuard.SessionKey] as AuthSession;
                var user = session == null ? null : session.User;
                if (user == null || !user.IsActive || AuthPolicy.NormalizeRole(user.Role) != "coordinator") return "null";
                return JsonConvert.SerializeObject(new {
                    databaseUrl = ConfigurationManager.AppSettings["FirebaseDatabaseUrl"],
                    idToken = session.IdToken,
                    inboxUrl = ResolveUrl("~/CoordinatorPages/ContactMessages.aspx")
                }, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
            }
        }
        protected string SidebarCsrf { get { return (string)Session["SidebarCsrf"]; } }
        protected override void OnInit(EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            if (Session["SidebarCsrf"] == null) Session["SidebarCsrf"] = Guid.NewGuid().ToString("N");
            base.OnInit(e);
        }
        protected void SidebarLogout_Click(object sender, EventArgs e)
        {
            if (Request.Form["sidebarCsrf"] != SidebarCsrf) throw new HttpException(403, "Please reload the page and try again.");
            Session.Clear();
            Session.Abandon();
            Context.Items["CoordinatorSignedOut"] = true;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Redirect(ResolveUrl("~/PublicPages/Login.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }

    public abstract class CoordinatorPage : Page
    {
        protected string BackendJson { get; private set; } = "null";
        protected string CsrfToken { get { return (string)Session["CoordinatorCsrf"]; } }
        protected readonly CoordinatorService Service = new CoordinatorService();
        protected abstract Task<object> LoadDataAsync(AuthSession session);
        protected virtual Task<string> SaveDataAsync(string action, JObject details, AuthSession session)
        { throw new ArgumentException("This page is read-only."); }
        protected override void OnInit(EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
            if (Session["CoordinatorCsrf"] == null) Session["CoordinatorCsrf"] = Guid.NewGuid().ToString("N");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            base.OnInit(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterAsyncTask(new PageAsyncTask(LoadPageAsync));
        }
        private async Task LoadPageAsync()
        {
            if (Context.Items["CoordinatorSignedOut"] != null) return;
            object data = null;
            string error = null, notice = Session["CoordinatorNotice"] as string;
            Session.Remove("CoordinatorNotice");
            var session = Session[AuthGuard.SessionKey] as AuthSession;
            try
            {
                if (Request.HttpMethod == "POST")
                {
                    if (string.IsNullOrEmpty(CsrfToken) || Request.Form["adminCsrf"] != CsrfToken)
                        throw new ArgumentException("Your form expired. Reload this page before trying again.");
                    string payload = Request.Form["adminPayload"] ?? "{}";
                    if (payload.Length > 16000) throw new ArgumentException("The submitted form is too large.");
                    notice = await SaveDataAsync(Request.Form["adminAction"], JObject.Parse(payload), session);
                    Session["CoordinatorNotice"] = notice;
                    // Redirect after saving so refresh never repeats a write or account registration.
                    Response.Redirect(Request.RawUrl, false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
            }
            catch (Exception ex) { error = FriendlyError(ex); }
            try { data = await LoadDataAsync(session); }
            catch (Exception ex) { error = FriendlyError(ex); }
            BackendJson = JsonConvert.SerializeObject(new { data, error, notice }, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });
        }
        private static string FriendlyError(Exception ex)
        {
            System.Diagnostics.Trace.TraceError("Coordinator request failed: " + ex.GetType().Name);
            if (ex is ArgumentException || ex is AuthException || ex is UnauthorizedAccessException) return ex.Message;
            if (ex.Message.StartsWith("Firebase HTTP error 401") || ex.Message.StartsWith("Firebase HTTP error 403"))
                return "Firebase denied access. Check the signed-in coordinator profile and publish the coordinator database rules.";
            if (ex.Message.StartsWith("Registration")) return ex.Message;
            return "The request could not be completed. Check Firebase configuration and connectivity, then try again.";
        }
    }

    // Ordinary reads and per-record writes still use the original FirebaseHelper.
    public class CoordinatorDatabase : FirebaseHelper
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private readonly HttpClient client;
        private readonly string url;
        public CoordinatorDatabase() : this(Client, ConfigurationManager.AppSettings["FirebaseDatabaseUrl"]) { }
        public CoordinatorDatabase(HttpClient client, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute)
                || new Uri(url).Scheme != Uri.UriSchemeHttps)
                throw new ConfigurationErrorsException("Set FirebaseDatabaseUrl to a valid HTTPS URL in Web.config.");
            this.client = client; this.url = url.TrimEnd('/');
        }
        public new Task UpdateAsync(string path, object changes, string idToken)
        {
            if (path != "/") return base.UpdateAsync(path, changes, idToken);
            if (string.IsNullOrWhiteSpace(idToken)) throw new UnauthorizedAccessException("Sign in first.");
            if (changes == null) throw new ArgumentNullException("changes");
            return SendAsync(new HttpMethod("PATCH"), url + "/.json?auth=" + Uri.EscapeDataString(idToken), changes);
        }
        internal Task SaveContactAsync(ContactMessage message)
        {
            if (message == null || !System.Text.RegularExpressions.Regex.IsMatch(message.Id ?? "", "^[a-f0-9]{32}$"))
                throw new ArgumentException("Invalid contact message ID.");
            return SendAsync(HttpMethod.Put, url + "/contactMessages/" + message.Id + ".json", new {
                message.Id, message.FullName, message.Email, message.Subject, message.Message,
                message.CreatedAtUnixMs, isRead = false, isArchived = false
            });
        }
        private async Task SendAsync(HttpMethod method, string address, object value)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            using (var request = new HttpRequestMessage(method, address))
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(value, settings), Encoding.UTF8, "application/json");
                using (var response = await client.SendAsync(request).ConfigureAwait(false))
                    if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Firebase HTTP error " + (int)response.StatusCode);
            }
        }
    }


    public class CoordinatorAccounts
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private readonly HttpClient client;
        private readonly string apiKey;
        public CoordinatorAccounts() : this(Client, ConfigurationManager.AppSettings["FirebaseWebApiKey"]) { }
        public CoordinatorAccounts(HttpClient client, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey)) throw new ConfigurationErrorsException("Configure FirebaseWebApiKey on the server.");
            this.client = client; this.apiKey = apiKey;
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
        public async Task<AuthSession> CreateUserAsync(string email, string password)
        {
            try
            {
                var json = await PostAsync("signUp", new { email, password, returnSecureToken = true }).ConfigureAwait(false);
                return ReadSession(json, email);
            }
            catch (AuthException ex)
            {
                if (ex.Code == "EMAIL_EXISTS") throw new AuthException("This email already exists in Firebase Authentication. Use a different email or complete its existing database profile.", ex.Code);
                throw;
            }
        }
        public async Task DeleteNewAccountAsync(AuthSession newAccount)
        {
            await PostAsync("delete", new { idToken = newAccount.IdToken }).ConfigureAwait(false);
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
    }
}
