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

        public string StudentNumber { get; set; }
        public string EnglishName { get; set; }
        public string NationalId { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string BirthPlace { get; set; }
        public string BirthDate { get; set; }
        public string AcademicStatus { get; set; }
        public string Gpa { get; set; }
        public string ExpectedGraduation { get; set; }
        public string Term { get; set; }
        public string AcademicSupervisorName { get; set; }
        public int CompletedCredits { get; set; }
        public int RequiredHours { get; set; }
        public string CvUrl { get; set; }
        public string ProfileImageFileName { get; set; }
        public bool CompletionCertificateAvailable { get; set; }
        public string CompletionCertificateEvaluationId { get; set; }

    }
}
