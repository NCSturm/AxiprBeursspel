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
            using (var db = new ApplicationDbContext())
            {
                var beurzen = await BeurzenManager.GetBeurzenAsync();
                foreach (var beurs in beurzen)
                {
                    db.Attach(beurs);

                    var oudeWaarde = beurs.HuidigeWaarde;
                    var dag = DateTime.Now;
                    var w = new BeursWaardes()
                    {
                        Beurs = beurs,
                        BeursId = beurs.BeursId,
                        Tijd = dag,
                        Waarde = oudeWaarde
                    };
                    if (beurs.OudeWaardes == null)
                        beurs.OudeWaardes = new List<BeursWaardes>();
                    beurs.OudeWaardes.Add(w);
                    beurs.HuidigeWaarde = beurs.HuidigeWaarde += (new Random().Next(-30, 30));
                    if (beurs.HuidigeWaarde < 1)
                    {
                        beurs.HuidigeWaarde = 1;
                    }
                    if (beurs.OudeWaardes == null)
                        beurs.OudeWaardes = new List<BeursWaardes>();
                    beurs.OudeWaardes.Add(w);
                }
                await db.SaveChangesAsync();

            }
        }
    }
}