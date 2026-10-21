using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class Evaluation
    {
        public string Id { get; set; }
        public string StudentUid { get; set; }
        public string AcademicSupervisorUid { get; set; }
        public string CompanySupervisorUid { get; set; }
        public double AcademicScore { get; set; }
        public double CompanyScore { get; set; }
        public bool AcademicSubmitted { get; set; }
        public bool CompanySubmitted { get; set; }
        public bool CoordinatorApproved { get; set; }
        // Display only after both evaluations and coordinator approval.
        public double WeightedScore { get { return System.Math.Round(AcademicScore * 0.60 + CompanyScore * 0.40, 2); } }
    }
}
