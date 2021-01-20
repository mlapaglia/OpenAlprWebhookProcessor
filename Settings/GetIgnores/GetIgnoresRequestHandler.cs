using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

            foreach (var ignore in await _processorContext.Ignores.ToListAsync())
            {
                ignores.Add(new Ignore());
            }

            ignores.Add(new Ignore());

            return ignores;
        }
    }
}