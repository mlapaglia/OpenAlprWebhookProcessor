export class AgentStatus {
    hostname: string;
    isConnected: boolean;
    version: string;
    cpuCores: number;
    cpuUsagePercent: number;
    daemonUptimeSeconds: number;
    diskFreeBytes: number;
    systemUptimeSeconds: number;
    agentEpochMs: number;
    alprdActive: boolean;

    constructor(init?:Partial<AgentStatus>) {
        Object.assign(this, init);
    }
}
