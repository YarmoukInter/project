using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Services;

class CompletionWorkflowTests
{
    static void Assert(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("PASS: " + label); }
    static async Task Reject(Func<Task> action, string label) {
        try { await action(); } catch (ArgumentException) { Assert(true, label); return; }
        catch (InvalidOperationException) { Assert(true, label); return; }
        catch (UnauthorizedAccessException) { Assert(true, label); return; }
        throw new Exception("Expected rejection: " + label);
    }
    static void Main(string[] args) { Run(args[0]).GetAwaiter().GetResult(); }
    static async Task Run(string output) {
        var db = JObject.Parse(@"{
            'users': {'admin':{'role':'admin','isActive':true},'student':{'role':'student','isActive':true,'fullName':'طالب تجريبي'},'other':{'role':'student','isActive':true}},
            'students': {'student':{'englishName':'Synthetic Student / طالب تجريبي','studentNumber':'QA-001','requiredTrainingHours':160,'approvedTrainingHours':0}},
            'placements': {'placement':{'studentUid':'student','companyId':'company','opportunityId':'opportunity','requiredHours':160,'approvedHours':0}},
            'companies': {'company':{'name':'Synthetic Training Company'}},
            'opportunities': {'opportunity':{'title':'Software Engineering Internship'}},
            'evaluations': {'evaluation':{'studentUid':'student','placementId':'placement','companySubmitted':true,'academicSubmitted':false,'companyScore':90,'academicScore':92}},
            'trainingLogs': {}, 'completionReports': {}
        }");
        var fixture = new Fixture(db);
        typeof(FirebaseHelper).GetField("Client", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, new HttpClient(fixture));
        var service = new CompletionService();
        await Reject(() => service.ApproveHoursAsync("evaluation", "student", "token", 160, "Verified"), "Students cannot approve hours");
        await Reject(() => service.ApproveHoursAsync("evaluation", "admin", "token", -1, "Verified"), "Negative hours rejected");
        await Reject(() => service.ApproveHoursAsync("evaluation", "admin", "token", 160, ""), "Hours require an evidence note");
        await Reject(() => service.ApproveCompletionAsync("evaluation", "admin", "token"), "Missing academic submission blocks completion");
        db["evaluations"]["evaluation"]["academicSubmitted"] = true;
        await Reject(() => service.ApproveCompletionAsync("evaluation", "admin", "token"), "Insufficient hours block completion");
        await service.ApproveHoursAsync("evaluation", "admin", "token", 160, "Synthetic fixture: verified attendance");
        Assert((int)db["students"]["student"]["approvedTrainingHours"] == 160 && (int)db["placements"]["placement"]["approvedHours"] == 160, "Hour approval updates both student and placement atomically");
        await Reject(() => service.SendReportAsync("evaluation", "admin", "token"), "PDF cannot be sent before coordinator approval");
        db["placements"]["placement"]["studentUid"] = "other";
        await Reject(() => service.ApproveCompletionAsync("evaluation", "admin", "token"), "Mismatched placement ownership rejected");
        db["placements"]["placement"]["studentUid"] = "student";
        await service.ApproveCompletionAsync("evaluation", "admin", "token");
        Assert((bool)db["evaluations"]["evaluation"]["coordinatorApproved"] && (decimal)db["evaluations"]["evaluation"]["finalScore"] == 91.2m, "Coordinator approval saves weighted grade 91.2/100");
        Assert((string)db["students"]["student"]["internshipStatus"] == "Completed" && (string)db["placements"]["placement"]["status"] == "Completed", "Completion status persisted for student and placement");
        await Reject(() => service.ApproveHoursAsync("evaluation", "admin", "token", 200, "Changed"), "Approved completion hours are locked");
        await service.SendReportAsync("evaluation", "admin", "token");
        var report = (JObject)db["completionReports"]["student"]["evaluation"];
        Assert(report != null && db["evaluations"]["evaluation"]["reportSentAtUnixMs"] != null, "PDF and delivery marker published together");
        var inbox = new CompletionInboxService();
        var reports = await inbox.ListAsync("student", "token");
        Assert(reports.Count == 1 && !(bool)reports[0]["read"] && reports[0]["dataBase64"] == null, "Student receives unread metadata without PDF bytes in notification response");
        var bytes = await inbox.GetPdfAsync("student", "evaluation", "token");
        Assert(System.Text.Encoding.ASCII.GetString(bytes, 0, 5) == "%PDF-", "Student can retrieve a real PDF");
        File.WriteAllBytes(output, bytes);
        await Reject(() => inbox.GetPdfAsync("other", "evaluation", "token"), "Another student cannot download the report");
        await inbox.MarkReadAsync("student", "evaluation", "token");
        Assert((bool)(await inbox.ListAsync("student", "token"))[0]["read"], "Acknowledgement persists across sessions");
        string snapshot = (string)report["dataBase64"];
        long sent = (long)report["sentAtUnixMs"];
        await service.SendReportAsync("evaluation", "admin", "token");
        Assert((long)report["sentAtUnixMs"] == sent && (bool)(await inbox.ListAsync("student", "token"))[0]["read"], "Repeated send does not duplicate notification or reset acknowledgement");
        db["evaluations"]["evaluation"]["academicScore"] = 10;
        Assert(Convert.ToBase64String(await service.GetCoordinatorPdfAsync("evaluation", "admin", "token")) == snapshot, "Printed PDF matches the issued snapshot after source changes");
        Assert((await inbox.ListAsync("other", "token")).Count == 0, "Inbox never returns another student's report");
        Console.WriteLine("All completion workflow checks passed using in-memory data. No live records changed.");
    }
    sealed class Fixture : HttpMessageHandler {
        readonly JObject db;
        public Fixture(JObject db) { this.db = db; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) {
            if (request.RequestUri.Host != "fixture.invalid") throw new Exception("Tests must never contact a live database");
            var path = request.RequestUri.AbsolutePath.Trim('/'); path = path.Substring(0, path.Length - 5);
            JToken value = db;
            if (request.Method.Method == "PATCH") {
                var changes = JObject.Parse(await request.Content.ReadAsStringAsync());
                foreach (var change in changes.Properties()) {
                    string fullPath = string.IsNullOrEmpty(path) ? change.Name : path + "/" + change.Name;
                    var segments = fullPath.Split('/'); JObject parent = db;
                    for (int i=0; i<segments.Length-1; i++) {
                        if (parent[segments[i]] == null) parent[segments[i]] = new JObject();
                        parent = (JObject)parent[segments[i]];
                    }
                    parent[segments.Last()] = change.Value.DeepClone();
                }
            }
            if (!string.IsNullOrEmpty(path)) foreach (var key in path.Split('/')) value = value == null ? null : value[key];
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(value == null ? "null" : value.ToString()) };
        }
    }
}
