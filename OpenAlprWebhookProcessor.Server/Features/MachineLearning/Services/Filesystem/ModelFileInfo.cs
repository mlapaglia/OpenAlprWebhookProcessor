using System;

namespace OpenAlprWebhookProcessor.Features.MachineLearning.Services.Filesystem
{
    public class ModelFileInfo
    {
        public DateTime? LastModified { get; set; }
        public long FileSize { get; set; }
        public bool Exists { get; set; }
    }
}
