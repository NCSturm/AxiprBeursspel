using System.Threading.Tasks;

namespace Beursspel.Tasks
{
    public interface IRecurringTask
    {
        string Cron { get; }
        bool Enabled { get; }
        Task ExecuteAsync();
    }
}