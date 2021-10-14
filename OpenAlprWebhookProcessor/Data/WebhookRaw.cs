using System;

namespace OpenAlprWebhookProcessor.Data
{
    public class PlateGroupRaw
    {
        public Guid Id { get; set; }

        public Guid? PlateGroupId { get; set; }

        public virtual PlateGroup? PlateGroup { get; set; }

        public string RawPlateGroup { get; set; }
    }
}
