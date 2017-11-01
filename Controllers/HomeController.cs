using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Microsoft.AspNetCore.Mvc;
using Beursspel.Models;
using Beursspel.Models.AccountViewModels;
using Beursspel.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Controllers
{
    public class HomeController : Controller
    {
        private static string _adminRoleId;
        public async Task<IActionResult> Index()
        {
            List<TopScoreModel> users;
            var telMomenten = await GeplandeTelMomentenManager.GetGeplandeMomenten();
            using (var db = new ApplicationDbContext())
            {
                if (_adminRoleId == null)
                    _adminRoleId = (await db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == "ADMIN")).Id;
                users = await db.Users.Where(x => !db.UserRoles.Any(y => y.UserId == x.Id && y.RoleId != _adminRoleId))
                .OrderByDescending(x => x.Waarde).Take(10).Select(x => new TopScoreModel
                {
                    Naam = x.Naam,
                    Waarde = x.Waarde
                }).ToListAsync();
            }
            return View(new HomeModel{TopScores = users, TelMomenten = telMomenten});
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
