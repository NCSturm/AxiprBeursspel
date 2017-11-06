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
        public static List<TopScoreModel> TopScoreCache;

        public async Task<IActionResult> Index()
        {
            var telMomenten = await GeplandeTelMomentenManager.GetGeplandeMomenten();
            if (TopScoreCache == null)
            {
                await CreateTopScoreCache();
            }
            var topscores = TopScoreCache.Take(10).ToList();
            return View(new HomeModel{TopScores = topscores, TelMomenten = telMomenten});
        }

        private async Task CreateTopScoreCache()
        {
            using (var db = new ApplicationDbContext())
            {
                if (_adminRoleId == null)
                    _adminRoleId = (await db.Roles.SingleOrDefaultAsync(x => x.NormalizedName == "ADMIN")).Id;
                TopScoreCache = await db.Users
                    .Where(x =>
                        db.UserRoles.Where(y => y.UserId == x.Id).All(y => y.RoleId != _adminRoleId)
                    )
                    .OrderByDescending(x => x.Waarde)
                    .Select(x => new TopScoreModel
                    {
                        Naam = x.Naam,
                        Waarde = x.Waarde
                    })
                    .ToListAsync();
            }
        }


        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Gesloten()
        {
            return View();
        }

        public async Task<IActionResult> Scores()
        {
            if (TopScoreCache == null)
            {
                await CreateTopScoreCache();
            }
            return View(TopScoreCache);
        }
    }
}
