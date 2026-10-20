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
        public bool MustChangePassword { get; set; }
    }
}
