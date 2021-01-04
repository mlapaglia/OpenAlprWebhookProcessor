using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Data
{
    public class ProcessorContext : DbContext
    {
        public ProcessorContext(DbContextOptions<ProcessorContext> options)
            : base(options)
        {
        }

        public DbSet<PlateGroup> PlateGroups { get; set; }
    }
}
