namespace OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentVideoStreams
{
    public class VideoStreamDto
    {
        public long CameraId { get; set; }

        public string CameraName { get; set; }

        public decimal Fps { get; set; }

        public bool IsStreaming { get; set; }

        public long LastPlateRead { get; set; }

        public long LastUpdate { get; set; }

        public decimal TotalPlateReads { get; set; }

        public string Url { get; set; }
    }
}