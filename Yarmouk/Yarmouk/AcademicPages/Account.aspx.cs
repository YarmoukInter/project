using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.AcademicPages
{
    public partial class Account : SupervisorPageBase
    {
        private static readonly Regex PhonePattern = new Regex(@"^[+0-9(). -]{1,30}$", RegexOptions.Compiled);
        private static readonly HttpClient FirebaseClient = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindProfile(GetSupervisorSession("academic").User);
        }

        protected void SaveProfile_Click(object sender, EventArgs e)
        {
            string fullName = (FullNameInput.Text ?? string.Empty).Trim();
            string phone = (PhoneInput.Text ?? string.Empty).Trim();

            if (fullName.Length == 0 || fullName.Length > 100)
            {
                ShowMessage("Enter a name of 1 to 100 characters.", true);
                return;
            }
            if (phone.Length > 0 && (phone.Length > 30 || !PhonePattern.IsMatch(phone) || !Regex.IsMatch(phone, @"\d")))
            {
                ShowMessage("Enter a valid phone number, or leave the field empty.", true);
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(() => SaveProfileAsync(fullName, phone)));
        }

        private async Task SaveProfileAsync(string fullName, string phone)
        {
            try
            {
                AuthSession session = GetSupervisorSession("academic");
                await SaveProfileFieldAsync(session, "fullName", fullName);
                await SaveProfileFieldAsync(session, "phone", phone);

                session.User.FullName = fullName;
                session.User.Phone = phone;
                BindProfile(session.User);
                ShowMessage("Your profile was saved.", false);
            }
            catch (UnauthorizedAccessException)
            {
                ShowMessage("Your session has expired. Sign in again and retry.", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Academic supervisor profile update failed: " + ex.GetType().Name);
                ShowMessage("We couldn't save your profile. Check the Firebase connection and account permissions, then try again.", true);
            }
        }

        private static async Task SaveProfileFieldAsync(AuthSession session, string field, string value)
        {
            if (field != "fullName" && field != "phone")
                throw new ArgumentException("Only profile fields can be updated.", "field");

            string databaseUrl = Yarmouk.Helpers.SiteSettingsStore.GetFirebaseDatabaseUrl();
            if (string.IsNullOrWhiteSpace(databaseUrl) || !Uri.IsWellFormedUriString(databaseUrl, UriKind.Absolute)
                || new Uri(databaseUrl).Scheme != Uri.UriSchemeHttps)
                throw new ConfigurationErrorsException("Set FirebaseDatabaseUrl to a valid HTTPS URL in Web.config.");

            string url = databaseUrl.TrimEnd('/') + "/users/" + Uri.EscapeDataString(session.Uid)
                + "/" + field + ".json?auth=" + Uri.EscapeDataString(session.IdToken);
            string json = JsonConvert.SerializeObject(value);

            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            using (HttpResponseMessage response = await FirebaseClient.PutAsync(url, content))
            {
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException("Firebase HTTP error " + (int)response.StatusCode);
            }
        }

        private void BindProfile(User user)
        {
            string name = string.IsNullOrWhiteSpace(user.FullName) ? "Academic Supervisor" : user.FullName.Trim();
            string email = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : GetSupervisorSession("academic").Email;
            ProfileName.Text = HttpUtility.HtmlEncode(name);
            ProfileInitials.Text = HttpUtility.HtmlEncode(GetInitials(name));
            ProfileEmailSummary.Text = HttpUtility.HtmlEncode(email ?? string.Empty);
            FullNameInput.Text = name == "Academic Supervisor" && string.IsNullOrWhiteSpace(user.FullName) ? string.Empty : user.FullName;
            EmailInput.Text = email ?? string.Empty;
            PhoneInput.Text = user.Phone ?? string.Empty;
        }

        private static string GetInitials(string name)
        {
            string[] parts = (name ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "AS";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        private void ShowMessage(string message, bool isError)
        {
            ProfileSaveMessage.Text = HttpUtility.HtmlEncode(message);
            ProfileSaveMessage.CssClass = isError ? "account-message is-error" : "account-message is-success";
        }
    }
}
