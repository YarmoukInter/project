using System;
using System.Web.UI;
namespace Yarmouk.AcademicPages
{
    public partial class Account : Page
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
