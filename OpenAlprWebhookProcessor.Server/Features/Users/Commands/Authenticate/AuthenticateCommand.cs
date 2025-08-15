using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.Authenticate
{
    public class AuthenticateCommand : IQuery<UserDto>
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool RememberMe { get; set; }

        public AuthenticateCommand(string username, string password, bool rememberMe)
        {
            Username = username;
            Password = password;
            RememberMe = rememberMe;
        }
    }
}