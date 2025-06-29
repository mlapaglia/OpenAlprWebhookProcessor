using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.Alerts
{
    public interface IAlertService
    {
        void AddJob(AlertUpdateRequest request);
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}