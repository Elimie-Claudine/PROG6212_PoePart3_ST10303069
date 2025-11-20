using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimsApp.Data;
using MonthlyClaimsApp.Models;
using System;

namespace MonthlyClaimsApp.Controllers
{
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "HR") return RedirectToAction("Login", "Account");

            return View(_context.Lecturer.ToList());
        }

        // Add Lecturer GET
        public IActionResult Create()
        {
            return View();
        }

        // Add Lecturer POST
        [HttpPost]
        public IActionResult Create(Lecturer lecturer)
        {
            if (!ModelState.IsValid) return View(lecturer);

            _context.Lecturer.Add(lecturer);
            _context.SaveChanges();

            TempData["Message"] = "Lecturer added successfully!";
            return RedirectToAction("Index");
        }
    }
}