using System;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models.Beurzen;
using Beursspel.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Beursspel.Berekeningen
{
    public static class VraagAanbod
    {
        public static async Task BerekenBeurzen()
        {
            using (var db = new ApplicationDbContext())
            {
                //Haal de beurzen uit de database, itereer over hen
                var beurzen = await db.Beurzen.Include(x => x.Waardes).ToListAsync();
                foreach (var beurs in beurzen)
                {
                    var huidigeAandelen = beurs.BeschikbareAandelen;
                    var vorigeAandelen = beurs.VorigeBeschikbareAandelen;
                    var verschil = huidigeAandelen - vorigeAandelen;

                    var huidigeWaarde = beurs.HuidigeWaarde;
                    var verschilProportie = verschil / Settings.StartBeursBeschikbareAandelen;
                    var nieuweWaarde = huidigeWaarde - (huidigeWaarde * 0.1f * verschilProportie);
                    beurs.Waardes.Add(new BeursWaardes
                    {
                        Beurs = beurs,
                        BeursId = beurs.BeursId,
                        Tijd = DateTime.Now,
                        Type = BeursWaardes.WaardeType.VraagAanbod,
                        Waarde = nieuweWaarde
                    });
                    db.Update(beurs);
                }
                await BeurzenManager.SetCache(beurzen);
                await db.SaveChangesAsync();
            }
        }
    }
}