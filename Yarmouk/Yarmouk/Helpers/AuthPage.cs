using System;
using System.Web;
using System.Web.UI;

namespace Yarmouk.Helpers
{
    // Shared redirect helper for the two authentication pages.
    public class AuthPage : Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }
        protected void Go(string path)
        {
            Response.Redirect(ResolveUrl(path), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
