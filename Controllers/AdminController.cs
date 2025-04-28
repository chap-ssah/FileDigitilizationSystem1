using FileDigitilizationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileDigitilizationSystem.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace FileDigitilizationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AdminController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Fetch users and map to view models
            var users = await _userManager.Users.ToListAsync();
            var userVMs = new List<UserViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userVMs.Add(new UserViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    IsActive = u.IsActive,
                    LastLogin = u.LastLogin,
                    FailedLoginAttempts = u.AccessFailedCount
                });
            }

            var model = new DashboardViewModel
            {
                ActiveUsers = userVMs.Count(u => u.IsActive),
                PendingRequests = 0, // TODO: implement actual pending requests count
                SecurityAlerts = userVMs.Sum(u => u.FailedLoginAttempts),
                RecentActivities = await GetRecentActivities(),
                Alerts = await GetSecurityAlerts(),
                Users = userVMs,
                PasswordPolicy = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$",
                PasswordPolicyText = "Minimum 8 characters with 1 uppercase, 1 lowercase, and 1 number"
            };

            return View(model);
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var list = new List<UserViewModel>();
            var users = await _userManager.Users.ToListAsync();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    IsActive = u.IsActive,
                    LastLogin = u.LastLogin,
                    FailedLoginAttempts = u.AccessFailedCount
                });
            }
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "Role created successfully.";
                    }
                    else
                    {
                        TempData["Error"] = "Failed to create role.";
                    }
                }
                else
                {
                    TempData["Warning"] = "Role already exists.";
                }
            }

            return RedirectToAction("Dashboard");
        }

        // GET: /Admin/Create
        public async Task<IActionResult> Create()
        {
            // Grab all roles from the database
            var roles = await _roleManager.Roles
                            .Select(r => new SelectListItem(r.Name, r.Name))
                            .ToListAsync();

            // Pass them via ViewBag (or ViewData)
            ViewBag.RoleList = roles;

            return View();
        }

        // POST: /Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate roles if you redisplay the form
                ViewBag.RoleList = await _roleManager.Roles
                                        .Select(r => new SelectListItem(r.Name, r.Name))
                                        .ToListAsync();
                return View(model);
            }

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction(nameof(Users));
            }

            AddErrors(result);

            // If we hit an error, re-populate roles so the dropdown still works
            ViewBag.RoleList = await _roleManager.Roles
                                    .Select(r => new SelectListItem(r.Name, r.Name))
                                    .ToListAsync();

            return View(model);
        }


        // GET: /Admin/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _roleManager.Roles
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToListAsync();
            ViewBag.RoleList = roles;

            var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
            var vm = new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = userRole,
                IsActive = user.IsActive
            };
            return View(vm);
        }

        // POST: /Admin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // re-populate roles on error
                ViewBag.RoleList = await _roleManager.Roles
                    .Select(r => new SelectListItem(r.Name, r.Name))
                    .ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // update fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;
            user.DeactivationDate = model.IsActive ? (DateTime?)null : DateTime.UtcNow;

            // update role if changed
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(Users));

            AddErrors(result);
            ViewBag.RoleList = await _roleManager.Roles
                .Select(r => new SelectListItem(r.Name, r.Name))
                .ToListAsync();
            return View(model);
        }

        // POST: /Admin/SendPasswordResetEmail/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPasswordResetEmail(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // avoid user‑enumeration
                return RedirectToAction(nameof(Users));
            }

            // 1) Generate reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // 2) Encode it for use in URL
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // 3) Build callback URL to Identity's ResetPassword page
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code, email = user.Email },
                protocol: Request.Scheme);

            // 4) Send the email
            await _emailSender.SendEmailAsync(
                user.Email,
                "Reset your password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            // Optionally display a success message in TempData
            TempData["StatusMessage"] = "Password reset email sent.";
            return RedirectToAction(nameof(Users));
        }


        // POST: /Admin/ToggleActivation/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivation(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            user.DeactivationDate = user.IsActive ? (DateTime?)null : DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                AddErrors(result);

            return RedirectToAction(nameof(Users));
        }

        private async Task<List<ActivityItem>> GetRecentActivities()
        {
            // Placeholder
            return new List<ActivityItem>();
        }

        private async Task<List<AlertItem>> GetSecurityAlerts()
        {
            // Placeholder
            return new List<AlertItem>();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
