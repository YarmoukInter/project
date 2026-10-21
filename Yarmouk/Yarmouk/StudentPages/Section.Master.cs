using System;
using System.Web.UI;
namespace Yarmouk.StudentPages
{
    public partial class SectionMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected void SignOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect(ResolveUrl("~/PublicPages/Login.aspx"), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
