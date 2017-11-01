using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models;
using Beursspel.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GeplandeTelMomentenController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var errors = TempData.Get<List<string>>("errors");
            if (errors != null)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError("", error);
                }
            }
            using (var db = new ApplicationDbContext())
            {
                var telMomenten = await GeplandeTelMomentenManager.GetGeplandeMomenten();
                return View(new GeplandeTelMomentenModel
                {
                    Geplande = telMomenten
                });
            }
        }

        public async Task<IActionResult> Opslaan(GeplandeTelMomentenModel model)
        {
            if (model.Nieuw != null)
            {
                var fouten = new List<string>();
                if (model.Nieuw.Tijd == default(DateTime))
                {
                    fouten.Add("Tijd mag niet leeg zijn");
                }
                if (string.IsNullOrWhiteSpace(model.Nieuw.Reden))
                {
                    fouten.Add("Reden mag niet leeg zijn");

                }
                if (fouten.Count > 0)
                {
                    TempData.Put("errors", fouten);
                    return RedirectToAction("Index");
                }
                await GeplandeTelMomentenManager.AddTelMoment(model.Nieuw);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Verwijder(string id)
        {
            if (!int.TryParse(id, out var intId))
            {
                return BadRequest();
            }
            await GeplandeTelMomentenManager.DeleteMoment(intId);
            return Ok();
        }
    }
}