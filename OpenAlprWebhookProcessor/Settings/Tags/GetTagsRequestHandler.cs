using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.Tags
{
    public class GetTagsRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetTagsRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<Tag>> HandleAsync(CancellationToken cancellationToken)
        {
            var dbTags = await _processorContext.Tags.ToListAsync(cancellationToken);

            var tags = dbTags.Select(x => new Tag()
            {
                 Id = x.Id,
                 Name = x.Name, 
            }).ToList();

            return tags;
        }
    }
}
