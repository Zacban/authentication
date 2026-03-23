using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IdentityNetCore.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IdentityNetCore.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;

    public AccountController(ILogger<AccountController> logger, UserManager<IdentityUser> userManager, IEmailService emailService)
    {
        _logger = logger;
        _userManager = userManager;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error!");
    }

    [HttpGet]
    public IActionResult Signup()
    {
        var model = new Models.SignupViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(Models.SignupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email!) is not null)
        {
            ModelState.AddModelError("Email", "Email is already in use.");
            return View(model);
        }

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        var confirmationLink = Url.ActionLink("ConfirmEmail", "Account", new { userId = user.Id, token = await _userManager.GenerateEmailConfirmationTokenAsync(user) }, Request.Scheme);
        await _emailService.SendEmailAsync("no-reply@example.com", model.Email!, "Confirm your email", $"Please confirm your email by clicking this link: {confirmationLink}");
        return RedirectToAction(nameof(Login));
    }
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return BadRequest();
        }

        return Ok();
    }
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Logout()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}