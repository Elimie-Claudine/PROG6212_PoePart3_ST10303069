using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MonthlyClaimsApp.Data;
using MonthlyClaimsApp.Models;
using System.IO;

namespace MonthlyClaimsApp.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LecturerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SubmitClaim()
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");

            if (lecturerId == null)
            {
                TempData["ErrorMessage"] = "Lecturer not logged in.";
                return RedirectToAction("Login", "Account");
            }

            var lecturer = _context.Lecturer.FirstOrDefault(l => l.LecturerID == lecturerId);

            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "Lecturer not found. Contact HR.";
                return RedirectToAction("Login", "Account");
            }

            var claim = new Claim
            {
                LecturerID = lecturer.LecturerID,
                LecturerName = lecturer.Name
            };

            return View();
        }




        [HttpPost]
        public IActionResult SubmitClaim(Claim claim, IFormFile? file)
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");

            if (lecturerId == null)
            {
                TempData["ErrorMessage"] = "Lecturer not logged in.";
                return RedirectToAction("Login", "Account");
            }

            var lecturer = _context.Lecturer.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "Lecturer not found. Contact HR.";
                return RedirectToAction("Login", "Account");
            }

            // Assign lecturer info automatically
            claim.LecturerID = lecturer.LecturerID;
            claim.LecturerName = lecturer.Name;

            // Validate hours/rate and calculate total
            if (claim.HoursWorked <= 0 || claim.HourlyRate <= 0)
            {
                TempData["ErrorMessage"] = "Hours and rate must be > 0.";
                return View(claim);
            }

            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;
            claim.Status = "Pending";

            // Save document if uploaded
            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                claim.DocumentName = "/uploads/" + uniqueFileName;
            }

            _context.Claims.Add(claim);
            _context.SaveChanges();

            TempData["Message"] = "Claim submitted successfully!";
            return RedirectToAction("Index");
        }





        public IActionResult TrackClaims()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrWhiteSpace(username))
                return View(_context.Claims.ToList());

            var lecturer = _context.Lecturer.FirstOrDefault(l => l.Email == username || l.Name == username);
            if (lecturer == null)
            {
                TempData["ErrorMessage"] = "Lecturer not found.";
                return View(new List<Claim>());
            }

            var myClaims = _context.Claims
                .Where(c => c.LecturerID == lecturer.LecturerID)
                .ToList();

            return View(myClaims);
        }


        [HttpPost]
        public IActionResult UploadDocument(int claimId, IFormFile file)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.ClaimID == claimId);

            if (claim != null && file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                claim.DocumentName = "/uploads/" + uniqueFileName;

                _context.SaveChanges();
            }

            return RedirectToAction("TrackClaims");
        }
    }
}
