using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using OpenAlprWebhookProcessor.Users.Register;
using AutoMapper;
using System.Threading.Tasks;
using System.Threading;

namespace OpenAlprWebhookProcessor.Users
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        private readonly IMapper _mapper;

        public UsersController(
            IUserService userService,
            IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(
            [FromBody] AuthenticateRequest model,
            CancellationToken cancellationToken)
        {
            var response = await _userService.AuthenticateAsync(
                model,
                GetIpAddress(),
                cancellationToken);

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
            var response = await _userService.RefreshTokenAsync(
                refreshToken,
                GetIpAddress(),
                cancellationToken);

            if (response == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            SetTokenCookie(response.RefreshToken, response.JwtToken);

            return Ok(response);
        }

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

            var response = await _userService.RevokeTokenAsync(
                token,
                GetIpAddress(),
                cancellationToken);

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
            var currentUsers = await _userService.GetAllAsync(cancellationToken);

            return currentUsers.Count == 0;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] RegisterModel model,
            CancellationToken cancellationToken)
        {
            var user = _mapper.Map<User>(model);

            try
            {
                await _userService.CreateAsync(user, model.Password);
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
            var user = _mapper.Map<User>(model);

            try
            {
                var currentUsers = await _userService.GetAllAsync(cancellationToken);

                if (currentUsers.Count > 0)
                {
                    return Forbid();
                }

                await _userService.CreateAsync(user, model.Password);
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
            var users = await _userService.GetAllAsync(cancellationToken);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            if (user == null) return NotFound();

            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            await _userService.DeleteAsync(id);

            return Ok();
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdatedById(
            int id,
            [FromBody] UpdateModel updateModel,
            CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);
            await _userService.UpdateAsync(user, updateModel.Password);

            return Ok();
        }

        [HttpGet("{id}/refresh-tokens")]
        public async Task<IActionResult> GetRefreshTokens(int id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(
                id,
                cancellationToken);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.RefreshTokens);
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
