using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Request.QueryString["downloadCv"]))
            {
                RegisterAsyncTask(new PageAsyncTask(DownloadCvAsync));
                return;
            }
            if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadPageAsync));
        }

        protected void ApplyFiltersButton_Click(object sender, EventArgs e)
        {
            SelectedApplicationId.Value = string.Empty;
            RegisterAsyncTask(new PageAsyncTask(LoadPageAsync));
        }

        protected void AcceptApplicationButton_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(() => SaveDecisionAsync("offered", string.Empty)));
        }

        protected void ConfirmRejectionButton_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(() => SaveDecisionAsync("rejected", RejectionReasonInput.Text)));
        }

        private async Task LoadPageAsync()
        {
            try
            {
                var auth = await VerifyCompanyAsync();
                var applications = await new CompanyWorkflowService().GetApplicationsAsync(auth.CompanyId, auth.Session.IdToken);
                BindPage(applications);
            }
            catch (UnauthorizedAccessException) { RedirectToLogin(); }
            catch (AuthException) { RedirectToLogin(); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company applications could not load: " + error.GetType().Name);
                BindPage(new List<CompanyApplicationItem>());
                PageFeedbackLabel.Text = "Could not load applications. Please try again.";
            }
        }

        private void BindPage(List<CompanyApplicationItem> applications)
        {
            applications = applications ?? new List<CompanyApplicationItem>();
            NewCountLiteral.Text = applications.Count(x => Same(x.Status, "new")).ToString(CultureInfo.InvariantCulture);
            ReviewCountLiteral.Text = applications.Count(x => Same(x.Status, "in-review") || Same(x.Status, "offered")).ToString(CultureInfo.InvariantCulture);
            AcceptedCountLiteral.Text = applications.Count(x => Same(x.Status, "accepted")).ToString(CultureInfo.InvariantCulture);
            RejectedCountLiteral.Text = applications.Count(x => Same(x.Status, "rejected")).ToString(CultureInfo.InvariantCulture);
            PopulateOpportunityFilter(applications);

            string query = (ApplicationSearchTextBox.Text ?? string.Empty).Trim();
            string opportunityId = ApplicationOpportunityFilter.SelectedValue ?? "all";
            string status = ApplicationStatusFilter.SelectedValue ?? "all";
            var filtered = applications.Where(item =>
                (string.IsNullOrEmpty(query) || JoinSearchText(item).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                && (opportunityId == "all" || Same(item.OpportunityId, opportunityId))
                && (status == "all" || Same(item.Status, status))).ToList();

            if (!filtered.Any(x => Same(x.Id, SelectedApplicationId.Value)))
                SelectedApplicationId.Value = string.Empty;
            ApplicationsRepeater.DataSource = filtered;
            ApplicationsRepeater.DataBind();
            ApplicationResultsCountLabel.Text = filtered.Count == 1 ? "1 application" : filtered.Count + " applications";
            EmptyStatePanel.Visible = filtered.Count == 0;
            ApplicationsDataLiteral.Text = SerializeApplications(filtered);
        }

        private void PopulateOpportunityFilter(IEnumerable<CompanyApplicationItem> applications)
        {
            string selected = ApplicationOpportunityFilter.SelectedValue;
            ApplicationOpportunityFilter.Items.Clear();
            ApplicationOpportunityFilter.Items.Add(new ListItem("All opportunities", "all"));
            foreach (var item in applications.GroupBy(x => x.OpportunityId).Select(group => group.First()).OrderBy(x => x.Opportunity))
                ApplicationOpportunityFilter.Items.Add(new ListItem(item.Opportunity, item.OpportunityId));
            ApplicationOpportunityFilter.SelectedValue = ApplicationOpportunityFilter.Items.FindByValue(selected) == null ? "all" : selected;
        }

        private string SerializeApplications(IEnumerable<CompanyApplicationItem> applications)
        {
            var payload = applications.Select(item => new
            {
                item.Id,
                item.Name,
                item.Number,
                item.Major,
                item.Email,
                applied = FormatDate(item.AppliedAtUnixMs),
                item.Opportunity,
                item.Status,
                statusLabel = StatusLabel(item.Status),
                cvUrl = item.HasUploadedCv
                    ? ResolveUrl("~/CompanyPages/ApplicationsReview?downloadCv=" + HttpUtility.UrlEncode(item.StudentUid) + "&application=" + HttpUtility.UrlEncode(item.Id))
                    : IsSafeWebUrl(item.CvUrl) ? item.CvUrl : string.Empty,
                questions = item.Questions ?? new List<string>(),
                answers = item.Answers ?? new List<string>(),
                note = Same(item.Status, "rejected") ? item.RejectionReason : item.DecisionNote,
                item.CanAccept,
                item.AcceptanceBlockedReason
            });
            return JsonConvert.SerializeObject(payload, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }).Replace("<", "\\u003c").Replace(">", "\\u003e").Replace("&", "\\u0026");
        }

        private async Task SaveDecisionAsync(string status, string reason)
        {
            if (string.IsNullOrWhiteSpace(SelectedApplicationId.Value))
            {
                PageFeedbackLabel.Text = "Select an application first.";
                return;
            }
            if (Same(status, "rejected") && string.IsNullOrWhiteSpace(reason))
            {
                RejectionFeedbackLabel.Text = "Enter a reason for rejection.";
                RejectionFeedbackLabel.Visible = true;
                return;
            }
            try
            {
                var auth = await VerifyCompanyAsync();
                await new CompanyWorkflowService().SetApplicationStatusAsync(auth.CompanyId, auth.Session.Uid,
                    SelectedApplicationId.Value, status, reason == null ? string.Empty : reason.Trim(), auth.Session.IdToken);
                await LoadPageAsync();
                PageFeedbackLabel.Text = Same(status, "offered")
                    ? "Offer saved. The student can now confirm it."
                    : "Rejection saved in Firebase.";
            }
            catch (UnauthorizedAccessException) { RedirectToLogin(); }
            catch (AuthException) { RedirectToLogin(); }
            catch (ArgumentException error) { await LoadPageAsync(); ShowDecisionError(error.Message); }
            catch (InvalidOperationException error) { await LoadPageAsync(); ShowDecisionError(error.Message); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company application decision failed: " + error.GetType().Name);
                ShowDecisionError("Firebase could not save this decision. Please try again.");
            }
        }

        private async Task DownloadCvAsync()
        {
            try
            {
                string studentUid = Request.QueryString["downloadCv"];
                string applicationId = Request.QueryString["application"];
                var auth = await VerifyCompanyAsync();
                var applications = await new CompanyWorkflowService().GetApplicationsAsync(auth.CompanyId, auth.Session.IdToken);
                var application = applications.FirstOrDefault(item => Same(item.Id, applicationId) && Same(item.StudentUid, studentUid));
                if (application == null) throw new UnauthorizedAccessException();
                var document = await new FirebaseHelper().GetAsync<CvDocument>(DatabasePaths.StudentCvFiles + "/" + studentUid, auth.Session.IdToken);
                if (document == null || string.IsNullOrWhiteSpace(document.DataBase64)) { WriteNotFound(); return; }
                byte[] bytes = Convert.FromBase64String(document.DataBase64);
                string fileName = Path.GetFileName(string.IsNullOrWhiteSpace(document.FileName) ? "student-cv.pdf" : document.FileName).Replace("\"", string.Empty);
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("Content-Disposition", "inline; filename=\"" + fileName + "\"");
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.SuppressContent = true;
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (UnauthorizedAccessException) { Response.StatusCode = 403; Response.SuppressContent = true; }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company CV download failed: " + error.GetType().Name);
                WriteNotFound();
            }
        }

        private async Task<VerifiedCompany> VerifyCompanyAsync()
        {
            var session = Session == null ? null : Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null || string.IsNullOrWhiteSpace(session.Uid) || string.IsNullOrWhiteSpace(session.IdToken)) throw new UnauthorizedAccessException();
            var user = await new StudentService().GetUserAsync(session.Uid, session.IdToken);
            if (user == null || !user.IsActive || !Same(AuthPolicy.NormalizeRole(user.Role), "company")) throw new UnauthorizedAccessException();
            session.User = user;
            var firebase = new FirebaseHelper();
            var userRecord = await firebase.GetAsync<JObject>(DatabasePaths.Users + "/" + session.Uid, session.IdToken);
            string companyId = userRecord == null ? null : (string)userRecord["companyId"];
            if (string.IsNullOrWhiteSpace(companyId))
            {
                var companies = await firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.Companies, session.IdToken);
                if (companies != null)
                {
                    var match = companies.FirstOrDefault(entry => entry.Value != null
                        && ((entry.Value["supervisorUids"] is JObject supervisors && supervisors[session.Uid] != null && supervisors[session.Uid].Value<bool>())
                            || Same((string)entry.Value["supervisorUid"], session.Uid)));
                    if (!string.IsNullOrWhiteSpace(match.Key)) companyId = match.Key;
                }
            }
            if (string.IsNullOrWhiteSpace(companyId)) throw new UnauthorizedAccessException();
            var company = await firebase.GetAsync<Company>(DatabasePaths.Companies + "/" + companyId, session.IdToken);
            if (company == null || !company.IsApproved) throw new UnauthorizedAccessException();
            return new VerifiedCompany { Session = session, CompanyId = companyId };
        }

        private void ShowDecisionError(string message)
        {
            PageFeedbackLabel.Text = "Could not save this decision: " + Encode(message);
        }

        private void WriteNotFound() { Response.StatusCode = 404; Response.SuppressContent = true; Context.ApplicationInstance.CompleteRequest(); }
        private void RedirectToLogin() { Response.Redirect(ResolveUrl("~/PublicPages/Login"), false); Context.ApplicationInstance.CompleteRequest(); }
        private static bool Same(string left, string right) { return string.Equals(left, right, StringComparison.OrdinalIgnoreCase); }
        private static string JoinSearchText(CompanyApplicationItem item) { return string.Join(" ", item.Name, item.Number, item.Major, item.Opportunity); }
        private static bool IsSafeWebUrl(string value) { Uri uri; return Uri.TryCreate(value, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps); }
        protected string Encode(object value) { return HttpUtility.HtmlEncode(Convert.ToString(value, CultureInfo.CurrentCulture)); }
        protected string Attribute(object value) { return HttpUtility.HtmlAttributeEncode(Convert.ToString(value, CultureInfo.InvariantCulture)); }
        protected string Initials(object value)
        {
            string name = Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
            var parts = name.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? "ST" : string.Concat(parts.Take(2).Select(part => char.ToUpperInvariant(part[0])));
        }
        protected string CandidateRowCss(object id) { return "co-candidate-row" + (Same(Convert.ToString(id, CultureInfo.InvariantCulture), SelectedApplicationId.Value) ? " selected" : string.Empty); }
        protected string StatusCss(object status) { return "co-status co-status-" + HttpUtility.HtmlAttributeEncode(Convert.ToString(status, CultureInfo.InvariantCulture).ToLowerInvariant()); }
        protected string StatusLabel(object status) { string value = Convert.ToString(status, CultureInfo.InvariantCulture); return Same(value, "in-review") ? "In review" : string.IsNullOrEmpty(value) ? "Unknown" : HttpUtility.HtmlEncode(char.ToUpperInvariant(value[0]) + value.Substring(1)); }
        protected string FormatDate(object value) { long unixMs; return long.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out unixMs) && unixMs > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(unixMs).ToString("dd MMM yyyy", CultureInfo.InvariantCulture) : "—"; }

        private sealed class VerifiedCompany { public AuthSession Session { get; set; } public string CompanyId { get; set; } }
        private sealed class CvDocument { public string FileName { get; set; } public string DataBase64 { get; set; } public long UploadedAtUnixMs { get; set; } }
    }
}
