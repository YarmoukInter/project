using System;
using System.Web.UI;
namespace Yarmouk.PublicPages
{
    public partial class Home : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var settings = Yarmouk.Helpers.SiteSettingsStore.Read(Server);
            Page.Title = (string)settings["homeTitle"] ?? "Yarmouk University — Your practical training journey";
            HomeDescriptionMeta.Content = (string)settings["homeDescription"] ?? string.Empty;
            HomeKeywordsMeta.Content = (string)settings["metaKeywords"] ?? string.Empty;
            if (!IsPostBack)
            {
                // Add page loading logic here.
            }
        }
    }
}
