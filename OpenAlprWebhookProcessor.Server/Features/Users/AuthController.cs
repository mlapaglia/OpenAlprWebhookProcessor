using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate;
using OpenAlprWebhookProcessor.Features.Users.Commands.VerifyTwoFactor;
using OpenAlprWebhookProcessor.Features.Users.Commands.RegisterUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.Logout;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetCurrentUser;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using OpenAlprWebhookProcessor.Features.Users.Commands.RegisterPasskey;
using OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyRegistration;
using OpenAlprWebhookProcessor.Features.Users.Commands.AuthenticatePasskey;
using OpenAlprWebhookProcessor.Features.Users.Commands.CompletePasskeyAuthentication;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeletePasskey;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserPasskeys;

namespace OpenAlprWebhookProcessor.Features.Users
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new AuthenticateCommand(request.Username, request.Password, request.RememberMe);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] Verify2FARequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new VerifyTwoFactorCommand(request.UserId, request.Code, request.RememberMe);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if registration is allowed (no users exist)
                var canRegisterQuery = new CanRegisterQuery();
                var canRegister = await _mediator.Send(canRegisterQuery, cancellationToken);

                if (!canRegister)
                {
                    return BadRequest(new { message = "Registration is not allowed. A user already exists." });
                }

                var command = new RegisterUserCommand(request.Username, request.Password, request.FirstName, request.LastName);
                await _mediator.Send(command, cancellationToken);
                return Ok(new { message = "Registration successful" });
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var command = new LogoutCommand();
            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
        {
            var query = new GetCurrentUserQuery(User);
            var user = await _mediator.Send(query, cancellationToken);
            
            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }

        [HttpPost("passkey/register")]
        public async Task<IActionResult> RegisterPasskey([FromBody] RegisterPasskeyRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new RegisterPasskeyCommand(User, request.Name);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("passkey/complete-registration")]
        public async Task<IActionResult> CompletePasskeyRegistration([FromBody] CompletePasskeyRegistrationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new CompletePasskeyRegistrationCommand(User, request.AttestationResponse, request.Name);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("passkey/authenticate")]
        public async Task<IActionResult> AuthenticatePasskey([FromBody] AuthenticatePasskeyRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new AuthenticatePasskeyCommand(request.Username);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("passkey/complete-authentication")]
        public async Task<IActionResult> CompletePasskeyAuthentication([FromBody] CompletePasskeyAuthenticationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new CompletePasskeyAuthenticationCommand(request.Username, request.AssertionResponse, request.RememberMe);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("passkey/list")]
        public async Task<IActionResult> GetPasskeys(CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetUserPasskeysQuery(User);
                var response = await _mediator.Send(query, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("passkey/{passkeyId}")]
        public async Task<IActionResult> DeletePasskey(int passkeyId, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeletePasskeyCommand(User, passkeyId);
                var response = await _mediator.Send(command, cancellationToken);
                return Ok(response);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public record RegisterPasskeyRequest(string? Name = null);
    public record CompletePasskeyRegistrationRequest(string AttestationResponse, string? Name = null);
    public record AuthenticatePasskeyRequest(string Username);
    public record CompletePasskeyAuthenticationRequest(string Username, string AssertionResponse, bool RememberMe = false);
}