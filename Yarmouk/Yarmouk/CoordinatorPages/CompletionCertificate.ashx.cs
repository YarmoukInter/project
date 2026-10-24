using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public sealed class CompletionCertificateDownload : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.AddHeader("X-Content-Type-Options", "nosniff");
            try
            {
                var auth = context.Session == null ? null : context.Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null || string.IsNullOrWhiteSpace(auth.Uid) || string.IsNullOrWhiteSpace(auth.IdToken)) throw new UnauthorizedAccessException();
                string evaluationId = context.Request.QueryString["evaluationId"];
                if (!ValidKey(evaluationId)) throw new HttpException(400, "The certificate reference is invalid.");
                var user = await new StudentService().GetUserAsync(auth.Uid, auth.IdToken).ConfigureAwait(false);
                if (!AuthPolicy.ValidUser(user, auth.Uid)) throw new UnauthorizedAccessException();
                var firebase = new FirebaseHelper();
                var evaluation = await firebase.GetAsync<JObject>(DatabasePaths.Evaluations + "/" + evaluationId, auth.IdToken).ConfigureAwait(false);
                if (evaluation == null || !Flag(evaluation, "coordinatorApproved")) throw new HttpException(404, "An approved completion certificate was not found.");
                string studentUid = Read(evaluation, "studentUid");
                string role = AuthPolicy.NormalizeRole(user.Role);
                if (role != "coordinator" && !(role == "student" && Same(auth.Uid, studentUid))) throw new UnauthorizedAccessException();

                var student = await firebase.GetAsync<JObject>(DatabasePaths.Students + "/" + studentUid, auth.IdToken).ConfigureAwait(false);
                var placement = await firebase.GetAsync<JObject>(DatabasePaths.Placements + "/" + Read(evaluation, "placementId"), auth.IdToken).ConfigureAwait(false);
                var company = placement == null ? null : await firebase.GetAsync<JObject>(DatabasePaths.Companies + "/" + Read(placement, "companyId"), auth.IdToken).ConfigureAwait(false);
                var opportunity = placement == null ? null : await firebase.GetAsync<JObject>(DatabasePaths.Opportunities + "/" + Read(placement, "opportunityId"), auth.IdToken).ConfigureAwait(false);
                var bytes = CompletionCertificatePdf.Create(First(Read(student, "englishName"), Read(student, "fullName")), First(Read(student, "studentId"), Read(student, "studentNumber")), Read(company, "name"), Read(opportunity, "title"), evaluationId, evaluation);
                context.Response.ContentType = "application/pdf";
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"internship-completion-certificate.pdf\"");
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            }
            catch (UnauthorizedAccessException) { Error(context, 403, "You do not have access to this certificate."); }
            catch (HttpException error) { Error(context, error.GetHttpCode(), error.Message); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceError("Completion certificate download failed: " + error);
                Error(context, 500, "The certificate could not be generated. Refresh the page and try again.");
            }
        }
        private static void Error(HttpContext context, int status, string message) { context.Response.StatusCode = status; context.Response.TrySkipIisCustomErrors = true; context.Response.ContentType = "text/plain"; context.Response.Write(message); }
        private static string Read(JObject record, string key) { return record == null || record[key] == null ? string.Empty : record[key].ToString(); }
        private static string First(string first, string second) { return string.IsNullOrWhiteSpace(first) ? second : first; }
        private static bool Flag(JObject record, string key) { bool value; return bool.TryParse(Read(record, key), out value) && value; }
        private static bool Same(string left, string right) { return string.Equals(left, right, StringComparison.Ordinal); }
        private static bool ValidKey(string value) { return !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/' }) < 0; }
    }
}
