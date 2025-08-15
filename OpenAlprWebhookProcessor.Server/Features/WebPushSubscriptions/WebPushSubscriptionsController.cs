using Lib.Net.Http.WebPush;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.AddWebPushSubscription;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Commands.DeleteWebPushSubscription;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebPushSubscriptionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WebPushSubscriptionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PushSubscription subscription, CancellationToken cancellationToken)
        {
            var command = new AddWebPushSubscriptionCommand(subscription);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpDelete("{endpoint}")]
        public async Task<ActionResult> Delete(string endpoint, CancellationToken cancellationToken)
        {
            var command = new DeleteWebPushSubscriptionCommand(endpoint);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
    }
} 