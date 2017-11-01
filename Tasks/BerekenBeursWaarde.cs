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
        public bool Enabled => true;
        public async Task ExecuteAsync()
        {
            await Berekeningen.VraagAanbod.BerekenBeurzen();
        }
    }
}