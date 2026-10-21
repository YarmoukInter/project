namespace Yarmouk.Models
{
    public class SupervisorStudentProgress
    {
        public string PlacementId { get; set; }
        public string StudentUid { get; set; }
        public string StudentName { get; set; }
        public string Initials { get; set; }
        public string StudentNumber { get; set; }
        public string CompanyName { get; set; }
        public string PlacementStatus { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string TrainingPeriodLabel { get; set; }
        public string CompanyFeedback { get; set; }
        public int ApprovedHours { get; set; }
        public int RequiredHours { get; set; }
        public int ProgressPercent { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int SubmittedTasks { get; set; }
        public string TaskProgressLabel { get; set; }
        public string HoursProgressLabel { get; set; }
        public string FeedbackLabel { get; set; }
    }

    public class AcademicEvaluationItem
    {
        public string PlacementId { get; set; }
        public string EvaluationId { get; set; }
        public string StudentUid { get; set; }
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public string CompanyName { get; set; }
        public string PlacementStatus { get; set; }
        public string AcademicScoreInput { get; set; }
        public string AcademicScoreLabel { get; set; }
        public string AcademicFeedback { get; set; }
        public string CompanyScoreLabel { get; set; }
        public string CompanyStatusLabel { get; set; }
        public string ApprovalStatusLabel { get; set; }
        public string ProposedFinalScoreLabel { get; set; }
        public bool AcademicSubmitted { get; set; }
        public bool CoordinatorApproved { get; set; }
        public bool CanSave { get; set; }
        public bool CanSubmit { get; set; }
        public string EvaluationStatusLabel { get; set; }
    }
}
