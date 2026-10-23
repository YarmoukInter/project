using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;
using Yarmouk.CompanyPages;

// Runs against in-memory Firebase fixtures; no requests reach a real database.
class WorkflowRegressionTests
{
    static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Console.WriteLine("PASS: " + message);
    }

    static void Main()
    {
        Run().GetAwaiter().GetResult();
    }

    static async Task Run()
    {
        TestTrainingProgress();
        var db = JObject.Parse(@"{
          'students': {
            'unplaced': {'academicSupervisorId':'doctor', 'englishName':'Unplaced'},
            'placed': {'academicSupervisorId':'doctor', 'englishName':'Placed'},
            'reassigned': {'academicSupervisorId':'other'},
            'unassigned': {}
          },
          'placements': {
            'old': {'studentUid':'placed', 'status':'completed'},
            'current': {'id':'stale-placement-id', 'studentUid':'placed', 'status':'active', 'academicSupervisorUid':'other'},
            'stale': {'studentUid':'reassigned', 'academicSupervisorUid':'doctor'},
            'orphan': {'studentUid':'missing', 'academicSupervisorUid':'doctor'}
          },
          'users': {'doctor': {'role':'academic'}},
          'companies': {}, 'tasks': {}, 'trainingLogs': {}, 'evaluations': {},
          'majors': {},
          'opportunities': {'opportunity-key': {'id':'stale-opportunity-id','companyId':'company','title':'Internship'}},
          'applications': {'application-key': {'id':'stale-application-id', 'studentUid':'placed','opportunityId':'opportunity-key','status':'Pending'}}
        }");
        var handler = new FirebaseFixture(db);
        typeof(FirebaseHelper).GetField("Client", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, new HttpClient(handler));
        var workspace = new SupervisorWorkspaceService();
        var rows = await workspace.GetAcademicStudentsAsync("doctor", "fixture-token");
        Check(rows.Count == 2 && rows.Any(r => r.StudentUid == "unplaced"), "Assigned students appear without placements; stale and orphan assignments are excluded");
        Check(rows.Single(r => r.StudentUid == "placed").PlacementId == "current", "One row per student prefers active placement and uses its database key");
        Check(rows.Single(r => r.StudentUid == "unplaced").PlacementStatus == "No placement", "Students without training show No placement");
        db["students"]["placed"]["requiredTrainingHours"] = 200;
        db["students"]["placed"]["approvedTrainingHours"] = 200;
        db["tasks"]["submitted-task"] = JObject.Parse(@"{'studentUid':'placed','placementId':'current','status':'InProgress'}");
        db["taskSubmissions"] = JObject.Parse(@"{'first':{'studentUid':'placed','taskId':'submitted-task','reviewStatus':'pending'},'retry':{'studentUid':'placed','taskId':'submitted-task'}}");
        rows = await workspace.GetAcademicStudentsAsync("doctor", "fixture-token");
        Check(rows.Single(r => r.StudentUid == "placed").ProgressPercent == 10
            && rows.Single(r => r.StudentUid == "placed").SubmittedTasks == 1,
            "Supervisor progress reads submission records, counts retries once, and ignores hours for the percentage");
        var queue = await workspace.GetAcademicEvaluationQueueAsync("doctor", "fixture-token");
        Check(queue.Count == 2 && queue.All(r => r.StudentUid == "placed"), "Evaluation access follows current student supervisor only");
        try {
            await workspace.SaveStudentProgressNoteAsync("stale", "doctor", "fixture-token", "Feedback");
            throw new Exception("Stale supervisor was allowed to save feedback");
        } catch (SupervisorWorkflowException) {
            Check(true, "Former supervisor cannot save feedback after reassignment");
        }
        var company = new CompanyWorkflowService();
        var apps = await company.GetApplicationsAsync("company", "fixture-token");
        Check(apps.Single().Id == "application-key", "Approval button receives the actual application database key");
        try {
            await company.SetApplicationStatusAsync("company", "reviewer", apps.Single().Id, "offered", "", "fixture-token");
            throw new Exception("A student with another confirmed placement received an offer");
        } catch (InvalidOperationException) {
            Check(true, "Students with another confirmed placement cannot receive another offer");
        }
        db["applications"]["application-key"]["studentUid"] = "unplaced";
        await company.SetApplicationStatusAsync("company", "reviewer", apps.Single().Id, "offered", "", "fixture-token");
        Check((string)db["applications"]["application-key"]["status"] == "Offered", "Accept application persists the offer on the correct record");
        Check((await company.GetApplicationsAsync("company", "fixture-token")).Single().Status == "offered", "Reload confirms the offer is awaiting student acceptance");
        var opportunities = await company.GetOpportunitiesAsync("company", "fixture-token");
        Check(opportunities.Single().Id == "opportunity-key", "Opportunity actions receive the actual database key");
        await company.SetOpportunityStatusAsync("company", opportunities.Single().Id, "active", "fixture-token");
        Check((string)db["opportunities"]["opportunity-key"]["status"] == "active"
            && (bool)db["opportunities"]["opportunity-key"]["isActive"], "Activating an opportunity updates the correct record and availability");
        try {
            await company.SetApplicationStatusAsync("another-company", "reviewer", "application-key", "offered", "", "fixture-token");
            throw new Exception("Another company was allowed to approve");
        } catch (UnauthorizedAccessException) {
            Check(true, "Other companies cannot approve the application");
        }
    }

    static void TestTrainingProgress()
    {
        var tasks = new Dictionary<string, JObject>();
        var submissions = new Dictionary<string, JObject>();
        Check(TrainingProgress.Percent(0) == 0, "No submissions means 0% progress");
        var assigned = new Dictionary<string, JObject> { ["assigned"] = JObject.Parse(@"{'studentUid':'student','status':'InProgress'}") };
        Check(TrainingProgress.CountSubmitted(assigned, new JObject[0], "student") == 0,
            "Assigned and in-progress tasks receive no submission credit");
        for (int i = 1; i <= 11; i++)
        {
            string id = "task-" + i;
            tasks[id] = JObject.FromObject(new { studentUid = "student", placementId = "placement", opportunityId = "opportunity", status = "InProgress" });
            submissions[id] = JObject.FromObject(new { studentUid = "student", taskId = id, reviewStatus = "pending" });
            int count = TrainingProgress.CountSubmitted(tasks, submissions.Values, "student", "placement", "opportunity");
            Check(count == i && TrainingProgress.Percent(count) == Math.Min(100, i * 10), "Submission " + i + " adds 10% up to 100%, before review");
        }
        submissions["revision"] = JObject.FromObject(new { studentUid = "student", taskId = "task-1", reviewStatus = "approved" });
        submissions["foreign"] = JObject.FromObject(new { studentUid = "other", taskId = "task-1" });
        submissions["orphan"] = JObject.FromObject(new { studentUid = "student", taskId = "missing" });
        tasks["old"] = JObject.FromObject(new { studentUid = "student", placementId = "old", status = "done" });
        tasks["other-opportunity"] = JObject.FromObject(new { studentUid = "student", opportunityId = "old", status = "done" });
        Check(TrainingProgress.CountSubmitted(tasks, submissions.Values, "student", "placement", "opportunity") == 11,
            "Resubmissions, other students, orphan submissions, and other placements add no credit");

        var placements = new Dictionary<string, JObject> {
            ["placement"] = JObject.FromObject(new { studentUid = "student", opportunityId = "opportunity", status = "active", requiredHours = 200, approvedHours = 200 })
        };
        var students = new Dictionary<string, JObject> { ["student"] = JObject.FromObject(new { requiredTrainingHours = 200, approvedTrainingHours = 200 }) };
        var empty = new Dictionary<string, JObject>();
        var oneTask = tasks.Take(1).ToDictionary(t => t.Key, t => t.Value);
        var oneSubmission = submissions.Take(1).ToDictionary(s => s.Key, s => s.Value);
        var academicBuilder = typeof(Yarmouk.AcademicPages.Dashboard).GetMethod("BuildStudentRow", BindingFlags.Static | BindingFlags.NonPublic);
        var academic = (SupervisorStudentProgress)academicBuilder.Invoke(null, new object[] {
            placements.First(), students, empty, empty, oneTask, oneSubmission, empty });
        Check(academic.ProgressPercent == 10 && academic.TaskProgressLabel == "1 of 10 tasks submitted",
            "Academic dashboard shows 10% for one submission even with all hours approved");
        var coordinatorBuilder = typeof(Yarmouk.CoordinatorPages.MonitorPlacements).GetMethod("BuildRows", BindingFlags.Static | BindingFlags.NonPublic);
        var coordinator = ((System.Collections.IEnumerable)coordinatorBuilder.Invoke(null, new object[] {
            placements, students, empty, empty, empty, oneTask, oneSubmission, empty, empty })).Cast<object>().Single();
        Check((int)coordinator.GetType().GetProperty("progress").GetValue(coordinator) == 10,
            "Coordinator monitor shows the same 10% using task database keys when id is missing");
        var companyBuilder = typeof(Yarmouk.CompanyPages.Dashboard).GetMethod("BuildTraineeRow", BindingFlags.Static | BindingFlags.NonPublic);
        var companyRow = companyBuilder.Invoke(null, new object[] { placements.First(), oneTask.ToList(), oneSubmission.ToList(), empty, students, empty });
        Check((int)companyRow.GetType().GetProperty("Percent").GetValue(companyRow) == 10,
            "Company dashboard uses the same fixed ten-task target");
        var milestonesBuilder = typeof(Yarmouk.StudentPages.Progress).GetMethod("BuildMilestones", BindingFlags.Static | BindingFlags.NonPublic);
        var milestones = ((System.Collections.IEnumerable)milestonesBuilder.Invoke(null, new object[] { 3, 10 })).Cast<object>().ToList();
        Check(milestones.Count == 10 && milestones.Count(m => (string)m.GetType().GetProperty("CssClass").GetValue(m) == "is-complete") == 3,
            "Student progress has ten milestones and reaches three after three submissions");
    }

    sealed class FirebaseFixture : HttpMessageHandler
    {
        readonly JObject db;
        public FirebaseFixture(JObject db) { this.db = db; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var path = request.RequestUri.AbsolutePath.Trim('/');
            path = path.Substring(0, path.Length - 5);
            JToken row = db;
            foreach (var key in path.Split('/')) row = row == null ? null : row[key];
            if (request.Method.Method == "PATCH") {
                var changes = JObject.Parse(await request.Content.ReadAsStringAsync());
                foreach (var change in changes.Properties()) row[change.Name] = change.Value;
            }
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(row == null ? "null" : row.ToString())
            };
        }
    }
}
