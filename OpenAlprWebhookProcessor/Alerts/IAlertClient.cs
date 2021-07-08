using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public interface IAlertClient
    {
        Task SendAlertAsync(
            Alert alert,
            string base64PreviewJpeg,
            CancellationToken cancellationToken);

        Task VerifyCredentialsAsync(
            CancellationToken cancellationToken);
    }
}
