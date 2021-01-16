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
    [Route("[controller]")]
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

            SetTokenCookie(response.RefreshToken);

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

            SetTokenCookie(response.RefreshToken);

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

            return Ok(new { message = "Token revoked" });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
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

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
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
