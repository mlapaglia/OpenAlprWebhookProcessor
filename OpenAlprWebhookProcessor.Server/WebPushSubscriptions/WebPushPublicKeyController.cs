using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading;
using System.Threading.Tasks;
using OpenAlprWebhookProcessor.WebPushSubscriptions.VapidKeys;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions
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
