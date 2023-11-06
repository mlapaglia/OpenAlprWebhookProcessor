using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Mvc;

namespace OpenAlprWebhookProcessor.WebPushSubscriptions
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
        public void Post([FromBody] PushSubscription subscription)
        {
            _pushSubscriptionsService.Insert(subscription);
        }

        [HttpDelete("{endpoint}")]
        public void Delete(string endpoint)
        {
            _pushSubscriptionsService.Delete(endpoint);
        }
    }
}
