using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data.Repositories
{
    public class AgentRepository : Repository<Agent>, IAgentRepository
    {
        public AgentRepository(ProcessorContext context) : base(context)
        {
        }

        public async Task<Agent?> GetFirstAgentAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(cancellationToken);
        }
    }
} 