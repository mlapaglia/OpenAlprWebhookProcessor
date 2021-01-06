using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAlprWebhookProcessor.HeartbeatService
{
    public class OpenAlprHeatbeat
    {
        [JsonPropertyName("data_type")]
        public string DataType { get; set; }

        [JsonPropertyName("company_id")]
        public string CompanyId { get; set; }

        [JsonPropertyName("agent_uid")]
        public string AgentUid { get; set; }

        [JsonPropertyName("agent_version")]
        public string AgentVersion { get; set; }

        [JsonPropertyName("openalpr_version")]
        public string OpenAlprVersion { get; set; }

        [JsonPropertyName("agent_hostname")]
        public string AgentHostname { get; set; }

        [JsonPropertyName("os")]
        public string Os { get; set; }

        [JsonPropertyName("agent_type")]
        public string AgentType { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("system_uptime_seconds")]
        public long SystemUptimeSeconds { get; set; }

        [JsonPropertyName("daemon_uptime_seconds")]
        public long DaemonUptimeSeconds { get; set; }

        [JsonPropertyName("license_systemid")]
        public string LicenseSystemId { get; set; }

        [JsonPropertyName("license_key")]
        public string LicenseKey { get; set; }

        [JsonPropertyName("license_valid")]
        public bool LicenseValid { get; set; }

        [JsonPropertyName("recording_enabled")]
        public bool RecordingEnabled { get; set; }

        [JsonPropertyName("cpu_cores")]
        public int CpuCores { get; set; }

        [JsonPropertyName("cpu_last_update")]
        public long CpuLastUpdate { get; set; }

        [JsonPropertyName("cpu_usage_percent")]
        public double CpuUsagePercent { get; set; }

        [JsonPropertyName("disk_quota_total_bytes")]
        public long DiskQuotaTotalBytes { get; set; }

        [JsonPropertyName("disk_quota_consumed_bytes")]
        public int DiskQuotaConsumedBytes { get; set; }

        [JsonPropertyName("disk_quota_earliest_result")]
        public long DiskQuotaEarliestResult { get; set; }

        [JsonPropertyName("disk_drive_total_bytes")]
        public long DiskDriveTotalBytes { get; set; }

        [JsonPropertyName("disk_drive_free_bytes")]
        public long DiskDriveFreeBytes { get; set; }

        [JsonPropertyName("memory_consumed_bytes")]
        public long MemoryConsumedBytes { get; set; }

        [JsonPropertyName("memory_last_update")]
        public long MemoryLastUpdate { get; set; }

        [JsonPropertyName("memory_swapused_bytes")]
        public int MemorySwapUsedBytes { get; set; }

        [JsonPropertyName("memory_swaptotal_bytes")]
        public int MemorySwapTotalBytes { get; set; }

        [JsonPropertyName("memory_total_bytes")]
        public long MemoryTotalBytes { get; set; }

        [JsonPropertyName("processing_threads_active")]
        public int ProcessingThreadsActive { get; set; }

        [JsonPropertyName("processing_threads_configured")]
        public int ProcessingThreadsConfigured { get; set; }

        [JsonPropertyName("beanstalk_queue_size")]
        public int BeanstalkQueueSize { get; set; }

        [JsonPropertyName("video_streams")]
        public List<VideoStream> VideoStreams { get; set; }
    }


}
