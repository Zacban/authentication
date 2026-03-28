using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityNetCore.Controllers;

[Authorize(Policy = "RequireAdminRole")]
public class AdminController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public IActionResult Users()
    {
        return View();
    }

    public IActionResult Roles()
    {
        var roles = _roleManager.Roles.ToList();
        return View(roles);
    }

    public IActionResult Claims()
    {
        return View();
    }
}
