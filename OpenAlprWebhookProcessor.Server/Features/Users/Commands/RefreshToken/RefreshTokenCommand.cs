using MediatR;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<AuthenticateResponse>
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