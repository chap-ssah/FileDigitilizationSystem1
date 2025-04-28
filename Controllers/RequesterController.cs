using System;
using System.Linq;
using System.Threading.Tasks;
using FileDigitilizationSystem.Data;
using FileDigitilizationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FileDigitilizationSystem.Controllers
{
    [Authorize(Roles = "Requester")]
    public class RequesterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequesterController(
          ApplicationDbContext context,
          UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var vm = new RequesterDashboardViewModel
            {
                UserName = User.Identity.Name,
                
                // pull your “missing‑file” requests:
                RecentRequests = await _context.FileRequests
                    .Where(r => r.RequesterId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                Notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.Timestamp)
                    .Take(5)
                    .ToListAsync(),
                // leave SearchResults null until someone searches
            };
            return View(vm);
        }

        // GET: Search
        public async Task<IActionResult> Search(string q, string suburb, bool digitized, bool physical)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.FileRecords.AsQueryable();
            if (!string.IsNullOrEmpty(q))
                query = query.Where(f => f.Reference.Contains(q));
            if (!string.IsNullOrEmpty(suburb))
                query = query.Where(f => f.Location == suburb);
            if (digitized) query = query.Where(f => f.IsDigital);
            if (physical) query = query.Where(f => !f.IsDigital);

            var results = await query.OrderByDescending(f => f.CreatedAt).ToListAsync();

            var vm = new RequesterDashboardViewModel
            {
                UserName = User.Identity.Name,
                SearchResults = results,
                NoResults = !results.Any(),
                SearchQuery = q,
                RecentRequests = await _context.FileRequests
                    .Where(r => r.RequesterId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                Notifications = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.Timestamp)
                    .Take(5)
                    .ToListAsync(),
            };
            return View("Dashboard", vm);
        }

        // GET: RequestMissing
        public IActionResult RequestMissing(string reference)
        {
            ViewBag.ApplicantTypes = new SelectList(new[] { "Individual", "Developer" });
            var vm = new RequestMissingViewModel { Reference = reference };
            return View(vm);
        }

        // POST: RequestMissing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestMissing(RequestMissingViewModel vm)
        {
            ViewBag.ApplicantTypes = new SelectList(new[] { "Individual", "Developer" }, vm.ApplicantType);

            if (!ModelState.IsValid)
                return View(vm);

            var fileReq = new FileRequest
            {
                Reference = vm.Reference,
                Suburb = vm.Suburb,
                ApplicantType = vm.ApplicantType,
                ApplicantName = vm.ApplicantName,
                Province = vm.Province,
                Notes = vm.Notes,
                RequesterId = _userManager.GetUserId(User),
                CreatedAt = DateTime.UtcNow
            };
            _context.FileRequests.Add(fileReq);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}
