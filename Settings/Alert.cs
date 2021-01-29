using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings
{
    public class Alert
    {
        public Guid Id { get; set; }

        public string PlateNumber { get; set; }

        public bool StrictMatch { get; set; }

        public string Description { get; set; }
    }
}
