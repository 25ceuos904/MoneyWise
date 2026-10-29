using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MoneyWise.Models;
using MoneyWise.Services;
using MoneyWise.ViewModels;

namespace MoneyWise.Controllers;

[AutoValidateAntiforgeryToken]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly MoneyJourneyService _journey;

    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, MoneyJourneyService journey)
    {
        _users = users;
        _signIn = signIn;
        _journey = journey;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.Name.Trim(),
            AgeGroup = model.AgeGroup,
            ExperienceLevel = model.Experience
        };
        _journey.StartFor(user);
        var result = await _users.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
        await _journey.EnsureScenarioAsync(user.Journey!, user.AgeGroup);
        await _signIn.SignInAsync(user, isPersistent: true);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _signIn.PasswordSignInAsync(model.Email, model.Password, true, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Email or password did not match.");
            return View(model);
        }
        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
