using Microsoft.EntityFrameworkCore;
using System.Linq;
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

        public async Task<Agent> GetOrCreateAgentAsync(CancellationToken cancellationToken = default)
        {
            var agent = await GetFirstAgentAsync(cancellationToken);
            
            if (agent == null)
            {
                agent = new Agent();
                await AddAsync(agent, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }

            return agent;
        }
    }
} 