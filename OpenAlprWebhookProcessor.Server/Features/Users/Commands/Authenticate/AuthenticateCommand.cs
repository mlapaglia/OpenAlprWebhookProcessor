using Mediator;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate
{
    public class AuthenticateCommand : IQuery<AuthenticateResponse>
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string IpAddress { get; set; } = default!;
        public bool RememberMe { get; set; }

        public AuthenticateCommand(string username, string password, string ipAddress, bool rememberMe = false)
        {
            Username = username;
            Password = password;
            IpAddress = ipAddress;
            RememberMe = rememberMe;
        }
    }
} 