using Microsoft.AspNetCore.Mvc;
using MonthlyClaimsApp.Models;
using System.IO;

namespace MonthlyClaimsApp.Controllers
{
    public class LecturerController : Controller
    {
        private static List<Claim> _claims => LecturerControllerHelper.GetClaims();

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
            {
                ModelState.AddModelError(nameof(claim.HoursWorked), "Hours worked must be greater than zero.");
            }

            if (claim.HourlyRate <= 0)
            {
                ModelState.AddModelError(nameof(claim.HourlyRate), "Hourly rate must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                return View(claim);
            }

            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            claim.ClaimID = _claims.Count + 1;
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

                // store a virtual path so Download link in views works
                claim.DocumentName = "/uploads/" + uniqueFileName;
            }
            else if (!string.IsNullOrWhiteSpace(claim.Description))
            {
                claim.DocumentName = "";
            }
            else
            {
                TempData["ErrorMessage"] = "Please upload a supporting document or add a description.";
                return View(claim);
            }

            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrWhiteSpace(username))
            {
                if (string.IsNullOrWhiteSpace(claim.LecturerName))
                    claim.LecturerName = username;

                claim.SubmittedBy = username;
            }

            _claims.Add(claim);

            TempData["Message"] = "Claim submitted successfully!";
            return RedirectToAction("TrackClaims");
        }

        public IActionResult TrackClaims()
        {
            // Show only claims for the logged in lecturer (if possible)
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrWhiteSpace(username))
            {
                // If no login info, show everything (fallback)
                return View(_claims);
            }

            var myClaims = _claims.Where(c =>
                string.Equals(c.LecturerName, username, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.SubmittedBy, username, StringComparison.OrdinalIgnoreCase)
                || (c.LecturerID != null && c.LecturerID.ToString() == username) // fallback
            ).ToList();

            // If the user has no own claims, return empty list to view
            return View(myClaims);
        }

        [HttpPost]
        public IActionResult UploadDocument(int claimId, IFormFile file)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimID == claimId);
            if (claim != null && file != null && file.Length > 0)
            {
                // Save file
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
                ViewBag.Message = $"Document '{file.FileName}' uploaded for claim #{claimId}";
            }
            return View("TrackClaims", _claims);
        }
    }
}
