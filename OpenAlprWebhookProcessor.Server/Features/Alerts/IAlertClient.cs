using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Alerts
{
    public interface IAlertClient
    {
        Task<bool> ShouldSendAllPlatesAsync(CancellationToken cancellationToken);

        Task SendAlertAsync(
            AlertUpdateRequest alert,
            CancellationToken cancellationToken);

        Task VerifyCredentialsAsync(
            CancellationToken cancellationToken);
    }
}
