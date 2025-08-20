using System;

namespace OpenAlprWebhookProcessor.Features.Ignores.Queries.GetIgnores
{
    public class IgnoreDto
    {
        public Guid Id { get; set; }

        public string PlateNumber { get; set; }

        public bool StrictMatch { get; set; }

        public string Description { get; set; }
    }
} 