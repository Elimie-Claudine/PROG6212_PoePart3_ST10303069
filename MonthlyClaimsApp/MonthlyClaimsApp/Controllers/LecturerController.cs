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
            ViewBag.Lecturers = new SelectList(_context.Lecturer, "LecturerID", "Name");
            return View();
        }



        [HttpPost]
        public IActionResult SubmitClaim(Claim claim, IFormFile? file)
        {
            // 1️⃣ Basic null check
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim data is missing.";
                return RedirectToAction("SubmitClaim");
            }

            // 2️⃣ Validate HoursWorked and HourlyRate
            if (claim.HoursWorked <= 0)
                ModelState.AddModelError(nameof(claim.HoursWorked), "Hours must be > 0.");
            if (claim.HourlyRate <= 0)
                ModelState.AddModelError(nameof(claim.HourlyRate), "Rate must be > 0.");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors.";

                ViewBag.Lecturers = new SelectList(_context.Lecturer, "LecturerID", "Name");
                return View(claim);
            }

            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;
            claim.Status = "Pending";

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


            if (claim.LecturerID > 0)
            {
                var lecturer = _context.Lecturer.FirstOrDefault(l => l.LecturerID == claim.LecturerID);
                if (lecturer == null)
                {
                    TempData["ErrorMessage"] = "Lecturer not found. Contact HR.";
                    ViewBag.Lecturers = new SelectList(_context.Lecturer, "LecturerID", "Name");
                    return View(claim);
                }

                claim.LecturerName = lecturer.Name;
            }
            else
            {
                TempData["ErrorMessage"] = "Please select a lecturer.";
                ViewBag.Lecturers = new SelectList(_context.Lecturer, "LecturerID", "Name");
                return View(claim);
            }

            // 6️⃣ Save claim
            _context.Claims.Add(claim);
            _context.SaveChanges();

            TempData["Message"] = "Claim submitted successfully!";
            return RedirectToAction("SubmitClaim");
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
