using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models;
using Beursspel.Models.AdminViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Beursspel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _userRoles;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> userRoles)
        {
            _userManager = userManager;
            _userRoles = userRoles;
        }

        public async Task<IActionResult> Users()
        {
            var model = _userManager.Users.AsEnumerable().Select(x => new UserListModel
            {
                UserId = x.Id,
                Naam = x.Naam,
                HuidigeWaarde = x.Waarde,
                Username = x.UserName,
                Rollen = (_userManager.GetRolesAsync(x).Result).ToList()
            }).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GrantAdmin(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.AddToRoleAsync(user, "Admin");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAdmin(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.RemoveFromRoleAsync(user, "Admin");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GrantTapper(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.AddToRoleAsync(user, "Tapper");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTapper(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.RemoveFromRoleAsync(user, "Tapper");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GrantBetaald(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.AddToRoleAsync(user, "Betaald");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBetaald(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.RemoveFromRoleAsync(user, "Betaald");
            return Ok();
        }
    }
}