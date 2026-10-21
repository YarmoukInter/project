using System;
using System.Web;
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
                string requestedRole = AuthPolicy.NormalizeRole(SelectedRole.Value);
                if (AuthPolicy.Home(requestedRole) == null)
                { Message.Text = "Choose a valid account type."; return; }
                try
                {
                    var auth = await new FirebaseAuthService().SignInAsync(email, loginPassword.Text);
                    var user = await new StudentService().GetUserAsync(auth.Uid, auth.IdToken);
                    if (user == null)
                        throw new AuthException("Firebase accepted your sign-in, but this account has no profile at users/{Authentication UID}. Copy this account's UID from Firebase Authentication and use it as both the users record key and the profile uid.");
                    if (!AuthPolicy.ValidUser(user, auth.Uid))
                        throw new AuthException("The Firebase profile is inactive or has an unsupported role. Ask the coordinator to check isActive and role.");
                    user.Role = AuthPolicy.NormalizeRole(user.Role);
                    if (!string.Equals(user.Role, requestedRole, StringComparison.Ordinal))
                        throw new AuthException("This account is registered for a different role. Select the role assigned to your account and try again.");
                    auth.User = user;
                    Session.Clear();
                    Session[AuthGuard.SessionKey] = auth;
                    Go(user.Role == "student" && user.MustChangePassword ? "~/StudentPages/ChangePassword.aspx" : AuthPolicy.Home(user.Role));
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
