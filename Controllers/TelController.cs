using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models.TelMomentModels;
using Beursspel.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TelMomentController : Controller
    {
        public async Task<IActionResult> Index()
        {
            using (var db = new ApplicationDbContext())
            {
                var ls = (await db.TelMomenten.ToListAsync()).OrderByDescending(x => x.Tijd).ToList();
                return View(ls);
            }
        }

        public async Task<IActionResult> Moment(string id)
        {
            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out var i))
            {
                var moment = new TelMomentHouder {Beurzen = new List<TelMomentModel>()};
                foreach (var beurs in await BeurzenManager.GetBeurzenAsync())
                {
                    moment.Beurzen.Add(new TelMomentModel
                    {
                        BeursId = beurs.BeursId,
                        BeursNaam = beurs.Naam,
                    });
                }
                return View(moment);
            }
            using (var db = new ApplicationDbContext())
            {
                var moment = await db.TelMomenten.Include(x => x.Beurzen)
                    .SingleAsync(x => x.TelMomentHouderId == i);
                return View(moment);
            }
        }

        public async Task<IActionResult> Opslaan(TelMomentHouder model)
        {
            if (model.Invoerder == null)
            {
                model.Invoerder = User.Identity.Name;
                model.Tijd = DateTime.Now;
                foreach (var telMomentModel in model.Beurzen)
                {
                    telMomentModel.Tijd = model.Tijd;
                }
                using (var db = new ApplicationDbContext())
                {
                    await db.TelMomenten.AddAsync(model);
                    await db.SaveChangesAsync();
                }
                await Berekeningen.Aanwezigheid.BerekenBeurzen(model);
            }
            else
            {
                using (var db = new ApplicationDbContext())
                {
                    var dbHouder = await db.TelMomenten.Include(x => x.Beurzen)
                        .SingleAsync(x => x.TelMomentHouderId == model.TelMomentHouderId);
                    dbHouder.LaatstBewerkt = DateTime.Now;
                    dbHouder.Bewerker = User.Identity.Name;
                    foreach (var telMomentModel in dbHouder.Beurzen)
                    {
                        var nieuweWaarde = model.Beurzen.FirstOrDefault(x => x.BeursId == telMomentModel.BeursId);
                        if (nieuweWaarde != null && telMomentModel.Aantal != nieuweWaarde.Aantal)
                        {
                            telMomentModel.Aantal = nieuweWaarde.Aantal;
                        }
                    }
                    await db.SaveChangesAsync();
                    await Berekeningen.Aanwezigheid.BerekenBeurzen(dbHouder);
                }
            }
            return RedirectToAction("Index");
        }
    }
}