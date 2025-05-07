using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FileDigitilizationSystem.Models;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    public AuthController(SignInManager<ApplicationUser> signIn,
                          UserManager<ApplicationUser> users)
    {
        _signIn = signIn;
        _users = users;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel m)
    {
        if (!ModelState.IsValid) return View(m);

        var res = await _signIn.PasswordSignInAsync(
            m.Email, m.Password, m.RememberMe, lockoutOnFailure: false);

        if (res.Succeeded)
        {
            var u = await _users.FindByEmailAsync(m.Email);
            if (await _users.IsInRoleAsync(u, "Admin"))
                return RedirectToAction("Dashboard", "Admin");
            if (await _users.IsInRoleAsync(u, "RecordsTeam"))
                return RedirectToAction("Dashboard", "Records");
            if (await _users.IsInRoleAsync(u, "Requester"))
                return RedirectToAction("Dashboard", "Requester");

            return RedirectToLocal(m.ReturnUrl);
        }

        ModelState.AddModelError("", "Invalid login.");
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Login", "Auth");
    }

    private IActionResult RedirectToLocal(string url)
        => Url.IsLocalUrl(url) ? Redirect(url) : RedirectToAction("Home", "Index");
}
