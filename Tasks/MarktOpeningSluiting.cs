using System.Threading.Tasks;
using Beursspel.Utilities;

namespace Beursspel.Tasks
{
    public class MarktSluiting : IRecurringTask
    {
        //Elke dag om 6 uur 's avonds
        public string Cron => "0 18 * * *";
        public bool Enabled => true;
        public async Task ExecuteAsync()
        {
            await GeplandeTelMomentenManager.CheckMarktSluiting();
        }
    }

    public class MarktOpening : IRecurringTask
    {
        //Elke dag om 4 uur 's nachts
        public string Cron => "0 4 * * *";
        public bool Enabled => true;
        public async Task ExecuteAsync()
        {
            await GeplandeTelMomentenManager.CheckMarktOpening();
        }
    }
}