using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class Application
    {
        public string Id { get; set; }
        public string StudentUid { get; set; }
        public string OpportunityId { get; set; }
        public string Status { get; set; }
        public long AppliedAtUnixMs { get; set; }
    }
}
