using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Threading;
using MediatR;
using OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate;
using OpenAlprWebhookProcessor.Features.Users.Commands.RefreshToken;
using OpenAlprWebhookProcessor.Features.Users.Commands.RevokeToken;
using OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.UpdateUser;
using OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById;
using OpenAlprWebhookProcessor.Features.Users.Queries.CanRegister;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetRefreshTokens;
using OpenAlprWebhookProcessor.Features.Users.Register;

namespace OpenAlprWebhookProcessor.Features.Users
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(
            [FromBody] AuthenticateRequest model,
            CancellationToken cancellationToken)
        {
            var command = new AuthenticateCommand(model.Username, model.Password, GetIpAddress());
            var response = await _mediator.Send(command, cancellationToken);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            SetTokenCookie(response.RefreshToken, response.JwtToken);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var command = new RefreshTokenCommand(refreshToken, GetIpAddress());
            var response = await _mediator.Send(command, cancellationToken);

            if (response == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            SetTokenCookie(response.RefreshToken, response.JwtToken);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken(
            [FromBody] RevokeTokenRequest model,
            CancellationToken cancellationToken)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token is required" });
            }

            var command = new RevokeTokenCommand(token, GetIpAddress());
            var response = await _mediator.Send(command, cancellationToken);

            if (!response)
            {
                return NotFound(new { message = "Token not found" });
            }

            Response.Cookies.Delete("jwtToken");
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Token revoked" });
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
                var command = new CreateUserCommand(model.FirstName, model.LastName, model.Username, model.Password);
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

        [HttpGet("{id}/refresh-tokens")]
        public async Task<IActionResult> GetRefreshTokens(int id, CancellationToken cancellationToken)
        {
            var query = new GetRefreshTokensQuery(id);
            var refreshTokens = await _mediator.Send(query, cancellationToken);

            if (refreshTokens == null)
            {
                return NotFound();
            }

            return Ok(refreshTokens);
        }

        private void SetTokenCookie(
            string refreshToken,
            string authenticationToken)
        {
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken, refreshCookieOptions);

            var authenticateCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };
            Response.Cookies.Append("jwtToken", authenticationToken, authenticateCookieOptions);
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
