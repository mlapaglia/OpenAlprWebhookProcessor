using MediatR;
using OpenAlprWebhookProcessor.Features.Users.Data;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<User>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;

        public CreateUserCommand(string firstName, string lastName, string username, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Password = password;
        }
    }
} 