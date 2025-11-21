using Microsoft.AspNetCore.Mvc;
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
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim data is missing.";
                return View(claim);
            }

            if (claim.HoursWorked <= 0)
                ModelState.AddModelError(nameof(claim.HoursWorked), "Hours must be > 0.");

            if (claim.HourlyRate <= 0)
                ModelState.AddModelError(nameof(claim.HourlyRate), "Rate must be > 0.");

            if (!ModelState.IsValid)
                return View(claim);

            // Auto calculate amount
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            // Default status
            claim.Status = "Pending";

            // Handle file upload
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

            // Handle "SubmittedBy"
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrWhiteSpace(username))
            {
                if (string.IsNullOrWhiteSpace(claim.LecturerName))
                    claim.LecturerName = username;

                claim.SubmittedBy = username;
            }

            // SAVE TO DATABASE
            _context.Claims.Add(claim);
            _context.SaveChanges();

            TempData["Message"] = "Claim submitted successfully!";
            return RedirectToAction("TrackClaims");
        }

        public IActionResult TrackClaims()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrWhiteSpace(username))
                return View(_context.Claims.ToList());

            var myClaims = _context.Claims
                .Where(c =>
                    c.LecturerName == username ||
                    c.SubmittedBy == username ||
                    (c.LecturerID != null && c.LecturerID.ToString() == username)
                ).ToList();

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
