using Mediator;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommand : ICommand
    {
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;

        public RegisterUserCommand(string username, string password, string firstName, string lastName)
        {
            Username = username;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
        }
    }
}