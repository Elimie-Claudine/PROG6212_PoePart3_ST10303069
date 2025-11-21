using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonthlyClaimsApp.Models
{
    public class Document
    {
        [Key]
        public int DocumentID { get; set; }

        [ForeignKey("Claim")]
        public int ClaimID { get; set; }
        public Claim? Claim { get; set; } 
        public string? FileName { get; set; }
        public string? FileType { get; set; }
    }
}
