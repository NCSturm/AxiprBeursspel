using System;
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

        public async Task<IActionResult> Index()
        {
            return View(new DashboardModel
            {
                StartBeursBeschikbareAandelen = Settings.StartBeursBeschikbareAandelen,
                StartBeursWaarde = Settings.StartBeursWaarde,
                StartSpelerGeld = Settings.StartSpelerGeld
            });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSettings(DashboardModel model)
        {
            Settings.StartBeursBeschikbareAandelen = model.StartBeursBeschikbareAandelen;
            Settings.StartBeursWaarde = model.StartBeursWaarde;
            Settings.StartSpelerGeld = model.StartSpelerGeld;
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> OpenBeursspel(string set)
        {
            if (!bool.TryParse(set, out var b))
            {
                return BadRequest();
            }
            Settings.IsOpen = !b;
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ResetBeursspel()
        {
            using (var db = new ApplicationDbContext())
            {
                foreach (var applicationUser in db.Users)
                {
                    applicationUser.Aandelen = new List<AandeelHouder>();
                    applicationUser.Geld = Settings.StartSpelerGeld;
                    ApplicationUser.ResetCache();
                }
                db.Database.ExecuteSqlCommand("DELETE FROM \"AandeelHouder\"");
                db.Database.ExecuteSqlCommand("DELETE FROM \"BeursWaardes\"");
                db.Database.ExecuteSqlCommand("DELETE FROM \"TelMomentModel\"");
                db.Database.ExecuteSqlCommand("DELETE FROM \"TelMomenten\"");
                await BeurzenManager.ClearCache();

                foreach (var beurs in db.Beurzen)
                {
                    beurs.Waardes = new List<BeursWaardes>
                    {
                        new BeursWaardes
                        {
                            Beurs = beurs,
                            BeursId = beurs.BeursId,
                            Tijd = DateTime.Now,
                            Type = BeursWaardes.WaardeType.Onbekend,
                            Waarde = Settings.StartBeursWaarde
                        }
                    };
                    beurs.BeschikbareAandelen = Settings.StartBeursBeschikbareAandelen;
                }

                await db.SaveChangesAsync();
            }
            var berekenSpelerWaardes = new Tasks.BerekenSpelerWaardes();
            await berekenSpelerWaardes.ExecuteAsync();
            return Ok();
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

        [HttpPost]
        public async Task<IActionResult> VerwijderGebruiker(string data)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = await db.Users.Include(x => x.Aandelen).SingleAsync(x => x.Id == data);
                if (user.Aandelen != null)
                {
                    foreach (var aandeelHouder in user.Aandelen)
                    {
                        var beurs = await db.Beurzen.FindAsync(aandeelHouder.BeursId);
                        beurs.BeschikbareAandelen += aandeelHouder.Aantal;
                    }
                    user.Aandelen = new List<AandeelHouder>();
                }
                db.Users.Remove(user);
                await db.SaveChangesAsync();
            }
            return Ok();
        }

        public async Task<IActionResult> Gebruiker(string id)
        {
            using (var db = new ApplicationDbContext())
            {
                var user = await db.Users.Include(x => x.Aandelen).SingleAsync(x => x.Id == id);
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
        public async Task<IActionResult> GrantAxipr(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.AddToRoleAsync(user, "Axipr");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAxipr(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.RemoveFromRoleAsync(user, "Axipr");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GrantDeelnemer(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.AddToRoleAsync(user, "Deelnemer");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveDeelnemer(string data)
        {
            var user = await _userManager.FindByIdAsync(data);
            await _userManager.RemoveFromRoleAsync(user, "Deelnemer");
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
            return View(new AdminBeursModel
            {
                Beurs = beurs,
                NieuweWaarde = beurs.HuidigeWaarde
            });
        }

        [HttpPost]
        public async Task<IActionResult> SetBeurs(AdminBeursModel data)
        {
            var id = data.Beurs.BeursId;
            if (data.Beurs.BeursId == 0)
            {
                id = await CreateBeurs(data.Beurs);
            }
            else
            {
                await ModifyBeurs(data);
            }
            return RedirectToAction("Beurs", new {id = id});
        }

        private static async Task ModifyBeurs(AdminBeursModel model)
        {
            var beurs = model.Beurs;
            using (var db = new ApplicationDbContext())
            {
                var dbBeurs = await db.Beurzen.Include(x => x.Waardes)
                    .FirstOrDefaultAsync(x => x.BeursId == beurs.BeursId);
                var laatste = dbBeurs.Waardes.LastOrDefault();
                if (laatste != null)
                {
                    laatste.Waarde = model.NieuweWaarde;
                    db.Update(laatste);
                }
                dbBeurs.AantalLeden = model.Beurs.AantalLeden;
                dbBeurs.BeschikbareAandelen = model.Beurs.BeschikbareAandelen;
                dbBeurs.Naam = model.Beurs.Naam;
                dbBeurs.Omschrijving = model.Beurs.Omschrijving;
                db.Update(dbBeurs);
                beurs = dbBeurs;

                await db.SaveChangesAsync();
            }
            await BeurzenManager.ModifyBeurs(beurs);
        }

        private static async Task<int> CreateBeurs(Beurs beurs)
        {
            beurs.Waardes = new List<BeursWaardes>
            {
                new BeursWaardes
                {
                    Beurs = beurs,
                    BeursId = beurs.BeursId,
                    Tijd = DateTime.Now,
                    Type = BeursWaardes.WaardeType.Onbekend,
                    Waarde = Settings.StartBeursWaarde
                }
            };
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
                db.Aandelen.RemoveRange(db.Aandelen.Where(x => x.BeursId == i));
                await db.SaveChangesAsync();
            }
            BeurzenManager.DeleteBeurs(i);
            return Ok();
        }
    }
}