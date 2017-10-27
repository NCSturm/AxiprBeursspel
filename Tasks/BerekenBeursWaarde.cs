using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beursspel.Data;
using Beursspel.Models.Beurzen;
using Beursspel.Utilities;

namespace Beursspel.Tasks
{
    public class BerekenBeursWaarde : IRecurringTask
    {
        public string Cron => "*/5 * * * *";
        public bool Enabled => false;
        public async Task ExecuteAsync()
        {
            return;
            using (var db = new ApplicationDbContext())
            {
                var beurzen = await BeurzenManager.GetBeurzenAsync();
                foreach (var beurs in beurzen)
                {
                    db.Attach(beurs);

                    var oudeWaarde = beurs.HuidigeWaarde;
                    var nieuweWaarde = oudeWaarde + new Random().Next(-30, 30);
                    if (nieuweWaarde < 1)
                    {
                        nieuweWaarde = 1;
                    }
                    var dag = DateTime.Now;
                    var w = new BeursWaardes()
                    {
                        Beurs = beurs,
                        BeursId = beurs.BeursId,
                        Tijd = dag,
                        Waarde = nieuweWaarde,
                        Type = BeursWaardes.WaardeType.Onbekend
                    };
                    if (beurs.Waardes == null)
                        beurs.Waardes = new List<BeursWaardes>();
                    beurs.Waardes.Add(w);
                }
                await db.SaveChangesAsync();

            }
        }
    }
}