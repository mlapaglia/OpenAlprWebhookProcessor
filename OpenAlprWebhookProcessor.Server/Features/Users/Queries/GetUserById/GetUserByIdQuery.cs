using MediatR;
using OpenAlprWebhookProcessor.Features.Users.Data;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<User>
    {
        public int Id { get; set; }

        public GetUserByIdQuery(int id)
        {
            Id = id;
        }
    }
} 