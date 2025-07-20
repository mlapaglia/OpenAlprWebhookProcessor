using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public interface IAgentRepository : IRepository<Agent>
    {
        Task<Agent?> GetFirstAgentAsync(CancellationToken cancellationToken = default);

        Task<Agent> GetOrCreateAgentAsync(CancellationToken cancellationToken = default);
    }
} 