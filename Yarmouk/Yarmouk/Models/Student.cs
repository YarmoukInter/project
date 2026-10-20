using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class Student
    {
        public string Uid { get; set; }
        public string StudentId { get; set; }
        public string MajorId { get; set; }
        public string AcademicSupervisorId { get; set; }
        public int CompletedCreditHours { get; set; }
        public string InternshipStatus { get; set; }
        public List<string> TechnicalSkills { get; set; }
        public List<string> SoftSkills { get; set; }
        public string AcceptedApplicationId { get; set; }
        public string CompanyId { get; set; }
        public string TrainingStartDate { get; set; }
        public int RequiredTrainingHours { get; set; }
        public int ApprovedTrainingHours { get; set; }
    }
}
