using Microsoft.AspNetCore.Mvc;
using MonthlyClaimsApp.Models;

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

        // POST: Submit claim
        [HttpPost]
        public IActionResult SubmitClaim(Claim claim, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return View(claim);
            }

            claim.ClaimID = _claims.Count + 1;
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
            else if (!string.IsNullOrWhiteSpace(claim.Description))
            {
                claim.DocumentName = "";
            }
            else
            {
                TempData["ErrorMessage"] = "Please upload a supporting document or provide a description.";
                return View(claim);
            }

            _claims.Add(claim);
            TempData["Message"] = "Claim submitted successfully!";
            return RedirectToAction("TrackClaims");
        }

          //  return View("Index", claim);
        



        public IActionResult TrackClaims()
        {
            return View(_claims);
        }


[HttpPost]
        public IActionResult UploadDocument(int claimId, IFormFile file)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimID == claimId);
            if (claim != null && file != null)
            {
                claim.DocumentName = file.FileName;
                ViewBag.Message = $"Document '{file.FileName}' uploaded for claim #{claimId}";
            }
            return View("TrackClaims", _claims);
        }


    }
}
