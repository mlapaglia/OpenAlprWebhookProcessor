using OpenAlprWebhookProcessor.Data.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Data.Repositories
{
    public interface IJwtKeyRepository : IRepository<JwtKey>
    {
        Task<JwtKey> GetFirstJwtKeyAsync(CancellationToken cancellationToken = default);
    }
} 