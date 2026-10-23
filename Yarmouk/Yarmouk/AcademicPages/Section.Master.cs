using System;
using System.Web;
using System.Web.UI;
namespace Yarmouk.AcademicPages
{
    public partial class SectionMaster : MasterPage
    {
        protected string SidebarCsrf { get { return (string)Session["SidebarCsrf"]; } }
        protected override void OnInit(EventArgs e)
        {
            if (Session["SidebarCsrf"] == null) Session["SidebarCsrf"] = Guid.NewGuid().ToString("N");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            base.OnInit(e);
        }
        protected void SidebarLogout_Click(object sender, EventArgs e)
        {
            if (Request.Form["sidebarCsrf"] != SidebarCsrf) throw new HttpException(403, "Please reload the page and try again.");
            Session.Clear();
            Session.Abandon();
            Response.Redirect(ResolveUrl("~/PublicPages/Login"));
        }
    }
}
