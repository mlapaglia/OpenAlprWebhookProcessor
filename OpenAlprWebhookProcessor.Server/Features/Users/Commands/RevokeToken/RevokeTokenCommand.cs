using MediatR;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RevokeToken
{
    public class RevokeTokenCommand : IRequest<bool>
    {
        public string Token { get; set; } = default!;
        public string IpAddress { get; set; } = default!;

        public RevokeTokenCommand(string token, string ipAddress)
        {
            Token = token;
            IpAddress = ipAddress;
        }
    }
} 