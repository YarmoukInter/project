using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class User
    {
        public string Uid { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool MustChangePassword { get; set; } = true;
        public string Phone { get; set; }
        public string CvLink { get; set; }
        public string PortfolioLink { get; set; }
        public string StudentId { get; set; }
        public string MajorId { get; set; }
        public string AcademicSupervisorId { get; set; }
    }
}
