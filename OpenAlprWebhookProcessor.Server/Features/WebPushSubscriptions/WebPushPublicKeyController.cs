using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.WebPushSubscriptions.Queries.GetWebPushPublicKey;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.WebPushSubscriptions
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebPushPublicKeyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WebPushPublicKeyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ContentResult> Get(CancellationToken cancellationToken)
        {
            var query = new GetWebPushPublicKeyQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Content(result, "text/plain");
        }
    }
} 