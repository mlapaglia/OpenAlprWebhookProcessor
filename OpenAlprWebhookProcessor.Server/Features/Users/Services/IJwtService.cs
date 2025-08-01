using OpenAlprWebhookProcessor.Features.Users.Data;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.Users.Services
{
    public interface IJwtService
    {
        Task<string> GenerateJwtTokenAsync(User user, bool rememberMe = false, CancellationToken cancellationToken = default);
        Task<byte[]> GetJwtSecretKeyAsync(CancellationToken cancellationToken = default);
        RefreshToken GenerateRefreshToken(string ipAddress);
    }
} 