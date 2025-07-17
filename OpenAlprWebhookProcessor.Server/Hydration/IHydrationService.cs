using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Hydrator
{
    public interface IHydrationService
    {
        void StartHydration(string name);
        Task ScheduleHydrationAsync(CancellationToken cancellationToken);
    }
} 