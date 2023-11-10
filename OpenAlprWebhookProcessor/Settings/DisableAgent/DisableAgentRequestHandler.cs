using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    public class DisableAgentRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        private readonly WebsocketClientOrganizer _websocketClientOrganizer;

        public DisableAgentRequestHandler(
            ProcessorContext processorContext,
            WebsocketClientOrganizer websocketClientOrganizer)
        {
            _processorContext = processorContext;
            _websocketClientOrganizer = websocketClientOrganizer;
        }

        public async Task<bool> HandleAsync(string agentId, CancellationToken cancellationToken)
        {
            var agent = await _processorContext.Agents
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (agent == null)
            {
                return false;
            }

            await _websocketClientOrganizer.StopAsync(cancellationToken);

            return true;
        }
    }
}
