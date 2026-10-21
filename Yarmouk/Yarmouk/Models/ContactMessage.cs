using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class ContactMessage
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public long CreatedAtUnixMs { get; set; }
    }
}
