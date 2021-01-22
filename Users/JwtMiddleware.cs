using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Users
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger _logger;

        public JwtMiddleware(
            RequestDelegate next,
            ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(
            HttpContext context,
            IUserService userService)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?
                .Split(" ")
                .Last();

            if (token != null)
            {
                await AttachUserToContextAsync(
                    context,
                    userService,
                    token);
            }

            await _next(context);
        }

        private async Task AttachUserToContextAsync(
            HttpContext context,
            IUserService userService,
            string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = await userService.GetJwtSecretKeyAsync();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey    = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "unique_name").Value);

                context.Items["User"] = await userService.GetByIdAsync(userId, default);
            }
            catch
            {
                _logger.LogWarning("attempted login failed");
            }
        }
    }
}
