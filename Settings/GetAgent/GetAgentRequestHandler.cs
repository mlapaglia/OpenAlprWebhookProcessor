using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetCameras
{
    public class GetAgentRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetAgentRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<Agent> HandleAsync()
        {
            var agent = await _processorContext.Agents.FirstOrDefaultAsync();

            return new Agent()
            {
                EndpointUrl = agent.EndpointUrl,
                Hostname = agent.Hostname,
                OpenAlprWebServerApiKey = agent.OpenAlprWebServerApiKey,
                OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                Uid = agent.Uid,
                Version = agent.Version,
            };
        }

        private static string CreateSampleImageUrl(
            string imageUuid,
            string cameraIpAddress)
        {
            if (!string.IsNullOrEmpty(imageUuid) || !string.IsNullOrEmpty(cameraIpAddress))
            {
                return null;
            }

            return Flurl.Url.Combine(cameraIpAddress, imageUuid);
        }
    }
}
