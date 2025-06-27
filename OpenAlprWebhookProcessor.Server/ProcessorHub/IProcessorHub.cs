using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.ProcessorHub
{
    public interface IProcessorHub
    {
        Task LicensePlateRecorded(string plateNumber);

        Task LicensePlateAlerted(string plateNumber);

        Task ProcessInformationLogged(string log);

        Task OpenAlprAgentConnected(string agentId, string ipAddress);

        Task OpenAlprAgentDisconnected(string agentId, string ipAddress);

        Task ScrapeFinished();
    }
}
