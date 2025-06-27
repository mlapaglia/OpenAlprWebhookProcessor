using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Server.WebPushSubscriptions
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebPushSubscriptionsController : ControllerBase
    {
        private readonly IWebPushSubscriptionsService _pushSubscriptionsService;

        public WebPushSubscriptionsController(IWebPushSubscriptionsService pushSubscriptionsService)
        {
            _pushSubscriptionsService = pushSubscriptionsService;
        }

        [HttpPost]
        public async Task Post(
            [FromBody] PushSubscription subscription,
            CancellationToken cancellationToken)
        {
            await _pushSubscriptionsService.InsertAsync(
                subscription,
                cancellationToken);
        }

        [HttpDelete("{endpoint}")]
        public async Task Delete(
            string endpoint,
            CancellationToken cancellationToken)
        {
            await _pushSubscriptionsService.DeleteAsync(
                endpoint,
                cancellationToken);
        }
    }
}
