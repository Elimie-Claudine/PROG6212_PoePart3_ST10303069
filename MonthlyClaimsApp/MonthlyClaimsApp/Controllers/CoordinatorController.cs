using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimsApp.Data;
using MonthlyClaimsApp.Models;

namespace MonthlyClaimsApp.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinatorController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userRole))
                return RedirectToAction("SelectRole");

            ViewBag.UserRole = userRole;

            List<Claim> claims;

            if (userRole == "Academic Manager")
            {
                claims = await _context.Claims
                    .Where(c =>
                        c.Status.Contains("waiting for Academic Manager") ||
                        c.Status == "Pending Manager Approval")
                    .ToListAsync();
            }
            else
            {
                claims = await _context.Claims
                    .Where(c =>
                        c.Status == "Pending" ||
                        c.Status.Contains("Returned to Lecturer"))
                    .ToListAsync();
            }

            return View(claims);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyClaim(int id, string actionType, string rejectionReason)
        {
            var claim = await _context.Claims.FindAsync(id);

            if (claim == null)
                return RedirectToAction("Index");

            var userRole = HttpContext.Session.GetString("UserRole") ?? "Program Coordinator";
            var userName = HttpContext.Session.GetString("Username") ?? userRole;

            if (actionType == "approve")
            {
                if (userRole == "Academic Manager")
                    claim.Status = "Approved";
                else
                    claim.Status = "Approved by Coordinator, waiting for Academic Manager";
            }
            else
            {
                claim.Status = $"Rejected by {userRole}, returned to Lecturer";
                claim.RejectionReason = rejectionReason;
            }

            claim.VerifiedByRole = userRole;
            claim.VerifiedBy = userName;
            claim.VerifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
