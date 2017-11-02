using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models;
using Beursspel.Models.BeursViewModels;
using Beursspel.Models.Beurzen;
using Beursspel.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Controllers
{
    public class BeurzenController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public BeurzenController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            var beurzen = await BeurzenManager.GetBeurzenAsync();
            Dictionary<int, int> aantallen;
            var userId = User.GetUserId();
            using (var db = new ApplicationDbContext())
            {
                aantallen = await db.Aandelen.Where(x => x.ApplicationUserId == userId)
                    .ToDictionaryAsync(x => x.BeursId, x => x.Aantal);
            }

            return View(new BeursLijstModel
            {
                Beurzen = beurzen,
                AandeelAantallen = aantallen
            });
        }

        private static async Task<ApplicationUser> GetGebruikerWithAandelen(HttpContext httpContext)
        {
            ApplicationUser gebruiker;
            using (var db = new ApplicationDbContext())
            {
                gebruiker = await db.Users.Include(x => x.Aandelen)
                    .SingleAsync(x => x.Id == httpContext.User.GetUserId());
            }
            return gebruiker;
        }

        public async Task<IActionResult> Beurs(string id)
        {
            var errors = TempData.Get<List<string>>("errors");
            if (errors != null)
            {
                var errorType = TempData.Get<string>("errorType");
                foreach (var error in errors)
                {
                    ModelState.AddModelError(errorType, error);
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            Beurs beurs;
            if (!int.TryParse(id, out var index))
            {
                beurs = await BeurzenManager.GetBeursMetNaamAsync(id);
            }
            else
            {
                beurs = await GetBeurs(index);
            }
            if (beurs == null)
            {
                return StatusCode(404);
            }
            if (!User.Identity.IsAuthenticated)
                return View(new BeursModel{Beurs = beurs, Aantal = 0});

            var aantal = 0;
            var gebruiker = await GetGebruikerWithAandelen(HttpContext);
            var ah = gebruiker.Aandelen?.FirstOrDefault(x =>
                x.ApplicationUserId == gebruiker.Id && x.BeursId == beurs.BeursId);
            if (ah != null)
                aantal = ah.Aantal;
            var geld = gebruiker.Geld;

            return View(new BeursModel{Beurs = beurs, Aantal = aantal, GebruikerGeld = geld});
        }

        private static async Task<Beurs> GetBeurs(int id)
        {
            return await BeurzenManager.GetBeursAsync(id);
        }

        [HttpPost]
        public async Task<IActionResult> KoopAandelen(BeursModel model)
        {
            if (GeplandeTelMomentenManager.IsMarktDicht)
            {
                return RedirectToAction("Beurs", new {id = model.Beurs.BeursId});
            }
            var gebruiker = await _userManager.GetCurrentUser(HttpContext);
            if (gebruiker == null)
            {
                throw new NullReferenceException("Gebruiker is null");
            }
            if (model.Beurs == null)
            {
                model.Beurs = await GetBeurs(model.BeursId);
            }

            var errors = new List<string>();

            var modelValue = model.Aantal * model.Beurs.AandeelPrijs;
            if (model.Aantal <= 0)
            {
                errors.Add("Je moet tenminste 1 aandeel kopen");
            }
            if (modelValue > gebruiker.Geld)
            {
                errors.Add("Je hebt hier niet genoeg geld voor");
            }

            if (errors.Count == 0)
            {
                await gebruiker.KoopAandelen(model.Beurs, model.Aantal);
            }
            TempData.Put("errors", errors);
            TempData.Put("errorType", "Koopfout");
            return RedirectToAction("Beurs", new {id = model.Beurs.BeursId});
        }

        [HttpPost]
        public async Task<IActionResult> VerkoopAandelen(BeursModel model)
        {
            if (GeplandeTelMomentenManager.IsMarktDicht)
            {
                return RedirectToAction("Beurs", new {id = model.Beurs.BeursId});
            }

            var gebruiker = await GetGebruikerWithAandelen(HttpContext);
            if (gebruiker == null)
            {
                throw new NullReferenceException("Gebruiker is null");
            }
            if (model.Beurs == null)
            {
                model.Beurs = await GetBeurs(model.BeursId);
            }

            var errors = new List<string>();
            if (gebruiker.Aandelen == null)
            {
                ModelState.AddModelError("Verkoopfout", "Je hebt hier niet genoeg aandelen in deze beurs voor");
                return RedirectToAction("Beurs", new {id = model.BeursId});
            }

            var aandeelHouder =
                gebruiker.Aandelen.FirstOrDefault(x => x.BeursId == model.BeursId && x.ApplicationUserId == gebruiker.Id);
            if (model.Aantal <= 0)
            {
                errors.Add("Je moet tenminste 1 aandeel verkopen");
            }
            if (aandeelHouder == null || aandeelHouder.Aantal < model.Aantal)
            {
                errors.Add("Je hebt hier niet genoeg aandelen in deze beurs voor");
            }

            if (errors.Count == 0)
            {
                await gebruiker.VerkoopAandelen(model.Beurs, model.Aantal);
            }
            TempData.Put("errors", errors);
            TempData.Put("errorType", "Verkoopfout");
            return RedirectToAction("Beurs", new {id = model.Beurs.BeursId});
        }
    }
}