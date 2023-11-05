using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace OpenAlprWebhookProcessor.PushSubscriptions
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicKeyController : ControllerBase
    {
        private readonly PushNotificationsOptions _options;

        public PublicKeyController(IOptions<PushNotificationsOptions> options)
        {
            _options = options.Value;
        }

        public ContentResult Get()
        {
            return Content(_options.PublicKey, "text/plain");
        }
    }
}
