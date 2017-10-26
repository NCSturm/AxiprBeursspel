using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models;
using Beursspel.Models.AdminViewModels;
using Beursspel.Models.Beurzen;
using Beursspel.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Gebruikers()
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

        public async Task<IActionResult> Gebruiker(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = await db.Users.Include(x => x.Aandelen).SingleOrDefaultAsync(x => x.Id == id);
                if (user == null)
                    return RedirectToAction("Gebruikers");
                var userBeursIds = user.Aandelen.Select(x => x.BeursId);
                var beursnamen = await db.Beurzen.Where(x => userBeursIds.Any(y => y == x.BeursId))
                    .ToDictionaryAsync(x => x.BeursId, x => x.Naam);

                return View(new GebruikerModel
                {
                    User = user,
                    BeursNamen = beursnamen
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ModifyMoney(string id, string verschil)
        {
            if (!int.TryParse(verschil, out var num))
            {
                return BadRequest();
            }
            var user = await _userManager.FindByIdAsync(id);
            user.Geld += num;
            if (user.Geld < 0)
            {
                user.Geld = 0;
            }
            await _userManager.UpdateAsync(user);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ModifyStocks(string id, string beurs, string verschil)
        {
            if (!int.TryParse(verschil, out var num))
            {
                return BadRequest();
            }
            if (!int.TryParse(beurs, out var beursId))
            {
                return BadRequest();
            }
            var beursobj = await BeurzenManager.GetBeursAsync(beursId);
            using (var db = new ApplicationDbContext())
            {
                var user = await db.Users.Include(x => x.Aandelen).SingleOrDefaultAsync(x => x.Id == id);
                if (user.Aandelen == null)
                {
                    user.Aandelen = new List<AandeelHouder>();
                }
                var aandeelHouder = user.Aandelen.SingleOrDefault(
                    x => x.BeursId == beursId && x.ApplicationUserId == user.Id);
                if (aandeelHouder == null && num > 0)
                {
                    user.Aandelen.Add(new AandeelHouder
                    {
                        Aantal = num,
                        ApplicationUser = user,
                        ApplicationUserId = user.Id,
                        BeursId = beursId
                    });
                }
                else if (aandeelHouder != null)
                {
                    aandeelHouder.Aantal += num;
                    if (aandeelHouder.Aantal < 0)
                        aandeelHouder.Aantal = 0;
                }
                await db.SaveChangesAsync();

            }
            return Ok();
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

        public async Task<IActionResult> Beurzen()
        {
            var beurzen = await BeurzenManager.GetBeurzenAsync();
            return View(beurzen);
        }

        public async Task<IActionResult> Beurs(string id)
        {
            if (id == null)
            {
                return View(null);
            }
            if (!int.TryParse(id, out var i))
            {
                return View(null);
            }
            var beurs = await BeurzenManager.GetBeursAsync(i);
            return View(beurs);
        }

        [HttpPost]
        public async Task<IActionResult> Beurs(Beurs beurs)
        {
            var id = beurs.BeursId;
            if (beurs.BeursId == 0)
            {
                id = await CreateBeurs(beurs);
            }
            else
            {
                await ModifyBeurs(beurs);
            }
            return RedirectToAction("Beurs", new {id = id});
        }

        private static async Task ModifyBeurs(Beurs beurs)
        {
            using (var db = new ApplicationDbContext())
            {
                var vorigeBeurs = await db.Beurzen.Include(x => x.OudeWaardes)
                    .FirstOrDefaultAsync(x => x.BeursId == beurs.BeursId);
                beurs.OudeWaardes = vorigeBeurs.OudeWaardes;

                vorigeBeurs = beurs;
                await db.SaveChangesAsync();
            }
            await BeurzenManager.ModifyBeurs(beurs);
        }

        private static async Task<int> CreateBeurs(Beurs beurs)
        {
            beurs.OudeWaardes = new List<BeursWaardes>();
            using (var db = new ApplicationDbContext())
            {
                await db.Beurzen.AddAsync(beurs);
                await db.SaveChangesAsync();
            }
            await BeurzenManager.AddBeurs(beurs);
            return beurs.BeursId;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBeurs(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if (!int.TryParse(id, out var i))
            {
                return BadRequest();
            }
            using (var db = new ApplicationDbContext())
            {
                db.Beurzen.Remove(db.Beurzen.Find(i));
                await db.SaveChangesAsync();
            }
            BeurzenManager.DeleteBeurs(i);
            return Ok();
        }
    }
}