using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimsApp.Data;
using MonthlyClaimsApp.Models;

namespace MonthlyClaimsApp.Controllers
{
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "HR") return RedirectToAction("Login", "Account");

            var lecturers = await _context.Lecturer.ToListAsync();
            return View(lecturers);
        }

        public IActionResult ClaimReport()
        {
            var approvedClaims = _context.Claims.Where(c => c.Status == "Approved").ToList();
            return View(approvedClaims);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateLecturer(Lecturer lecturer, string username, string password)
        {
            if (lecturer == null)
            {
                TempData["Message"] = "Invalid Lecturer data.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
                return View("Create", lecturer);

            if (_context.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View("Create", lecturer);
            }

            _context.Lecturer.Add(lecturer);
            _context.SaveChanges();

            var newUser = new Users
            {
                Username = string.IsNullOrWhiteSpace(username) ? lecturer.Email : username,
                Password = string.IsNullOrWhiteSpace(password) ? "changeme123" : password,
                Role = "Lecturer"
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();

            TempData["Message"] = "Lecturer added successfully!";
            return RedirectToAction("Index");
        }

    }
}
