using System;
using System.Web;
using System.Threading.Tasks;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;
namespace Yarmouk.PublicPages
{
    public partial class Login : AuthPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Request.QueryString["expired"] == "1")
                Message.Text = "Please sign in again. Your session expired or your account could not be verified.";
        }
        protected void SignIn_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string email = loginEmail.Text.Trim();
                if (!AuthPolicy.ValidEmail(email) || string.IsNullOrEmpty(loginPassword.Text) || loginPassword.Text.Length > 128)
                { Message.Text = "Enter a valid email address and password."; return; }
                try
                {
                    var auth = await new FirebaseAuthService().SignInAsync(email, loginPassword.Text);
                    await CompleteSignInAsync(auth);
                }
                catch (AuthException error) { Message.Text = HttpUtility.HtmlEncode(error.Message); }
                catch (Exception error)
                {
                    System.Diagnostics.Trace.TraceWarning("Sign-in failed: " + error.GetType().Name);
                    Message.Text = "Sign-in is temporarily unavailable. Please contact the internship coordinator.";
                }
                finally { loginPassword.Text = ""; }
            }));
        }
        protected void SocialSignIn_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string token = SocialIdToken.Value;
                SocialIdToken.Value = "";
                try
                {
                    var auth = await new FirebaseAuthService().SignInWithIdTokenAsync(token);
                    await CompleteSignInAsync(auth);
                }
                catch (AuthException error) { Message.Text = HttpUtility.HtmlEncode(error.Message); }
                catch (Exception error)
                {
                    System.Diagnostics.Trace.TraceWarning("Social sign-in failed: " + error.GetType().Name);
                    Message.Text = "Social sign-in is temporarily unavailable. Please contact the internship coordinator.";
                }
            }));
        }
        private async Task CompleteSignInAsync(AuthSession auth)
        {
            var user = await new StudentService().GetUserAsync(auth.Uid, auth.IdToken);
            if (user == null)
                throw new AuthException("This account has no user profile in the internship portal. Contact the coordinator.");
            if (!AuthPolicy.ValidUser(user, auth.Uid))
                throw new AuthException("This account is inactive or has an unsupported role. Contact the coordinator.");
            user.Role = AuthPolicy.NormalizeRole(user.Role);
            auth.User = user;
            Session.Clear();
            Session[AuthGuard.SessionKey] = auth;
            Go(user.Role == "student" && user.MustChangePassword ? "~/StudentPages/ChangePassword.aspx" : AuthPolicy.Home(user.Role));
        }
        protected void Recover_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string email = resetEmail.Text.Trim();
                if (!AuthPolicy.ValidEmail(email)) { RecoveryMessage.Text = "Enter a valid email address."; return; }
                DateTime? last = Session["RecoveryRequestedUtc"] as DateTime?;
                if (last.HasValue && DateTime.UtcNow - last.Value < TimeSpan.FromSeconds(60))
                { RecoveryMessage.Text = "Please wait a minute before requesting another email."; return; }
                Session["RecoveryRequestedUtc"] = DateTime.UtcNow;
                try
                {
                    await new FirebaseAuthService().SendRecoveryAsync(email);
                    RecoveryMessage.Text = "Firebase accepted the reset request. If this email is registered, check your inbox and spam folder for the reset link.";
                }
                catch (AuthException error)
                {
                    RecoveryMessage.Text = HttpUtility.HtmlEncode(error.Message);
                }
                catch (Exception error)
                {
                    System.Diagnostics.Trace.TraceWarning("Recovery failed: " + error.GetType().Name);
                    RecoveryMessage.Text = "The reset request could not reach Firebase. Please try again later.";
                }
            }));
        }
    }
}
