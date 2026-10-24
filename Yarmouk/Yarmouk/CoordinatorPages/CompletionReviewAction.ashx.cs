using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public sealed class CompletionReviewAction : HttpTaskAsyncHandler, IRequiresSessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            context.Response.AddHeader("X-Content-Type-Options", "nosniff");
            try
            {
                if (!string.Equals(context.Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)) throw new HttpException(405, "POST is required.");
                var auth = context.Session == null ? null : context.Session[AuthGuard.SessionKey] as AuthSession;
                if (auth == null || string.IsNullOrWhiteSpace(auth.Uid) || string.IsNullOrWhiteSpace(auth.IdToken)) throw new UnauthorizedAccessException();
                var user = await new StudentService().GetUserAsync(auth.Uid, auth.IdToken).ConfigureAwait(false);
                if (!AuthPolicy.ValidUser(user, auth.Uid) || AuthPolicy.NormalizeRole(user.Role) != "coordinator") throw new UnauthorizedAccessException();
                string token = Convert.ToString(context.Session["FinalCompletionReviewActionToken"]);
                if (string.IsNullOrWhiteSpace(token) || !Same(token, context.Request.Form["requestToken"])) throw new HttpException(403, "Your review session has expired. Refresh the page and try again.");

                string evaluationId = context.Request.Form["evaluationId"];
                if (!ValidKey(evaluationId)) throw new HttpException(400, "The selected evaluation is invalid.");
                string action = context.Request.Form["action"];
                var firebase = new FirebaseHelper();
                var evaluation = await firebase.GetAsync<JObject>(DatabasePaths.Evaluations + "/" + evaluationId, auth.IdToken).ConfigureAwait(false);
                if (evaluation == null) throw new HttpException(404, "The selected evaluation no longer exists.");
                string studentUid = Read(evaluation, "studentUid");
                string placementId = Read(evaluation, "placementId");
                if (!ValidKey(studentUid) || !ValidKey(placementId)) throw new HttpException(400, "The evaluation is missing its student or placement reference.");

                var studentTask = firebase.GetAsync<JObject>(DatabasePaths.Students + "/" + studentUid, auth.IdToken);
                var placementTask = firebase.GetAsync<JObject>(DatabasePaths.Placements + "/" + placementId, auth.IdToken);
                var logsTask = firebase.GetAsync<Dictionary<string, JObject>>(DatabasePaths.TrainingLogs, auth.IdToken);
                await Task.WhenAll(studentTask, placementTask, logsTask).ConfigureAwait(false);
                var student = await studentTask;
                var placement = await placementTask;
                if (student == null || placement == null) throw new HttpException(409, "The student or placement record is no longer available.");

                if (Same(action, "approve"))
                {
                    EnsureEligible(evaluation, student, placement, await logsTask, studentUid, placementId);
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    await firebase.UpdateRootAsync(new Dictionary<string, object>
                    {
                        { DatabasePaths.Evaluations + "/" + evaluationId + "/coordinatorApproved", true },
                        { DatabasePaths.Evaluations + "/" + evaluationId + "/coordinatorUid", auth.Uid },
                        { DatabasePaths.Evaluations + "/" + evaluationId + "/coordinatorApprovedAtUnixMs", now },
                        { DatabasePaths.Placements + "/" + placementId + "/status", "completed" },
                        { DatabasePaths.Placements + "/" + placementId + "/completionApprovedAtUnixMs", now },
                        { DatabasePaths.Students + "/" + studentUid + "/internshipStatus", "Completed" }
                    }, auth.IdToken).ConfigureAwait(false);
                    Write(context, 200, new { success = true, message = "Completion approved. You can now generate the official PDF." });
                    return;
                }

                if (Same(action, "certificate"))
                {
                    if (!Flag(evaluation, "coordinatorApproved")) throw new HttpException(409, "Approve completion before generating the official PDF.");
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    await firebase.UpdateRootAsync(new Dictionary<string, object>
                    {
                        { DatabasePaths.Evaluations + "/" + evaluationId + "/officialCertificateGeneratedAtUnixMs", now },
                        { DatabasePaths.Students + "/" + studentUid + "/completionCertificateAvailable", true },
                        { DatabasePaths.Students + "/" + studentUid + "/completionCertificateEvaluationId", evaluationId }
                    }, auth.IdToken).ConfigureAwait(false);
                    Write(context, 200, new { success = true, message = "The official PDF has been generated and made available to the student.", pdfUrl = "CompletionCertificate.ashx?evaluationId=" + HttpUtility.UrlEncode(evaluationId) });
                    return;
                }

                if (Same(action, "download"))
                {
                    if (!Flag(evaluation, "coordinatorApproved")) throw new HttpException(409, "Approve completion before downloading the official PDF.");
                    string companyId = Read(placement, "companyId");
                    string opportunityId = Read(placement, "opportunityId");
                    var company = ValidKey(companyId) ? await firebase.GetAsync<JObject>(DatabasePaths.Companies + "/" + companyId, auth.IdToken).ConfigureAwait(false) : null;
                    var opportunity = ValidKey(opportunityId) ? await firebase.GetAsync<JObject>(DatabasePaths.Opportunities + "/" + opportunityId, auth.IdToken).ConfigureAwait(false) : null;
                    var bytes = CompletionCertificatePdf.Create(First(Read(student, "englishName"), Read(student, "fullName")),
                        First(Read(student, "studentId"), Read(student, "studentNumber")), Read(company, "name"), Read(opportunity, "title"), evaluationId, evaluation);
                    context.Response.ContentType = "application/pdf";
                    context.Response.AddHeader("Content-Disposition", "attachment; filename=\"internship-completion-certificate.pdf\"");
                    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    return;
                }

                throw new HttpException(400, "The requested action is invalid.");
            }
            catch (UnauthorizedAccessException) { Write(context, 403, new { success = false, message = "Sign in as a coordinator to perform this action." }); }
            catch (HttpException error) { Write(context, error.GetHttpCode(), new { success = false, message = error.Message }); }
            catch (Exception error)
            {
                System.Diagnostics.Trace.TraceError("Completion review action failed: " + error);
                Write(context, 500, new { success = false, message = "The completion action could not be saved. Refresh the page and try again." });
            }
        }

        private static void EnsureEligible(JObject evaluation, JObject student, JObject placement, IDictionary<string, JObject> logs, string studentUid, string placementId)
        {
            int required = Number(student, "requiredTrainingHours");
            if (required <= 0) required = Number(student, "requiredHours");
            if (required <= 0) required = Number(placement, "requiredHours");
            int approved = Number(student, "approvedTrainingHours");
            if (approved <= 0) approved = (logs ?? new Dictionary<string, JObject>()).Where(item => item.Value != null && Same(Read(item.Value, "studentUid"), studentUid)
                && (string.IsNullOrWhiteSpace(Read(item.Value, "placementId")) || Same(Read(item.Value, "placementId"), placementId))
                && (Flag(item.Value, "isApproved") || Same(Read(item.Value, "status"), "approved"))).Sum(item => Number(item.Value, "hours"));
            if (!Flag(evaluation, "companySubmitted") || !Flag(evaluation, "academicSubmitted") || required <= 0 || approved < required)
                throw new HttpException(409, "Completion cannot be approved until both final evaluations and all required approved hours are recorded.");
        }
        private static void Write(HttpContext context, int status, object body)
        {
            context.Response.StatusCode = status;
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.ContentType = "application/json";
            context.Response.Write(JsonConvert.SerializeObject(body));
        }
        private static string Read(JObject record, string key) { return record == null || record[key] == null ? string.Empty : Convert.ToString(record[key], CultureInfo.InvariantCulture); }
        private static int Number(JObject record, string key) { int value; return int.TryParse(Read(record, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out value) ? Math.Max(0, value) : 0; }
        private static bool Flag(JObject record, string key) { bool value; return bool.TryParse(Read(record, key), out value) && value; }
        private static bool Same(string left, string right) { return string.Equals(left, right, StringComparison.Ordinal); }
        private static string First(string first, string second) { return string.IsNullOrWhiteSpace(first) ? second : first; }
        private static bool ValidKey(string value) { return !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value.IndexOfAny(new[] { '.', '#', '$', '[', ']', '/' }) < 0; }
    }
}
