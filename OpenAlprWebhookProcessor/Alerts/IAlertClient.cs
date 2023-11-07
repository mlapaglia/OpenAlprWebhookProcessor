using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public interface IAlertClient
    {
        Task SendAlertAsync(
            Alert alert,
            byte[] plateJpeg,
            CancellationToken cancellationToken);

        Task VerifyCredentialsAsync(
            CancellationToken cancellationToken);
    }
}
