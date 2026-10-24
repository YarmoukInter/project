using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class ApplicationsReview : CoordinatorPage
    {
        protected override async Task<object> LoadDataAsync(AuthSession session)
        {
            if (session == null || session.ExpiresUtc <= DateTime.UtcNow || session.User == null
                || !session.User.IsActive || AuthPolicy.NormalizeRole(session.User.Role) != "coordinator")
                throw new UnauthorizedAccessException("Sign in with an active coordinator account.");
            var selectedId = (Request.QueryString["opportunityId"] ?? "").Trim();
            var firebase = new FirebaseHelper();
            var appsRequest = firebase.GetAsync<Dictionary<string, Application>>(DatabasePaths.Applications, session.IdToken);
            var opportunitiesRequest = firebase.GetAsync<Dictionary<string, Opportunity>>(DatabasePaths.Opportunities, session.IdToken);
            var usersRequest = firebase.GetAsync<Dictionary<string, User>>(DatabasePaths.Users, session.IdToken);
            var studentsRequest = firebase.GetAsync<Dictionary<string, Student>>(DatabasePaths.Students, session.IdToken);
            var companiesRequest = firebase.GetAsync<Dictionary<string, Company>>(DatabasePaths.Companies, session.IdToken);
            var majorsRequest = firebase.GetAsync<Dictionary<string, Major>>(DatabasePaths.Majors, session.IdToken);
            await Task.WhenAll(appsRequest, opportunitiesRequest, usersRequest, studentsRequest, companiesRequest, majorsRequest);
            var applications = await appsRequest ?? new Dictionary<string, Application>();
            var opportunities = await opportunitiesRequest ?? new Dictionary<string, Opportunity>();
            var users = await usersRequest ?? new Dictionary<string, User>();
            var students = await studentsRequest ?? new Dictionary<string, Student>();
            var companies = await companiesRequest ?? new Dictionary<string, Company>();
            var majors = await majorsRequest ?? new Dictionary<string, Major>();
            if (selectedId.Length > 0 && (!opportunities.ContainsKey(selectedId) || opportunities[selectedId] == null))
                throw new InvalidOperationException("This opportunity is no longer available. Return to the opportunity directory to choose another.");
            var rows = applications.Where(p => p.Value != null && (selectedId.Length == 0 || p.Value.OpportunityId == selectedId))
                .OrderByDescending(p => p.Value.AppliedAtUnixMs).Select(pair => {
                    var application = pair.Value;
                    User user; Student student; Opportunity opportunity; Company company; Major major;
                    users.TryGetValue(application.StudentUid ?? "", out user);
                    students.TryGetValue(application.StudentUid ?? "", out student);
                    if (student == null) student = students.Values.FirstOrDefault(s => s != null && s.Uid == application.StudentUid);
                    opportunities.TryGetValue(application.OpportunityId ?? "", out opportunity);
                    companies.TryGetValue(application.CompanyId ?? (opportunity == null ? "" : opportunity.CompanyId) ?? "", out company);
                    majors.TryGetValue(student == null ? "" : student.MajorId ?? "", out major);
                    return new {
                        id = pair.Key, name = user == null ? "Student unavailable" : user.FullName,
                        number = student == null ? "" : student.StudentNumber ?? student.StudentId,
                        email = user == null ? "" : user.Email,
                        photo = StudentProfileImages.GetUrl(student == null ? null : student.ProfileImageFileName),
                        major = major == null ? "Not specified" : major.Name,
                        opportunity = opportunity == null ? "Opportunity unavailable" : opportunity.Title,
                        company = company == null ? "Provider unavailable" : company.Name,
                        status = string.IsNullOrWhiteSpace(application.Status) ? "new" : application.Status,
                        appliedAt = application.AppliedAtUnixMs,
                        questions = application.ApplicationQuestions ?? (opportunity == null ? null : opportunity.ApplicationQuestions) ?? new List<string>(),
                        answers = application.Answers ?? new List<string>(), rejectionReason = application.RejectionReason
                    };
                }).ToList();
            return new { records = rows, opportunityId = selectedId, opportunityTitle = selectedId.Length == 0 ? "All opportunities" : opportunities[selectedId].Title };
        }
    }
}
