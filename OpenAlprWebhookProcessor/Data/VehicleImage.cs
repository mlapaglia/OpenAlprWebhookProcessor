using Microsoft.EntityFrameworkCore;
using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class VehicleImage
    {
        public Guid Id { get; set; }
        
        public Guid PlateGroupId { get; set; }

        public PlateGroup PlateGroup { get; set; }

        public byte[] Jpeg { get; set; }

        public bool IsCompressed { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PlateImage>()
                .HasIndex(x => x.Id);

            builder.Entity<PlateImage>()
                .HasIndex(x => x.PlateGroupId);
        }
    }
}
