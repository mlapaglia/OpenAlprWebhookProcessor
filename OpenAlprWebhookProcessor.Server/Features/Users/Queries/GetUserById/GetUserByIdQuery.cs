using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Data;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IQuery<User>
    {
        public int Id { get; set; }

        public GetUserByIdQuery(int id)
        {
            Id = id;
        }
    }
} 