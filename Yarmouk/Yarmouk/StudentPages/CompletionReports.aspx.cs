using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.StudentPages
{
    public partial class CompletionReports : Page
    {
        private bool IsEndpoint { get { return Request.QueryString["action"] != null || Request.QueryString["pdf"] != null; } }
        protected override void Render(HtmlTextWriter writer) { if (!IsEndpoint) base.Render(writer); }
        protected void Page_Load(object sender, EventArgs e) {
            if (IsEndpoint) RegisterAsyncTask(new PageAsyncTask(ProcessAsync));
        }
        private async Task ProcessAsync() {
            Response.Clear(); Response.Cache.SetNoStore(); Response.TrySkipIisCustomErrors = true;
            try {
                var auth = Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null) throw new UnauthorizedAccessException();
                var service = new CompletionInboxService();
                if (Request.HttpMethod == "GET" && Request.QueryString["pdf"] != null) {
                    var bytes = await service.GetPdfAsync(auth.Uid, Request.QueryString["pdf"], auth.IdToken);
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", "inline; filename=internship-final-report.pdf");
                    Response.AddHeader("X-Content-Type-Options", "nosniff");
                    Response.BinaryWrite(bytes); return;
                }
                if (Request.HttpMethod == "GET" && Request.QueryString["action"] == "list") {
                    WriteJson(200, new { success = true, reports = await service.ListAsync(auth.Uid, auth.IdToken) }); return;
                }
                if (Request.HttpMethod != "POST" || Request.QueryString["action"] != "read") throw new ArgumentException();
                var csrf = Session["CompletionInboxCsrf"] as string;
                if (string.IsNullOrEmpty(csrf) || Request.Headers["X-Completion-CSRF"] != csrf) throw new UnauthorizedAccessException();
                if (Request.ContentLength > 4096) throw new ArgumentException();
                JObject body;
                using (var reader = new StreamReader(Request.InputStream)) body = JObject.Parse(reader.ReadToEnd());
                await service.MarkReadAsync(auth.Uid, (string)body["id"], auth.IdToken);
                WriteJson(200, new { success = true });
            } catch (UnauthorizedAccessException) { WriteJson(403, new { success = false, message = "This report is not available for your account. Please sign in again." }); }
              catch (ArgumentException) { WriteJson(400, new { success = false, message = "Invalid report request." }); }
              catch (Exception error) {
                Trace.Warn("CompletionReports", "Could not load student reports", error);
                WriteJson(500, new { success = false, message = "Reports could not be loaded. Please try again." });
              }
        }
        private void WriteJson(int status, object value) {
            Response.StatusCode = status; Response.ContentType = "application/json";
            Response.Write(JsonConvert.SerializeObject(value));
        }
    }
}
