namespace OpenAlprWebhookProcessor.Settings
{
    public class AgentStatus
    {
        public bool IsConnected { get; set; }

        public string Hostname { get; set; }

        public string Version { get; set; }

        public int CpuCores { get; set; }

        public decimal CpuUsagePercent { get; set; }

        public long DaemonUptimeSeconds { get; set; }

        public long DiskFreeBytes { get; set; }

        public long SystemUptimeSeconds { get; set; }

        public long AgentEpochMs { get; set; }

        public bool AlprdActive { get; set; }
    }
}
