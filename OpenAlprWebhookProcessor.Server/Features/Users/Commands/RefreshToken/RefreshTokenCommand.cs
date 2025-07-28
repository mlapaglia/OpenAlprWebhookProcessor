using Mediator;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RefreshToken
{
    public class RefreshTokenCommand : IQuery<AuthenticateResponse>
    {
        public string Token { get; set; } = default!;
        public string IpAddress { get; set; } = default!;

        public RefreshTokenCommand(string token, string ipAddress)
        {
            Token = token;
            IpAddress = ipAddress;
        }
    }
} 