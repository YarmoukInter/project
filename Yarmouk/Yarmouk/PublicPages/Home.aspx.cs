using System;
using System.Web.UI;
namespace Yarmouk.PublicPages
{
    public partial class Home : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Add page loading logic here.
            }
        }
    }
}
