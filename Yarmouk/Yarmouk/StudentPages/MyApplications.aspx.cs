using System;
using System.Web.UI;
namespace Yarmouk.StudentPages
{
    public partial class MyApplications : Page
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
