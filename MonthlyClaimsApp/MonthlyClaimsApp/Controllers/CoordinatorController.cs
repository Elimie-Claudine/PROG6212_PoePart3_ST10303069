using Microsoft.AspNetCore.Mvc;
using MonthlyClaimsApp.Models;

namespace MonthlyClaimsApp.Controllers
{
    public class CoordinatorController : Controller
    {
        private static List<Claim> _claims => LecturerControllerHelper.GetClaims();

        public IActionResult SelectRole()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetRole(string role)
        {
            HttpContext.Session.SetString("UserRole", role);
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole))
                return RedirectToAction("SelectRole");

            ViewBag.UserRole = userRole;

            List<Claim> toShow;

            if (userRole == "Academic Manager")
            {
                toShow = _claims.Where(c => c.Status.Contains("waiting for Academic Manager") || c.Status == "Pending Manager Approval").ToList();
            }
            else
            {
                toShow = _claims.Where(c => c.Status == "Pending" || c.Status.Contains("Returned to Lecturer")).ToList();
            }

            return View(toShow);
        }

        [HttpPost]
        public IActionResult VerifyClaim(int id, string actionType)
        {
            var claim = _claims.FirstOrDefault(c => c.ClaimID == id);
            if (claim != null)
            {
                var userRole = HttpContext.Session.GetString("UserRole") ?? "Program Coordinator";
                var userName = HttpContext.Session.GetString("Username") ?? userRole;

                if (actionType == "approve")
                {
                    if (userRole == "Academic Manager")
                    {
                        claim.Status = "Approved";
                        claim.VerifiedByRole = userRole;
                        claim.VerifiedBy = userName;
                        claim.VerifiedDate = DateTime.Now;
                    }
                    else
                    {
                        claim.Status = "Approved by Coordinator, waiting for Academic Manager";
                        claim.VerifiedByRole = userRole;
                        claim.VerifiedBy = userName;
                        claim.VerifiedDate = DateTime.Now;
                    }
                }
                else 
                {
                    claim.Status = $"Rejected by {userRole}, returned to Lecturer";
                    claim.VerifiedByRole = userRole;
                    claim.VerifiedBy = userName;
                    claim.VerifiedDate = DateTime.Now;
                }
            }

            return RedirectToAction("Index");
        }

    }

    public static class LecturerControllerHelper
    {
        private static List<Claim> _sharedClaims = new List<Claim>();
        public static List<Claim> GetClaims() => _sharedClaims;
    }
}
