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
                TempData["ErrorMessage"] = "Lecturer not found.";
                return RedirectToAction("Login", "Account");
            }

            // AUTO-ASSIGN LECTURER INFORMATION
            claim.LecturerID = lecturer.LecturerID;
            claim.LecturerName = lecturer.Name;

            if (claim.HoursWorked <= 0 || claim.HourlyRate <= 0)
            {
                TempData["ErrorMessage"] = "Hours and rate must be > 0.";
                return View(claim);
            }

            // Calculate total amount
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;
            claim.Status = "Pending";

            // FILE UPLOAD HANDLING
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
            return RedirectToAction("TrackClaims");
        }

        public IActionResult TrackClaims()
        {
            var lecturerId = HttpContext.Session.GetInt32("LecturerID");

            if (lecturerId == null)
            {
                TempData["ErrorMessage"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            var myClaims = _context.Claims
                .Where(c => c.LecturerID == lecturerId)
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
