using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IdentityNetCore.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using IdentityNetCore.Models;
using System.Security.Claims;

namespace IdentityNetCore.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> rolemanager;

    public AccountController(ILogger<AccountController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> rolemanager, IEmailService emailService)
    {
        this.rolemanager = rolemanager;
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
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

        var isFirstUser = !await _userManager.Users.AnyAsync();

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

        if (isFirstUser)
        {
            const string adminRoleName = "Admin";

            if (!await rolemanager.RoleExistsAsync(adminRoleName))
            {
                await rolemanager.CreateAsync(new IdentityRole(adminRoleName));
            }

            await _userManager.AddToRoleAsync(user, adminRoleName);
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

        return RedirectToAction(nameof(Login));
    }
    public IActionResult Login()
    {
        return View(new Models.LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email != null)
        {
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            await _emailService.SendEmailAsync("no-reply@example.com", email, "Welcome", "Welcome to our application!");

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                foreach (var claim in info.Principal.Claims)
                {
                    await _userManager.AddClaimAsync(user, claim);
                }
                createResult = await _userManager.AddLoginAsync(user, info);
                if (createResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl ?? Url.Content("~/"));
                }
            }
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(Models.LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, lockoutOnFailure: false);

        if (result.RequiresTwoFactor)
        {
            return RedirectToAction(nameof(LoginWith2fa), new { model.RememberMe });
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError("Login", "Invalid login attempt.");
            return View(model);
        }

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    public IActionResult TwoFactor()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> LoginWith2fa(bool rememberMe)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var model = new LoginWith2faViewModel { RememberMe = rememberMe };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.TwoFactorCode!, model.RememberMe, rememberClient: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return View(model);
        }

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    public async Task<IActionResult> TwoFactorSetup()
    {
        var user = await _userManager.GetUserAsync(User);
        await _userManager.ResetAuthenticatorKeyAsync(user!);
        var token = await _userManager.GetAuthenticatorKeyAsync(user!);
        var qrCodeUri = $"otpauth://totp/IdentityNetCore:{user!.Email}?secret={token}&issuer=IdentityNetCore&digits=6";

        var model = new TwoFactorViewModel() { Token = token, QrCodeUri = qrCodeUri };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> TwoFactorSetup(TwoFactorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user!, _userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code!);

        if (!isValid)
        {
            ModelState.AddModelError("Code", "Invalid code.");
            return View(model);
        }

        await _userManager.SetTwoFactorEnabledAsync(user!, true);
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }
}