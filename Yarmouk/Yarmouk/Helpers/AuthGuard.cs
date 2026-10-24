using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Yarmouk.Services;

namespace Yarmouk.Helpers
{
    public static class AuthGuard
    {
        public const string SessionKey = "AuthenticatedUser";
        public static async Task CheckAsync(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var context = app.Context;
            if (!(context.CurrentHandler is Page)) return;
            string path = context.Request.AppRelativeCurrentExecutionFilePath;
            string requiredRole = AuthPolicy.RequiredRole(path);
            if (requiredRole == null) return;
            var session = context.Session[SessionKey] as AuthSession;
            try
            {
                if (session == null || session.ExpiresUtc <= DateTime.UtcNow)
                {
                    context.Session.Remove(SessionKey);
                    Redirect(context, "~/PublicPages/Login?expired=1"); return;
                }
                var user = await new StudentService().GetUserAsync(session.Uid, session.IdToken);
                if (!AuthPolicy.ValidUser(user, session.Uid)) throw new AuthException("Account unavailable.");
                user.Role = AuthPolicy.NormalizeRole(user.Role);
                session.User = user;
                string redirect = AuthPolicy.ProtectedPageRedirect(user, path);
                if (redirect != null) { Redirect(context, redirect); return; }
                context.User = new System.Security.Principal.GenericPrincipal(new System.Security.Principal.GenericIdentity(session.Uid, "Firebase"), new[] { user.Role });
            }
            catch (Exception)
            {
                context.Session.Remove(SessionKey);
                Redirect(context, "~/PublicPages/Login?expired=1");
            }
        }
        private static void Redirect(HttpContext context, string path)
        {
            context.Response.Redirect(VirtualPathUtility.ToAbsolute(path), false);
            // Skip page execution even though CompleteRequest alone does not abort a page lifecycle.
            context.Handler = new EmptyHandler();
            context.ApplicationInstance.CompleteRequest();
        }
        private sealed class EmptyHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
        {
            public bool IsReusable { get { return false; } }
            public void ProcessRequest(HttpContext context) { }
        }
    }
}
