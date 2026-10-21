using System;
using System.Web.UI;
using Yarmouk.Services;

namespace Yarmouk.Helpers
{
    public abstract class SupervisorPageBase : Page
    {
        protected AuthSession GetSupervisorSession(string expectedRole)
        {
            var session = Context == null || Context.Session == null
                ? null : Context.Session[AuthGuard.SessionKey] as AuthSession;
            if (session == null || session.User == null
                || !string.Equals(AuthPolicy.NormalizeRole(session.User.Role), expectedRole, StringComparison.Ordinal)
                || !session.User.IsActive || string.IsNullOrWhiteSpace(session.Uid)
                || string.IsNullOrWhiteSpace(session.IdToken))
                throw new UnauthorizedAccessException("A verified supervisor session is required.");
            return session;
        }
    }
}
