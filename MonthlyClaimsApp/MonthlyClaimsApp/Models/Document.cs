namespace MonthlyClaimsApp.Models
{
    public class Document
    {
        public int DocumentID { get; set; }
        public int ClaimID { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
    }
}
