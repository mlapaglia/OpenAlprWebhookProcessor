using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.Server.Data;
using OpenAlprWebhookProcessor.Server.WebPushSubscriptions.VapidKeys;

namespace OpenAlprWebhookProcessor.Server.WebPushSubscriptions
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebPushPublicKeyController : ControllerBase
    {
        private readonly ProcessorContext _processorContext;

        public WebPushPublicKeyController(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<ContentResult> Get(CancellationToken cancellationToken)
        {
            var keys = await VapidKeyHelper.GetVapidKeysAsync(
                _processorContext,
                cancellationToken);

            return Content(keys.PublicKey, "text/plain");
        }
    }
}
