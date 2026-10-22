using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class Application
    {
        public string Id { get; set; }
        public string StudentUid { get; set; }
        public string OpportunityId { get; set; }
        public string CompanyId { get; set; }
        public string Status { get; set; }
        public long AppliedAtUnixMs { get; set; }
        public List<string> Answers { get; set; }
        public List<string> ApplicationQuestions { get; set; }
        public string RejectionReason { get; set; }
    }
}
