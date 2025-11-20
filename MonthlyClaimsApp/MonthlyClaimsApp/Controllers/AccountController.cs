using Microsoft.AspNetCore.Mvc;
using MonthlyClaimsApp.Models;

namespace MonthlyClaimsApp.Controllers
{
    public class AccountController : Controller
    {
        // Temporary in-memory users. Later, you can fetch from DB.
        private static List<Users> _users = new List<Users>
        {
            new Users { UserID = 1, Username = "lecturer1", Password = "pass123", Role = "Lecturer" },
            new Users { UserID = 2, Username = "coordinator1", Password = "pass123", Role = "Program Coordinator" },
            new Users { UserID = 3, Username = "manager1", Password = "pass123", Role = "Academic Manager" },
            new Users { UserID = 4, Username = "hr1", Password = "pass123", Role = "HR" }
        };

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Invalid username or password";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
