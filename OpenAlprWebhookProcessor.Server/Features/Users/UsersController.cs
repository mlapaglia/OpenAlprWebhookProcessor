using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Threading;
using Mediator;
using Microsoft.AspNetCore.Identity;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Text.Encodings.Web;
using OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using OpenAlprWebhookProcessor.Features.Users.Register;
using OpenAlprWebhookProcessor.Features.Users.Commands.EnableTwoFactor;

namespace OpenAlprWebhookProcessor.Features.Users
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly UrlEncoder _urlEncoder;

        public UsersController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            UrlEncoder urlEncoder)
        {
            _mediator = mediator;
            _userManager = userManager;
            _urlEncoder = urlEncoder;
        }

        [AllowAnonymous]
        [HttpGet("canregister")]
        public async Task<bool> CanRegister(CancellationToken cancellationToken)
        {
            var query = new CanRegisterQuery();
            return await _mediator.Send(query, cancellationToken);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] RegisterModel model,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new CreateUserCommand(
                    model.FirstName,
                    model.LastName,
                    model.Username,
                    model.Password);

                await _mediator.Send(command, cancellationToken);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterModel model,
            CancellationToken cancellationToken)
        {
            try
            {
                var canRegisterQuery = new CanRegisterQuery();
                var canRegister = await _mediator.Send(canRegisterQuery, cancellationToken);

                if (!canRegister)
                {
                    return Forbid();
                }

                var command = new CreateUserCommand(model.FirstName, model.LastName, model.Username, model.Password);
                await _mediator.Send(command, cancellationToken);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var query = new GetAllUsersQuery();
            var users = await _mediator.Send(query, cancellationToken);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery(id);
            var user = await _mediator.Send(query, cancellationToken);
            if (user == null) return NotFound();

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(int id, CancellationToken cancellationToken)
        {
            var command = new DeleteUserCommand(id);
            await _mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdatedById(
            int id,
            [FromBody] UpdateModel updateModel,
            CancellationToken cancellationToken)
        {
            var command = new UpdateUserCommand(id, updateModel.FirstName, updateModel.LastName, updateModel.Username, updateModel.Password);
            await _mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpGet("{id}/twofactor/status")]
        public async Task<IActionResult> GetTwoFactorStatus(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            var hasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;

            return Ok(new
            {
                IsTwoFactorEnabled = isTwoFactorEnabled,
                HasAuthenticator = hasAuthenticator
            });
        }

        [HttpPost("{id}/twofactor/setup")]
        public async Task<IActionResult> SetupTwoFactor(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return BadRequest(new { message = "User not found" });

            await _userManager.ResetAuthenticatorKeyAsync(user);
            var key = await _userManager.GetAuthenticatorKeyAsync(user);

            var email = await _userManager.GetEmailAsync(user) ?? user.UserName;
            var qrCodeUri = GenerateQrCodeUri(email, key);

            return Ok(new
            {
                SharedKey = key,
                QrCodeUri = qrCodeUri
            });
        }

        [HttpPost("{id}/twofactor/enable")]
        public async Task<IActionResult> EnableTwoFactor(int id, [FromBody] EnableTwoFactorRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var isValidToken = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                request.Code);

            if (!isValidToken)
                return BadRequest(new { message = "Verification code is invalid" });

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return Ok(new 
            { 
                message = "Two-factor authentication has been enabled",
                RecoveryCodes = recoveryCodes 
            });
        }

        [HttpPost("{id}/twofactor/disable")]
        public async Task<IActionResult> DisableTwoFactor(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return BadRequest(new { message = "User not found" });

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            return Ok(new { message = "Two-factor authentication has been disabled" });
        }

        [HttpGet("{id}/twofactor/recovery-codes")]
        public async Task<IActionResult> GetRecoveryCodes(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return BadRequest(new { message = "User not found" });

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return Ok(new { RecoveryCodes = recoveryCodes });
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("OpenALPR Webhook Processor"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }


}
