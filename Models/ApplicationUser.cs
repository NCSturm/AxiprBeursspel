using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models.Beurzen;
using Beursspel.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string Naam { get; set; }
        private double _geld;

        public double Geld
        {
            get => _geld;
            set
            {
                _spelersGeld[Id] = value;
                _geld = value;
            }
        }

        public double Waarde { get; set; }

        public virtual List<AandeelHouder> Aandelen { get; set; }


        /// <summary>
        /// Cache voor geld, aangezien die waarde overal wordt gebruikt.
        /// </summary>
        private static ConcurrentDictionary<string, double> _spelersGeld =
            new ConcurrentDictionary<string, double>();

        public static void ResetCache()
        {
            _spelersGeld = new ConcurrentDictionary<string, double>();
        }

        public static async Task<double> GetCachedGeld(ClaimsPrincipal user, UserManager<ApplicationUser> userManager,
            HttpContext httpContext)
        {
            var userId = user.GetUserId();
            if (_spelersGeld.TryGetValue(userId, out var geld))
            {
                return geld;
            }
            geld = (await userManager.GetCurrentUser(httpContext)).Geld;
            _spelersGeld.TryAdd(userId, geld);
            return geld;
        }

        public ApplicationUser()
        {
            _spelersGeld.TryAdd(Id, Geld);
        }


        public async Task KoopAandelen(Beurs beurs, int aantal)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Attach(this);
                db.Attach(beurs);
                await db.Entry(this).Collection(x => x.Aandelen).LoadAsync();
                var kosten = beurs.AandeelPrijs * aantal;
                if (kosten > Geld)
                {
                    return;
                }
                Geld -= kosten;

                if (Aandelen == null)
                {
                    Aandelen = new List<AandeelHouder>();
                }
                var aandeelHouder = Aandelen.FirstOrDefault(
                    x => x.BeursId == beurs.BeursId && x.ApplicationUserId == Id
                );
                if (aandeelHouder == null)
                {
                    Aandelen.Add(new AandeelHouder
                    {
                        Aantal = aantal,
                        ApplicationUser = this,
                        ApplicationUserId = Id,
                        BeursId = beurs.BeursId
                    });
                }
                else
                {
                    aandeelHouder.Aantal += aantal;
                }
                beurs.BeschikbareAandelen -= aantal;
                await db.SaveChangesAsync();
            }
        }

        public async Task VerkoopAandelen(Beurs beurs, int aantal)
        {
            using (var db = new ApplicationDbContext())
            {
                db.Attach(this);
                db.Attach(beurs);
                await db.Entry(this).Collection(x => x.Aandelen).LoadAsync();
                var kosten = beurs.AandeelPrijs * aantal;
                Geld += kosten;

                var aandeelHouder = Aandelen.FirstOrDefault(
                    x => x.BeursId == beurs.BeursId && x.ApplicationUserId == Id
                );
                if (aandeelHouder != null)
                    aandeelHouder.Aantal -= aantal;
                beurs.BeschikbareAandelen += aantal;
                await db.SaveChangesAsync();
            }

        }
    }
}
