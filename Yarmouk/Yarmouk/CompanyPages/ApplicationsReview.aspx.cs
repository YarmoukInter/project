using System;
using System.IO;
using System.Linq;
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

namespace Yarmouk.CompanyPages
{
    public partial class ApplicationsReview : Page
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsApiRequest) RegisterAsyncTask(new PageAsyncTask(ProcessApiRequestAsync));
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (!IsApiRequest) base.Render(writer);
        }

        private bool IsApiRequest
        {
            get { return !string.IsNullOrWhiteSpace(Request.QueryString["action"]); }
        }

        private async Task ProcessApiRequestAsync()
        {
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            try
            {
                var auth = await VerifyCompanyAsync();
                var workflow = new CompanyWorkflowService();
                string action = Request.QueryString["action"] ?? string.Empty;
                if (string.Equals(Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
                {
                    if (action != "applications") { WriteJson(400, new { success = false, message = "Unknown request." }); return; }
                    var applications = await workflow.GetApplicationsAsync(auth.CompanyId, auth.Session.IdToken);
                    WriteJson(200, new { success = true, applications = applications });
                    return;
                }
                if (!string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    Response.AppendHeader("Allow", "GET, POST");
                    WriteJson(405, new { success = false, message = "Method not allowed." });
                    return;
                }
                if (!string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal))
                { WriteJson(400, new { success = false, message = "Invalid request." }); return; }
                if (Request.ContentLength > 65536)
                { WriteJson(413, new { success = false, message = "Request is too large." }); return; }

                JObject body;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8)) body = JObject.Parse(reader.ReadToEnd());
                action = (string)body["action"] ?? action;
                if (action != "setApplicationStatus") { WriteJson(400, new { success = false, message = "Unknown request." }); return; }
                await workflow.SetApplicationStatusAsync(auth.CompanyId, auth.Session.Uid, (string)body["id"],
                    (string)body["status"], (string)body["reason"], auth.Session.IdToken);
                WriteJson(200, new { success = true });
            }
            catch (UnauthorizedAccessException)
            { WriteJson(401, new { success = false, message = "Your company session is no longer valid. Sign in again." }); }
            catch (AuthException)
            { WriteJson(401, new { success = false, message = "Your company session is no longer valid. Sign in again." }); }
            catch (ArgumentException error)
            { WriteJson(400, new { success = false, message = error.Message }); }
            catch (JsonException)
            { WriteJson(400, new { success = false, message = "Invalid request data." }); }
            catch (InvalidOperationException error)
            { WriteJson(409, new { success = false, message = error.Message }); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company application request failed: " + error.GetType().Name);
                WriteJson(500, new { success = false, message = "Firebase could not complete this request. Check the database rules and try again." });
            }
        }

        private async Task<VerifiedCompany> VerifyCompanyAsync()
        {
            var session = Session == null ? null : Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null || string.IsNullOrWhiteSpace(session.Uid) || string.IsNullOrWhiteSpace(session.IdToken))
                throw new UnauthorizedAccessException();
            // AuthGuard has already checked this session and its role for this
            // protected page. The extra lookup here expired company actions
            // after 30 minutes even while the page still loaded normally.
            var user = await new StudentService().GetUserAsync(session.Uid, session.IdToken);
            if (user == null || !user.IsActive || !string.Equals(AuthPolicy.NormalizeRole(user.Role), "company", StringComparison.Ordinal))
                throw new UnauthorizedAccessException();
            session.User = user;

            var firebase = new FirebaseHelper();
            var userRecord = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + session.Uid, session.IdToken);
            string companyId = userRecord == null ? null : (string)userRecord["companyId"];
            if (string.IsNullOrWhiteSpace(companyId))
            {
                var companies = await firebase.GetAsync<System.Collections.Generic.Dictionary<string, JObject>>(DatabasePaths.Companies, session.IdToken);
                if (companies != null)
                {
                    var match = companies.FirstOrDefault(entry => entry.Value != null
                        && ((entry.Value["supervisorUids"] is JObject supervisors
                                && supervisors[session.Uid] != null && supervisors[session.Uid].Value<bool>())
                            || string.Equals((string)entry.Value["supervisorUid"], session.Uid, StringComparison.Ordinal)));
                    if (!string.IsNullOrWhiteSpace(match.Key)) companyId = match.Key;
                }
            }
            if (string.IsNullOrWhiteSpace(companyId)) throw new UnauthorizedAccessException();
            var company = await firebase.GetAsync<Company>(DatabasePaths.Companies + "/" + companyId, session.IdToken);
            if (company == null || !company.IsApproved) throw new UnauthorizedAccessException();
            return new VerifiedCompany { Session = session, CompanyId = companyId };
        }

        private void WriteJson(int statusCode, object value)
        {
            Response.StatusCode = statusCode;
            Response.TrySkipIisCustomErrors = true;
            Response.Write(JsonConvert.SerializeObject(value, JsonSettings));
        }

        private sealed class VerifiedCompany
        {
            public AuthSession Session { get; set; }
            public string CompanyId { get; set; }
        }
    }
}
