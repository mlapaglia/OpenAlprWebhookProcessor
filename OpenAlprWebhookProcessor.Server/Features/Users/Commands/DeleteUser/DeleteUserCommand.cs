using Mediator;

namespace OpenAlprWebhookProcessor.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : ICommand
    {
        public int Id { get; set; }

        public DeleteUserCommand(int id)
        {
            Id = id;
        }
    }
} 