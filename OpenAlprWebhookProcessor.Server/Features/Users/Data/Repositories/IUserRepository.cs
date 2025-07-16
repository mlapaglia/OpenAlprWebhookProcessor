using OpenAlprWebhookProcessor.Data.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllWithRefreshTokensAsync(CancellationToken cancellationToken = default);
        Task<User> GetByIdWithRefreshTokensAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    }
} 