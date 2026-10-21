using System;
using System.Web.UI;
namespace Yarmouk.PublicPages
{
    public partial class ContactUs : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Add page loading logic here.
            }
        }

        protected void SendButton_Click(object sender, EventArgs e)
        {
            string name = Request.Form["name"];
            string email = Request.Form["email"];
            string subject = Request.Form["subject"];
            string message = Request.Form["message"];

            // Firebase saving goes here next.
        }
    }
}
