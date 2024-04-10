using Intex_II.Models;
using Microsoft.AspNetCore.Authorization;

namespace Intex_II.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserAdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserAdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> UserAdmin()
    {
        var users = _userManager.Users.ToList();
        var userRolesViewModel = new List<UserRolesViewModel>();

        foreach (IdentityUser user in users)
        {
            var thisViewModel = new UserRolesViewModel();
            thisViewModel.UserId = user.Id;
            thisViewModel.Email = user.Email;
            thisViewModel.Roles = await GetUserRoles(user);
            userRolesViewModel.Add(thisViewModel);
        }
        ViewData["Roles"] = _roleManager.Roles.Select(r => r.Name).ToList();

        return View(userRolesViewModel);
    }

    private async Task<List<string>> GetUserRoles(IdentityUser user)
    {
        return new List<string>(await _userManager.GetRolesAsync(user));
    }
    [Authorize(Roles = "Admin")]

    [HttpPost]
    public async Task<IActionResult> UserAdmin(string userId, List<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var userRoles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.ToList();
        var selectedRoles = roles;

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!result.Succeeded)
        {
            // Handle the error
            return View();
        }

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded)
        {
            // Handle the error
            return View();
        }

        return RedirectToAction("UserAdmin");
    }
}
