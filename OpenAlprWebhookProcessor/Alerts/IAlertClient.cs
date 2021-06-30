using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Alerts
{
    public interface IAlertClient
    {
        Task SendAlertAsync(Alert alert);
    }
}
