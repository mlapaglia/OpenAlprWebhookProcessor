using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public interface IAlertClient
    {
        Task<bool> ShouldSendAllPlatesAsync(CancellationToken cancellationToken = default);

        Task SendAlertAsync(
            AlertUpdateRequest alert,
            CancellationToken cancellationToken = default);

        Task VerifyCredentialsAsync(
            CancellationToken cancellationToken = default);
    }
}
