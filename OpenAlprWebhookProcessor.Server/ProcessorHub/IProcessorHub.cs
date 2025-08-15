using OpenAlprWebhookProcessor.Features.SystemLogs.Queries.GetLogs;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.ProcessorHub
{
    public interface IProcessorHub
    {
        Task LicensePlateRecorded(string plateNumber);

        Task LicensePlateAlerted(string plateNumber);

        Task ProcessInformationLogged(
            ApiLogLevel logLevel,
            string log);

        Task OpenAlprAgentConnected(
            string agentId,
            string ipAddress);

        Task OpenAlprAgentDisconnected(
            string agentId,
            string ipAddress);

        Task ScrapeFinished();

        Task DatabaseCleanupCompleted();
    }
}
