using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class Account : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            var session = Session[AuthGuard.SessionKey] as AuthSession;
            var user = session == null ? null : session.User;
            string name = user == null ? string.Empty : user.FullName;
            string email = user == null ? string.Empty : user.Email;
            if (string.IsNullOrWhiteSpace(email) && session != null) email = session.Email;

            string role = user == null ? string.Empty : AuthPolicy.NormalizeRole(user.Role);
            string roleLabel = string.Equals(role, "coordinator", StringComparison.OrdinalIgnoreCase)
                ? "Coordinator" : string.IsNullOrWhiteSpace(role) ? "Not available" : role;
            string jobTitle = string.Equals(role, "coordinator", StringComparison.OrdinalIgnoreCase)
                ? "Field Training Coordinator" : roleLabel;
            string accountStatus = user != null && user.IsActive ? "Active" : "Unavailable";
            string accountId = session == null || string.IsNullOrWhiteSpace(session.Uid)
                ? "Not available" : "••••" + session.Uid.Substring(Math.Max(0, session.Uid.Length - 6));

            SetLiteral("profileSummaryName", name, "Coordinator account");
            SetLiteral("profileFullName", name, "Not available");
            SetLiteral("profileSummaryEmail", email, "Not available");
            SetLiteral("profileEmail", email, "Not available");
            SetLiteral("profileSummaryTitle", jobTitle, "Coordinator");
            SetLiteral("profileJobTitle", jobTitle, "Not available");
            SetLiteral("profileRole", roleLabel, "Not available");
            SetLiteral("profileStatus", accountStatus);
            SetLiteral("profileSummaryStatus", accountStatus);
            SetLiteral("profileAccountId", accountId);
        }

        private void SetLiteral(string id, string value, string fallback = "—")
        {
            var control = FindControlRecursive(this, id) as Literal;
            if (control != null)
                control.Text = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? fallback : value);
        }

        private static Control FindControlRecursive(Control parent, string id)
        {
            if (parent == null) return null;
            if (string.Equals(parent.ID, id, StringComparison.Ordinal)) return parent;
            foreach (Control child in parent.Controls)
            {
                var match = FindControlRecursive(child, id);
                if (match != null) return match;
            }
            return null;
        }
    }
}