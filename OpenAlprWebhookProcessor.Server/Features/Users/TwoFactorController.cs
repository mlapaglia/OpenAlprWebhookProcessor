using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetTwoFactorStatus;
using OpenAlprWebhookProcessor.Features.Users.Commands.SetupTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Commands.EnableTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Commands.DisableTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Commands.GetRecoveryCodes;

namespace OpenAlprWebhookProcessor.Features.Users
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TwoFactorController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TwoFactorController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetTwoFactorStatusQuery(User);
                var response = await _mediator.Send(query, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("setup")]
        public async Task<IActionResult> Setup(CancellationToken cancellationToken)
        {
            try
            {
                var command = new SetupTwoFactorCommand(User);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("enable")]
        public async Task<IActionResult> Enable([FromBody] EnableTwoFactorRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new EnableTwoFactorCommand(User, request.Code);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("disable")]
        public async Task<IActionResult> Disable(CancellationToken cancellationToken)
        {
            try
            {
                var command = new DisableTwoFactorCommand(User);
                await _mediator.Send(command, cancellationToken);
                return Ok(new { message = "Two-factor authentication has been disabled" });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("recovery-codes")]
        public async Task<IActionResult> GetRecoveryCodes(CancellationToken cancellationToken)
        {
            try
            {
                var command = new GetRecoveryCodesCommand(User);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


    }
}