using Microsoft.AspNetCore.Mvc;
using MonthlyClaimsApp.Data;
using MonthlyClaimsApp.Models;

namespace MonthlyClaimsApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Save username + role
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role);

                // FIXED: Correct lookup for lecturer
                if (user.Role == "Lecturer")
                {
                    var lecturer = _context.Lecturer
                        .FirstOrDefault(l => l.Email == user.Username || l.Name == user.Username);

                    if (lecturer != null)
                    {
                        HttpContext.Session.SetInt32("LecturerID", lecturer.LecturerID);
                    }
                }

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
