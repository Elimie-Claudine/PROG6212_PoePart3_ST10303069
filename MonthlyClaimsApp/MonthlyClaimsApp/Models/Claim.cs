using System.ComponentModel.DataAnnotations;

namespace MonthlyClaimsApp.Models
{
    public class Claim
    {
        [Key]
        public int ClaimID { get; set; }
        public int LecturerID { get; set; }
        public string? LecturerName { get; set; }
        public string? DocumentName { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? VerifiedBy { get; set; }    
        public string? VerifiedByRole { get; set; }  
        public DateTime? VerifiedDate { get; set; }
        public string? SubmittedBy { get; set; }
        public string? AutoFlagReason { get; set; }
        public string? RejectionReason { get; set; }

    }
}
