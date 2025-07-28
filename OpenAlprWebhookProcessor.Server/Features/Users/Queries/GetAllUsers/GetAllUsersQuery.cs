using Mediator;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IQuery<List<User>>
    {
    }
} 