using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class Opportunity
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> EligibleMajorIds { get; set; }
        public int AvailablePositions { get; set; }
        public int FilledPositions { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public string Deadline { get; set; }
        public List<string> ApplicationQuestions { get; set; }
        public string Requirements { get; set; }
        public string Duration { get; set; }
        public int RequiredTrainingHours { get; set; }
        public string WorkMode { get; set; }
        public List<string> Skills { get; set; }
    }
}
