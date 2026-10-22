using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.CompanyPages
{
    public partial class Account : SupervisorPageBase
    {
        private AuthSession auth;
        private readonly CompanySupervisorService service = new CompanySupervisorService();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                auth = GetSupervisorSession("company");
                if (!IsPostBack) RegisterAsyncTask(new PageAsyncTask(LoadAccountAsync));
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Your company supervisor session could not be verified. Sign in again.");
                ProfileForm.Visible = false;
            }
        }

        private async Task LoadAccountAsync()
        {
            try
            {
                var profile = await service.GetSupervisorAccountAsync(auth.Uid, auth.IdToken);
                BindProfile(profile);
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Your company supervisor profile could not be verified in Firebase.");
                ProfileForm.Visible = false;
            }
            catch (InvalidOperationException error)
            {
                System.Diagnostics.Trace.TraceWarning("Company account Firebase response: " + error.Message);
                ShowError(FirebaseReadMessage(error));
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company account load failed: " + error.GetType().Name + ": " + error.Message);
                ShowError("Account data could not be loaded. Check that the app is connected to the right Firebase project and that this supervisor has a users/{uid} profile.");
            }
        }

        protected void SaveAccount_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(SaveAccountAsync));
        }

        private async Task SaveAccountAsync()
        {
            try
            {
                auth = auth ?? GetSupervisorSession("company");
                await service.SaveProfileAsync(auth.Uid, auth.IdToken, FullNameBox.Text, PhoneBox.Text);
                var profile = await service.GetSupervisorAccountAsync(auth.Uid, auth.IdToken);
                BindProfile(profile);
                AccountNoticeText.Text = "Your profile was saved to Firebase and reloaded.";
                AccountNotice.Visible = true;
                AccountError.Visible = false;
            }
            catch (ArgumentException error)
            {
                ShowError(error.Message);
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("Your company supervisor session could not be verified. Sign in again.");
            }
            catch (InvalidOperationException error)
            {
                System.Diagnostics.Trace.TraceWarning("Company account Firebase save response: " + error.Message);
                ShowError(FirebaseWriteMessage(error));
            }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company account save failed: " + error.GetType().Name + ": " + error.Message);
                ShowError("Your profile could not be saved to Firebase. Check the Firebase rules and try again.");
            }
        }

        private void BindProfile(CompanyProfileData profile)
        {
            FullNameBox.Text = string.IsNullOrWhiteSpace(profile.FullName) && auth.User != null
                ? auth.User.FullName : profile.FullName;
            PhoneBox.Text = profile.Phone;
            EmailValue.Text = Display(string.IsNullOrWhiteSpace(profile.Email) ? auth.Email : profile.Email);
        }

        private static string Display(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<span class=\"not-recorded\">Not recorded in Firebase</span>" : HttpUtility.HtmlEncode(value);
        }

        private void ShowError(string message)
        {
            AccountErrorText.Text = HttpUtility.HtmlEncode(message);
            AccountError.Visible = true;
        }

        private static string FirebaseReadMessage(Exception error)
        {
            if (error.Message.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0
                || error.Message.IndexOf("403", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Firebase refused the profile read (HTTP 401/403). Merge and publish the Company Supervisor read permissions in Firebase Realtime Database Rules, then sign out and sign in again.";
            if (error.Message.IndexOf("404", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Firebase returned HTTP 404. Check the FirebaseDatabaseUrl setting and confirm that this project uses Realtime Database.";
            return "Firebase could not return the supervisor profile (" + error.Message + "). Check the database URL, profile record, and read rules.";
        }

        private static string FirebaseWriteMessage(Exception error)
        {
            if (error.Message.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0
                || error.Message.IndexOf("403", StringComparison.OrdinalIgnoreCase) >= 0
                || error.Message.IndexOf("Permission denied", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Firebase denied the save. Publish the Company Supervisor write rules for this linked account, then sign out and back in.";
            return "Firebase rejected the save (" + error.Message + "). Check the field rules and database record.";
        }
    }
}
