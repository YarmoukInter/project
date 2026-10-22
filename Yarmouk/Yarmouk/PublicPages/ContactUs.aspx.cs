using System;
using System.Web.UI;
using Yarmouk.Helpers;

namespace Yarmouk.PublicPages
{
    public partial class ContactUs : Page
    {
        protected bool MessageSent { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            ViewStateUserKey = Session.SessionID;
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["ContactSent"] != null)
            {
                Session.Remove("ContactSent");
                MessageSent = true;
                ContactStatus.Text = "Your message was sent to the internship coordinator.";
            }
        }

        protected void SendButton_Click(object sender, EventArgs e)
        {
            string name = ContactName.Text.Trim();
            string email = ContactEmail.Text.Trim();
            string subject = ContactSubject.Text.Trim();
            string message = ContactMessage.Text.Trim();

            if (name.Length == 0 || email.Length == 0 || subject.Length == 0 || message.Length == 0)
            {
                ContactStatus.Text = "Please complete all fields.";
                return;
            }

            if (name.Length > 120 || email.Length > 254 || subject.Length > 180 || message.Length > 5000)
            {
                ContactStatus.Text = "One or more fields exceed the allowed length.";
                return;
            }

            if (!AuthPolicy.ValidEmail(email))
            {
                ContactStatus.Text = "Please enter a valid email address.";
                return;
            }

            RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                try
                {
                    var contactMessage = new Yarmouk.Models.ContactMessage
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        FullName = name,
                        Email = email,
                        Subject = subject,
                        Message = message,
                        CreatedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    await new FirebaseHelper().SaveContactMessageAsync(contactMessage);

                    Session["ContactSent"] = true;
                    Response.Redirect(Request.Path, false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Contact submission failed: " + ex.GetType().Name);
                    ContactStatus.Text = "Your message could not be sent. Please try again later or contact the internship office.";
                }
            }));
        }
    }
}
