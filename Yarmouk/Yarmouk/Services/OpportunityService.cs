using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Yarmouk.Helpers;
using Yarmouk.Models;
namespace Yarmouk.Services
{
    public class OpportunityService
    {
        private readonly FirebaseHelper firebase = new FirebaseHelper();
        public async Task<List<Opportunity>> GetAllAsync(string idToken)
        {
            var data = await firebase.GetAsync<Dictionary<string, Opportunity>>(DatabasePaths.Opportunities, idToken);
            var opportunities = new List<Opportunity>();
            if (data == null) return opportunities;
            foreach (var item in data)
            {
                if (item.Value == null) continue;
                if (string.IsNullOrWhiteSpace(item.Value.Id)) item.Value.Id = item.Key;
                opportunities.Add(item.Value);
            }
            return opportunities;
        }

        public async Task<Opportunity> GetByIdAsync(string id, string idToken)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            var opportunity = await firebase.GetAsync<Opportunity>(DatabasePaths.Opportunities + "/" + id, idToken);
            if (opportunity != null && string.IsNullOrWhiteSpace(opportunity.Id)) opportunity.Id = id;
            return opportunity;
        }

        public async Task<List<Opportunity>> GetAvailableForStudentAsync(Student student, string idToken)
        {
            if (student == null) throw new ArgumentNullException("student");

            var opportunities = await GetAllAsync(idToken).ConfigureAwait(false);
            return opportunities.Where(opportunity => IsAvailableForStudent(opportunity, student)).ToList();
        }

        public static bool IsAvailableForStudent(Opportunity opportunity, Student student)
        {
            if (opportunity == null || student == null || !opportunity.IsActive) return false;
            if (!IsBeforeDeadline(opportunity.Deadline)) return false;

            var eligibleMajors = opportunity.EligibleMajorIds ?? new List<string>();
            if (eligibleMajors.Count > 0 && !eligibleMajors.Any(majorId =>
                string.Equals(majorId, student.MajorId, StringComparison.OrdinalIgnoreCase))) return false;

            int filledPositions = 0;
            var filledPositionsProperty = typeof(Opportunity).GetProperty("FilledPositions");
            if (filledPositionsProperty != null)
            {
                var value = filledPositionsProperty.GetValue(opportunity, null);
                if (value != null) int.TryParse(value.ToString(), out filledPositions);
            }
            if (opportunity.AvailablePositions > 0 && filledPositions >= opportunity.AvailablePositions) return false;

            return true;
        }

        private static bool IsBeforeDeadline(string deadline)
        {
            if (string.IsNullOrWhiteSpace(deadline)) return true;
            DateTime parsed;
            if (!DateTime.TryParse(deadline, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out parsed)) return false;
            return parsed.Date >= DateTime.Today;
        }
    }
}
