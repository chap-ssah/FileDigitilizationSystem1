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
            var users = await _userManager.Users.ToListAsync(); // Fetch all users first

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Now safely fetch roles
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? string.Empty,
                    IsActive = user.IsActive,
                });
            }

            var rolesList = await _roleManager.Roles
                .Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToListAsync();

            var viewModel = new DashboardViewModel
            {
                Users = userViewModels,
                Roles = rolesList
            };

            return View(viewModel);
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
                    FailedLoginAttempts = u.AccessFailedCount,
                    PhoneNumber = u.PhoneNumber,      
                    Address = u.Address,              
                    Gender = u.Gender                 
                });

            }
            return View(list);
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
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Gender = model.Gender,
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

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

       // GET: Admin/EditUser/id
public async Task<IActionResult> EditUser(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Gender = user.Gender,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };

            ViewBag.RoleList = new SelectList(_roleManager.Roles, "Name", "Name");  // Load roles for dropdown
            return View(model);
        }

        // POST: Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoleList = new SelectList(_roleManager.Roles, "Name", "Name");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.Gender = model.Gender;

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            await _userManager.UpdateAsync(user);

            return RedirectToAction("Users");
        }

        // POST: Admin/ToggleUserStatus/{id}
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive; // flip status
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Users");
        }









    }
}
