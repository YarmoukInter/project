using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class TaskSubmission
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string StudentUid { get; set; }
        public string SubmissionUrl { get; set; }
        public string Notes { get; set; }
        public long SubmittedAtUnixMs { get; set; }
    }
}
