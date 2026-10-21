using System.Collections.Generic;
namespace Yarmouk.Models
{
    public class TrainingLog
    {
        public string Id { get; set; }
        public string StudentUid { get; set; }
        public string Date { get; set; }
        public int Hours { get; set; }
        public string Description { get; set; }
        public bool IsApproved { get; set; }
    }
}
