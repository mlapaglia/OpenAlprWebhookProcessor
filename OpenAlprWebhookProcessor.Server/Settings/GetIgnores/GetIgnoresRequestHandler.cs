using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetIgnores
{
    public class GetIgnoresRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetIgnoresRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<List<Ignore>> HandleAsync()
        {
            var ignores = new List<Ignore>();

            foreach (var dbIgnore in await _processorContext.Ignores.AsNoTracking().ToListAsync())
            {
                var ignore = new Ignore()
                {
                    Id = dbIgnore.Id,
                    PlateNumber = dbIgnore.PlateNumber,
                    StrictMatch = dbIgnore.IsStrictMatch,
                    Description = dbIgnore.Description,
                };

                ignores.Add(ignore);
            }

            return ignores;
        }
    }
}