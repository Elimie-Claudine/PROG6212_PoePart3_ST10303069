using System.ComponentModel.DataAnnotations;

namespace MonthlyClaimsApp.Models
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
