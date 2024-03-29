﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace OpenAlprWebhookProcessor.Data
{
    public class PlateGroup
    {
        public Guid Id { get; set; }

        public string OpenAlprUuid { get; set; }

        public int OpenAlprCameraId { get; set; }

        public double OpenAlprProcessingTimeMs { get; set; }

        public bool IsAlert { get; set; }

        public string AlertDescription { get; set; }

        public long ReceivedOnEpoch { get; set; }

        public string BestNumber { get; set; }

        public List<PlateGroupPossibleNumbers> PossibleNumbers { get; set; }

        public PlateImage PlateImage { get; set; }

        public VehicleImage VehicleImage { get; set; }

        public double? AgentImageScrapeOccurredOn { get; set; }

        public double Confidence { get; set; }

        public string VehicleColor { get; set; }

        public string VehicleMake { get; set; }

        public string VehicleMakeModel { get; set; }

        public string VehicleType { get; set; }

        public string VehicleYear { get; set; }

        public string VehicleRegion { get; set; }

        public double Direction { get; set; }

        public string PlateCoordinates { get; set; }

        public string Notes { get; set; }

        public bool IsEnriched { get; set; }


        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PlateGroup>()
                .HasIndex(x => new { x.BestNumber, x.ReceivedOnEpoch });

            builder.Entity<PlateGroup>()
                .HasIndex(x => x.ReceivedOnEpoch);

            builder.Entity<PlateGroup>()
                .HasIndex(x => x.OpenAlprUuid);

            builder.Entity<PlateGroup>()
                .HasIndex(x => x.VehicleMake);

            builder.Entity<PlateGroup>()
                .HasIndex(x => x.VehicleMakeModel);

            builder.Entity<PlateGroup>()
                .HasIndex(x => x.VehicleColor);

            builder.Entity<PlateGroup>()
                .HasIndex(x => x.VehicleType);

            builder.Entity<PlateGroup>()
                .HasIndex(x => x.VehicleYear);

            builder.Entity<PlateGroup>()
                .HasIndex(x => x.VehicleRegion);
        }
    }
}
