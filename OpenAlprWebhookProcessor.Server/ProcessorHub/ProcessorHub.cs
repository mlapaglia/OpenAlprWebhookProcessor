using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OpenAlprWebhookProcessor.ProcessorHub
{
    [Authorize]
    public class ProcessorHub : Hub<IProcessorHub>
    {
    }
}
