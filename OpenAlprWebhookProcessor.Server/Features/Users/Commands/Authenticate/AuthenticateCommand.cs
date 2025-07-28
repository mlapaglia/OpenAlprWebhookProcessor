using Mediator;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate
{
    public class AuthenticateCommand : IQuery<AuthenticateResponse>
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string IpAddress { get; set; } = default!;

        public AuthenticateCommand(string username, string password, string ipAddress)
        {
            Username = username;
            Password = password;
            IpAddress = ipAddress;
        }
    }
} 