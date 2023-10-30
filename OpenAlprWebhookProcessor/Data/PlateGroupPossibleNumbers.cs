using Microsoft.EntityFrameworkCore;
using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class PlateGroupPossibleNumbers
    {
        public Guid Id { get; set; }

        public Guid PlateGroupId { get; set; }

        public PlateGroup PlateGroup { get; set; }

        public string Number { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PlateGroupPossibleNumbers>()
                .HasIndex(x => new { x.PlateGroupId, x.Number });
        }
    }
}
