using System;
using System.Web.UI;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            RegisterAsyncTask(new PageAsyncTask(LoadStudentIdentityAsync));
        }

        private async System.Threading.Tasks.Task LoadStudentIdentityAsync()
        {
            var auth = Session[AuthGuard.SessionKey] as AuthSession;
            if (auth == null) return;

            try
            {
                var user = await new StudentService().GetUserAsync(auth.Uid, auth.IdToken);
                if (user == null || string.IsNullOrWhiteSpace(user.FullName)) return;

                StudentName.Text = Server.HtmlEncode(user.FullName.Trim());
                StudentInitials.Text = Server.HtmlEncode(GetInitials(user.FullName));
            }
            catch (Exception)
            {
                // Keep the safe generic account label if the profile cannot be loaded.
            }
        }

        private static string GetInitials(string fullName)
        {
            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "ST";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }
    }
}
