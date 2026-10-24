using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;
using Yarmouk.Services;

namespace Yarmouk.CoordinatorPages
{
    public partial class ManageOpportunities : CoordinatorPage
    {
        protected override async Task<object> LoadDataAsync(AuthSession session)
        {
            if (session == null || session.ExpiresUtc <= DateTime.UtcNow || session.User == null
                || !session.User.IsActive || AuthPolicy.NormalizeRole(session.User.Role) != "coordinator")
                throw new UnauthorizedAccessException("Sign in with an active coordinator account.");
            var firebase = new FirebaseHelper();
            var opportunitiesRequest = firebase.GetAsync<Dictionary<string, Opportunity>>(DatabasePaths.Opportunities, session.IdToken);
            var companiesRequest = firebase.GetAsync<Dictionary<string, Company>>(DatabasePaths.Companies, session.IdToken);
            var majorsRequest = firebase.GetAsync<Dictionary<string, Major>>(DatabasePaths.Majors, session.IdToken);
            var applicationsRequest = firebase.GetAsync<Dictionary<string, Application>>(DatabasePaths.Applications, session.IdToken);
            await Task.WhenAll(opportunitiesRequest, companiesRequest, majorsRequest, applicationsRequest);
            var opportunities = await opportunitiesRequest ?? new Dictionary<string, Opportunity>();
            var companies = await companiesRequest ?? new Dictionary<string, Company>();
            var majors = await majorsRequest ?? new Dictionary<string, Major>();
            var applications = await applicationsRequest ?? new Dictionary<string, Application>();
            var counts = applications.Values.Where(a => a != null && !string.IsNullOrEmpty(a.OpportunityId))
                .GroupBy(a => a.OpportunityId).ToDictionary(g => g.Key, g => g.Count());
            return new { records = opportunities.Where(p => p.Value != null).Select(pair => {
                var opportunity = pair.Value;
                Company company;
                companies.TryGetValue(opportunity.CompanyId ?? "", out company);
                var status = (opportunity.Status ?? "").Trim().ToLowerInvariant();
                if (string.IsNullOrEmpty(status)) status = opportunity.IsActive ? "active" : "closed";
                int count;
                counts.TryGetValue(pair.Key, out count);
                return new {
                    id = pair.Key, title = opportunity.Title, company = company == null ? "Unknown provider" : company.Name,
                    location = company == null ? "" : company.Location, status,
                    seats = opportunity.AvailablePositions, filled = opportunity.FilledPositions,
                    applications = count, deadline = opportunity.Deadline,
                    majors = (opportunity.EligibleMajorIds ?? new List<string>()).Select(id => majors.ContainsKey(id) && majors[id] != null ? majors[id].Name : id).ToList(),
                    description = opportunity.Description, requirements = opportunity.Requirements,
                    duration = opportunity.Duration, hours = opportunity.RequiredTrainingHours,
                    workMode = opportunity.WorkMode, skills = opportunity.Skills ?? new List<string>(),
                    questions = opportunity.ApplicationQuestions ?? new List<string>()
                };
            }).OrderBy(row => row.title).ToList() };
        }
    }
}
