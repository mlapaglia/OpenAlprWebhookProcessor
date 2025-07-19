using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Data.Repositories
{
    public interface IJwtKeyRepository : IRepository<JwtKey>
    {
        new Task<JwtKey> GetFirstAsync(CancellationToken cancellationToken = default);
    }
} 