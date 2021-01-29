using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetCameras
{
    public class UpsertAgentRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public UpsertAgentRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task HandleAsync(Agent agent)
        {
            var dbAgent = await _processorContext.Agents.FirstOrDefaultAsync();

            if (dbAgent == null)
            {
                dbAgent = new Data.Agent()
                {
                    EndpointUrl = agent.EndpointUrl,
                    Hostname = agent.Hostname,
                    OpenAlprWebServerApiKey = agent.OpenAlprWebServerApiKey,
                    OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                    Uid = agent.Uid,
                    Version = agent.Version,
                };

                _processorContext.Add(dbAgent);
            }
            else
            {
                dbAgent.EndpointUrl = agent.EndpointUrl;
                dbAgent.Hostname = agent.Hostname;
                dbAgent.OpenAlprWebServerApiKey = agent.OpenAlprWebServerApiKey;
                dbAgent.OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl;
                dbAgent.Uid = agent.Uid;
                dbAgent.Version = agent.Version;
            }

            await _processorContext.SaveChangesAsync();
        }
    }
}
