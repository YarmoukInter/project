using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class TrainingTask
    {
        public string Id { get; set; }
        public string StudentUid { get; set; }
        public string SupervisorUid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public long DueAtUnixMs { get; set; }
    }
}
