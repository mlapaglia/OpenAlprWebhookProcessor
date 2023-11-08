using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.WebhookProcessor.OpenAlprWebsocket
{
    public class AgentStatusResponse
    {
        [JsonPropertyName("agent_epoch_ms")]
        public long AgentEpochMs { get; set; }

        [JsonPropertyName("agent_log")]
        public string AgentLog { get; set; }

        [JsonPropertyName("agent_status")]
        public AgentStatus AgentStatus { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    public class AgentStatus
    {
        [JsonPropertyName("agent_hostname")]
        public string AgentHostname { get; set; }

        [JsonPropertyName("agent_type")]
        public string AgentType { get; set; }

        [JsonPropertyName("agent_uid")]
        public string AgentUid { get; set; }

        [JsonPropertyName("agent_version")]
        public string AgentVersion { get; set; }

        [JsonPropertyName("alprd_active")]
        public bool AlprdActive { get; set; }

        [JsonPropertyName("beanstalk_queue_size")]
        public int BealstalkQueueSize { get; set; }

        [JsonPropertyName("company_id")]
        public string CompanyId { get; set; }

        [JsonPropertyName("cpu_cores")]
        public int CpuCores { get; set; }

        [JsonPropertyName("cpu_last_update")]
        public long CpuLastUpdate { get; set; }

        [JsonPropertyName("cpu_usage_percent")]
        public int CpuUsagePercent { get; set; }

        [JsonPropertyName("daemon_uptime_seconds")]
        public int DaemonUptimeSeconds { get; set; }

        [JsonPropertyName("data_type")]
        public string DataType { get; set; }

        [JsonPropertyName("disk_drive_free_bytes")]
        public long DiskDriveFreeBytes { get; set; }

        [JsonPropertyName("disk_drive_is_writable")]
        public bool DiskDriveIsWritable { get; set; }

        [JsonPropertyName("disk_drive_total_bytes")]
        public long DiskDriveTotalBytes { get; set; }

        [JsonPropertyName("disk_quota_consumed_bytes")]
        public int DiskQuotaConsumedBytes { get; set; }

        [JsonPropertyName("disk_quota_earliest_result")]
        public int DiskQuotaEarliestResult { get; set; }

        [JsonPropertyName("disk_quota_total_bytes")]
        public long DiskQuotaTotalBytes { get; set; }

        [JsonPropertyName("license_key")]
        public string LicenseKey { get; set; }

        [JsonPropertyName("license_systemid")]
        public string LicenseSystemId { get; set; }

        [JsonPropertyName("license_valid")]
        public bool LicenseValid { get; set; }

        [JsonPropertyName("memory_consumed_bytes")]
        public long MemoryConsumedBytes { get; set; }

        [JsonPropertyName("memory_last_update")]
        public long MemoryLastUpdate { get; set; }

        [JsonPropertyName("memory_swaptotal_bytes")]
        public long MemorySwapTotalBytes { get; set; }

        [JsonPropertyName("memory_swapused_bytes")]
        public long MemorySwapUsedBytes { get; set; }

        [JsonPropertyName("memory_total_bytes")]
        public long MemoryTotalBytes { get; set; }

        [JsonPropertyName("openalpr_version")]
        public string OpenAlprVersion { get; set; }

        [JsonPropertyName("os")]
        public string Os { get; set; }

        [JsonPropertyName("processing_threads_active")]
        public int ProcessingThreadsActive { get; set; }

        [JsonPropertyName("processing_threads_configured")]
        public int ProcessingThreadsConfigured { get; set; }

        [JsonPropertyName("recording_enabled")]
        public bool RecordingEnabled { get; set; }

        [JsonPropertyName("system_uptime_seconds")]
        public int SystemUptimeSeconds { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("video_streams")]
        public List<VideoStream> VideoStreams { get; set; }
    }

    public class VideoStream
    {
        [JsonPropertyName("camera_id")]
        public int CameraId { get; set; }

        [JsonPropertyName("camera_name")]
        public string CameraName { get; set; }

        [JsonPropertyName("fps")]
        public int Fps { get; set; }

        [JsonPropertyName("is_streaming")]
        public bool IsStreaming  { get; set; }

        [JsonPropertyName("last_plate_read")]
        public int LastPlateRead { get; set; }

        [JsonPropertyName("last_update")]
        public long LastUpdate { get; set; }

        [JsonPropertyName("total_plate_reads")]
        public int TotalPlateReads { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
