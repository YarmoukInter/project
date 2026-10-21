using System;
using System.Web;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;
namespace Yarmouk.StudentPages
{
    public partial class ChangePassword : AuthPage
    {
        private const string PendingSetupKey = "PasswordChangedSetupPending";

        protected void Change_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                var session = Session[AuthGuard.SessionKey] as AuthSession;
                if (session == null) { Go("~/PublicPages/Login.aspx"); return; }
                bool passwordAlreadyChanged = Session[PendingSetupKey] as bool? == true;
                try
                {
                    if (!passwordAlreadyChanged)
                    {
                        string error = AuthPolicy.PasswordError(CurrentPassword.Text, NewPassword.Text, ConfirmPassword.Text);
                        if (error != null) { Message.Text = error; return; }

                        var updated = await new FirebaseAuthService().ChangePasswordAsync(session, CurrentPassword.Text, NewPassword.Text, ConfirmPassword.Text);
                        updated.User = session.User;
                        Session[AuthGuard.SessionKey] = updated;
                        session = updated;
                    }

                    // Clear the flag only after Firebase confirms the password change.
                    await new PasswordSetupService().CompleteAsync(session.Uid, session.IdToken);
                    Session.Remove(PendingSetupKey);
                    session.User.MustChangePassword = false;
                    Go(AuthPolicy.Home(session.User.Role));
                }
                catch (AuthException ex) { Message.Text = HttpUtility.HtmlEncode(ex.Message); }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("Password setup failed: " + ex.GetType().Name);
                    if (!passwordAlreadyChanged && session.IdToken != null && session.ExpiresUtc > DateTime.UtcNow)
                    {
                        // Keep the refreshed token so setup can be retried after the database rule is fixed.
                        Session[PendingSetupKey] = true;
                        Message.Text = "Your password changed, but Firebase could not finish account setup. Ask the coordinator to apply the users rule in firebase/auth-profile.rules.json, then click Continue to workspace again.";
                    }
                    else
                    {
                        Message.Text = "Account setup is still unavailable. Ask the coordinator to apply the users rule in firebase/auth-profile.rules.json, then retry.";
                    }
                }
                finally { CurrentPassword.Text = NewPassword.Text = ConfirmPassword.Text = ""; }
            }));
        }
    }
}
