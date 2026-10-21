using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class Company
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Industry { get; set; }
        public string Location { get; set; }
        public string ContactEmail { get; set; }
        public string LogoUrl { get; set; }
        public bool IsApproved { get; set; }
        public string SupervisorUid { get; set; }
    }
}
