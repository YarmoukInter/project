using System;
using System.Web.UI;
using System.Web;
using Yarmouk.Helpers;
using Yarmouk.Services;
namespace Yarmouk.StudentPages
{
    public partial class SectionMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            var email = auth == null ? string.Empty : auth.User != null && !string.IsNullOrWhiteSpace(auth.User.Email) ? auth.User.Email : auth.Email;
            var name = auth == null || auth.User == null ? null : auth.User.FullName;
            if (string.IsNullOrWhiteSpace(name))
                name = string.IsNullOrWhiteSpace(email) ? "Student" : email.Split('@')[0].Replace('.', ' ').Replace('_', ' ').Replace('-', ' ');
            name = name.Trim();
            StudentTopName.Text = HttpUtility.HtmlEncode(name);
            StudentTopInitials.Text = HttpUtility.HtmlEncode(Initials(name));
            StudentSidebarName.Text = HttpUtility.HtmlEncode(name);
            StudentSidebarInitials.Text = HttpUtility.HtmlEncode(Initials(name));
            StudentSidebarEmail.Text = HttpUtility.HtmlEncode(email);
        }

        private static string Initials(string name)
        {
            var parts = (name ?? "Student").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "ST";
            return parts.Length > 1 ? (parts[0][0].ToString() + parts[parts.Length - 1][0]).ToUpperInvariant() : parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
        }

        protected void SignOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect(ResolveUrl("~/PublicPages/Login"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
