using Microsoft.AspNetCore.Mvc;
using MonthlyClaimsApp.Models;

namespace MonthlyClaimsApp.Controllers
{
    public class AccountController : Controller
    {
        public static List<Users> UsersList = new List<Users>
        {
            new Users { UserID = 1, Username = "lecturer1", Password = "pass123", Role = "Lecturer" },
            new Users { UserID = 2, Username = "lecturer2", Password = "pass123", Role = "Lecturer" },
            new Users { UserID = 3, Username = "coordinator1", Password = "pass123", Role = "Program Coordinator" },
            new Users { UserID = 4, Username = "manager1", Password = "pass123", Role = "Academic Manager" },
            new Users { UserID = 5, Username = "hr1", Password = "pass123", Role = "HR" }
        };

        public static void AddUser(Users user)
        {
            if (user == null) return;
            user.UserID = UsersList.Count > 0 ? UsersList.Max(u => u.UserID) + 1 : 1;
            UsersList.Add(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = UsersList.FirstOrDefault(u => u.Username == username && u.Password == password);
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
