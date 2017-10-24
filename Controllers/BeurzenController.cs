using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models;
using Beursspel.Models.BeursViewModels;
using Beursspel.Models.Beurzen;
using Beursspel.Utilities;
using Microsoft.AspNetCore.Authorization;
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
            return View(beurzen);
        }

        private async Task<ApplicationUser> GetGebruikerWithAandelen(HttpContext httpContext)
        {
            ApplicationUser gebruiker = null;
            using (var db = new ApplicationDbContext())
            {
                gebruiker = db.Users.Include(x => x.Aandelen).FirstOrDefault(x => x.Id == httpContext.User.GetUserId());
            }
            return gebruiker;
        }

        public async Task<IActionResult> Beurs(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }
            if (!int.TryParse(id, out var index))
            {
                return await BeursNaam(id);
            }
            var beurs = await GetBeurs(index);
            if (beurs == null)
            {
                return StatusCode(404);
            }

            int aantal = 0;
            var gebruiker = await GetGebruikerWithAandelen(HttpContext);
            var ah = gebruiker.Aandelen?.FirstOrDefault(x => x.ApplicationUserId == gebruiker.Id && x.BeursId == index);
            if (ah != null)
                aantal = ah.Aantal;

            return View(new BeursModel{Beurs = beurs, Aantal = aantal});
        }

        private static async Task<Beurs> GetBeurs(int id)
        {
            return await BeurzenManager.GetBeursAsync(id);
        }

        public async Task<IActionResult> BeursNaam(string naam)
        {
            Beurs beurs;
            using (var context = new ApplicationDbContext())
            {
                naam = naam.ToLowerInvariant();
                beurs = await (from x in context.Beurzen
                    where x.Naam.ToLower() == naam
                    select x).FirstOrDefaultAsync();
            }
            if (beurs == null)
            {
                return StatusCode(404);
            }
            return View("Beurs", new BeursModel{Beurs = beurs});
        }

        public async Task<IActionResult> KoopAandelen(BeursModel model)
        {
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

            var modelValue = model.Aantal * model.Beurs.HuidigeWaarde;
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
            else
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return RedirectToAction("Beurs", new {id = model.BeursId});
        }

        public async Task<IActionResult> VerkoopAandelen(BeursModel model)
        {
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
                ModelState.AddModelError(string.Empty, "Je hebt hier niet genoeg aandelen in deze beurs voor");
                return RedirectToAction("Beurs", new {id = model.BeursId});
            }

            var modelValue = model.Aantal * model.Beurs.HuidigeWaarde;
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
            else
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return RedirectToAction("Beurs", new {id = model.BeursId});

        }
    }
}