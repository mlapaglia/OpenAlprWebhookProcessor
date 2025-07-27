using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Hydrator
{
    public interface IHydrationService
    {
        void StartHydration(string name);

        Task ScheduleHydrationAsync(CancellationToken cancellationToken = default);

        int GetPendingHydrationCount();

        IAsyncEnumerable<string> GetConsumingHydrationRequestsAsync(CancellationToken cancellationToken = default);
    }
}