using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.AcademicPages
{
    public sealed class CompanyEvaluationPdfDownload : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.AddHeader("X-Content-Type-Options", "nosniff");
            try
            {
                var auth = context.Session == null ? null : context.Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null || string.IsNullOrWhiteSpace(auth.Uid) || string.IsNullOrWhiteSpace(auth.IdToken))
                    throw new UnauthorizedAccessException();
                var user = await new StudentService().GetUserAsync(auth.Uid, auth.IdToken);
                if (!AuthPolicy.ValidUser(user, auth.Uid) || AuthPolicy.NormalizeRole(user.Role) != "academic")
                    throw new UnauthorizedAccessException();
                var bytes = await new SupervisorWorkspaceService().DownloadCompanyEvaluationAsync(
                    context.Request.QueryString["placementId"], auth.Uid, auth.IdToken);
                context.Response.ContentType = "application/pdf";
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"company-evaluation.pdf\"");
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            }
            catch (UnauthorizedAccessException)
            {
                Error(context, 403, "Sign in as the academic supervisor assigned to this student to download the evaluation.");
            }
            catch (SupervisorWorkflowException error) { Error(context, 404, error.Message); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceWarning("Company evaluation PDF download failed: " + error.GetType().Name);
                Error(context, 500, "The evaluation PDF could not be downloaded. Refresh the evaluation page and try again.");
            }
        }

        private static void Error(HttpContext context, int status, string message)
        {
            context.Response.StatusCode = status;
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.ContentType = "text/plain";
            context.Response.Write(message);
        }
    }
}
