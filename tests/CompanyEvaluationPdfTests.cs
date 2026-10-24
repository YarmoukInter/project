using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Yarmouk.Helpers;
using Yarmouk.Services;

class CompanyEvaluationPdfTests
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("PASS: " + message);
    }

    static void Main() { Run().GetAwaiter().GetResult(); }

    static async Task Run()
    {
        var db = JObject.Parse(@"{
          'students': {'student': {'academicSupervisorId':'doctor', 'englishName':'Test Student', 'studentId':'20260001'}},
          'placements': {'placement': {'studentUid':'student', 'companyId':'company','status':'active', 'startDate':'2026-06-01','endDate':'2026-09-01'}},
          'users': {'doctor': {'role':'academic'}, 'other': {'role':'academic'}, 'company-user': {'role':'company'}},
          'companies': {'company': {'name':'Training Company'}},
          'tasks': {}, 'trainingLogs': {}, 'applications': {},
          'evaluations': {'evaluation': {'studentUid':'student','placementId':'placement','academicSupervisorUid':'doctor',
            'companySubmitted':false,'companyScore':87,'companyFeedback':'Detailed company observations.', 'companySubmittedAtUnixMs':1792692000000}}
        }");
        typeof(FirebaseHelper).GetField("Client", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, new HttpClient(new Fixture(db)));
        var service = new SupervisorWorkspaceService();
        Check(!(await service.GetAcademicEvaluationQueueAsync("doctor", "token")).Single().CompanySubmitted,
            "A scored draft does not display a PDF link");
        await Denied<SupervisorWorkflowException>(() => service.DownloadCompanyEvaluationAsync("placement", "doctor", "token"),
            "Direct downloads reject drafts even when they contain a score");
        var record = (JObject)db["evaluations"]["evaluation"];
        record["companySubmitted"] = true;
        var criteria = new JObject();
        for (int i = 1; i <= 18; i++) criteria[i.ToString()] = i == 11 ? "N/A" : "Very Good";
        record["companyCriteria"] = criteria;
        Check((await service.GetAcademicEvaluationQueueAsync("doctor", "token")).Single().CompanySubmitted,
            "Submitted company evaluations display a PDF link");
        var pdf = await service.DownloadCompanyEvaluationAsync("placement", "doctor", "token");
        Check(Encoding.ASCII.GetString(pdf, 0, 5) == "%PDF-", "Assigned doctor receives a real PDF");
        File.WriteAllBytes("company-evaluation.pdf", pdf);
        var arabicStudent = new Yarmouk.Models.SupervisorStudentProgress {
            StudentName = "رشا أحمد الخطيب", StudentNumber = "20260001", CompanyName = "شركة التدريب",
            TrainingPeriodLabel = "01 Jun 2026 - 01 Sep 2026"
        };
        var arabicRecord = (JObject)record.DeepClone();
        arabicRecord["companyFeedback"] = "أداء ممتاز والتزام واضح بمتطلبات التدريب والعمل الجماعي.";
        File.WriteAllBytes("company-evaluation-arabic.pdf", CompanyEvaluationPdf.Create(arabicStudent, "arabic-test", arabicRecord));
        arabicRecord["academicScore"] = 92;
        arabicRecord["companyScore"] = 88;
        File.WriteAllBytes("completion-certificate-arabic.pdf", CompletionCertificatePdf.Create(
            arabicStudent.StudentName, arabicStudent.StudentNumber, arabicStudent.CompanyName,
            "تطوير البرمجيات", "arabic-test", arabicRecord));
        Check(true, "Arabic evaluation and certificate render using Windows shaping and bidi layout");
        await Denied<UnauthorizedAccessException>(() => service.DownloadCompanyEvaluationAsync("placement", "other", "token"),
            "Another doctor cannot download this evaluation");
        await Denied<UnauthorizedAccessException>(() => service.DownloadCompanyEvaluationAsync("placement", "company-user", "token"),
            "Company accounts cannot access the doctor's download");
        await Denied<SupervisorWorkflowException>(() => service.DownloadCompanyEvaluationAsync("../placement", "doctor", "token"),
            "Invalid placement keys are rejected");
        await Denied<UnauthorizedAccessException>(() => service.DownloadCompanyEvaluationAsync("placement", "doctor", ""),
            "Downloads require a verified identity token");
        db["students"]["student"]["academicSupervisorId"] = "other";
        await Denied<UnauthorizedAccessException>(() => service.DownloadCompanyEvaluationAsync("placement", "doctor", "token"),
            "The former doctor loses access after reassignment");
        db["students"]["student"]["academicSupervisorId"] = "doctor";
        var array = new JArray { JValue.CreateNull() };
        for (int i = 1; i <= 18; i++) array.Add(criteria[i.ToString()]);
        record["companyCriteria"] = array;
        record["companyFeedback"] = string.Concat(Enumerable.Repeat("Long observations: achievements, teamwork, and recommendations. ", 60));
        File.WriteAllBytes("company-evaluation-long.pdf", await service.DownloadCompanyEvaluationAsync("placement", "doctor", "token"));
        record["academicSubmitted"] = true;
        record["coordinatorApproved"] = true;
        Check((await service.DownloadCompanyEvaluationAsync("placement", "doctor", "token")).Length > 0,
            "PDF stays available after academic submission and university approval");
        record.Remove("companySubmitted");
        Check((await service.DownloadCompanyEvaluationAsync("placement", "doctor", "token")).Length > 0,
            "Legacy submitted evaluations follow the existing score-based submission rule");
    }

    static async Task Denied<T>(Func<Task<byte[]>> action, string message) where T : Exception
    {
        try { await action(); } catch (T) { Check(true, message); return; }
        throw new Exception(message);
    }

    sealed class Fixture : HttpMessageHandler
    {
        readonly JObject db;
        public Fixture(JObject db) { this.db = db; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            string path = request.RequestUri.AbsolutePath.Trim('/');
            path = path.Substring(0, path.Length - 5);
            JToken value = db;
            foreach (var key in path.Split('/')) value = value == null ? null : value[key];
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(value == null ? "null" : value.ToString())
            });
        }
    }
}
