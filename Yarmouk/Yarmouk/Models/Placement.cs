using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class Placement
    {
        public string Id { get; set; }
        public string StudentUid { get; set; }
        public string CompanyId { get; set; }
        public string OpportunityId { get; set; }
        public string ApplicationId { get; set; }
        public string AcademicSupervisorUid { get; set; }
        public string CompanySupervisorUid { get; set; }
        public string Status { get; set; }
        public int RequiredHours { get; set; }
    }
}
