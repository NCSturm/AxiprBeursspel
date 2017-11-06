using System;
using System.Linq;
using System.Threading.Tasks;
using Beursspel.Controllers;
using Beursspel.Data;
using Beursspel.Models.Beurzen;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Tasks
{
    public class BerekenSpelerWaardes : IRecurringTask
    {
        //Elk uur
        public string Cron => "0 * * * *";
        public bool Enabled => true;
        public async Task ExecuteAsync()
        {
            using (var db = new ApplicationDbContext())
            {
                var beurzen = await db.Beurzen.Include(x => x.Waardes).ToListAsync();
                foreach (var beurs in beurzen)
                {
                    beurs.Waardes.Sort((x, y) => DateTime.Compare(x.Tijd, y.Tijd));
                }
                foreach (var applicationUser in db.Users.Include(x => x.Aandelen))
                {
                    var sum = (from aandeelHouder in applicationUser.Aandelen
                        let beurs = beurzen.FirstOrDefault(x => x.BeursId == aandeelHouder.BeursId)
                        where beurs != null
                        select aandeelHouder.Aantal * beurs.AandeelPrijs).Sum();

                    var waarde = applicationUser.Geld + sum;
                    applicationUser.Waarde = waarde;
                }
                await db.SaveChangesAsync();
            }
            HomeController.TopScoreCache = null;
        }
    }
}