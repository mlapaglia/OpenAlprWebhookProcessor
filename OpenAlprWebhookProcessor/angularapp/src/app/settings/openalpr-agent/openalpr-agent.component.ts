import { Component, OnInit } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { SettingsService } from '../settings.service';
import { Agent } from './agent';
import { AgentStatus } from './agentStatus';
import { PlateStatisticsData } from 'app/plates/plate/plateStatistics';

@Component({
  selector: 'app-openalpr-agent',
  templateUrl: './openalpr-agent.component.html',
  styleUrls: ['./openalpr-agent.component.less']
})
export class OpenalprAgentComponent implements OnInit {
  public agent: Agent;
  public agentStatus: AgentStatus;
  public agentStatusData: PlateStatisticsData[] = [];
  public displayedColumns: string[] = ['key', 'value'];

  public isSaving: boolean = false;
  public isHydrating: boolean = false;

  constructor(
    private settingsService: SettingsService,
    private snackBarService: SnackbarService) { }

  ngOnInit(): void {
    this.getAgent();
    this.getAgentStatus();
  }

  public saveAgent() {
    this.isSaving = true;

    this.settingsService.upsertAgent(this.agent).subscribe(result => {
      this.isSaving = false;
      this.getAgent();
    });
  }

  public scrapeAgent() {
    this.isHydrating = true;

    this.settingsService.startAgentScrape().subscribe(_ => {
      this.isHydrating = false;
      this.snackBarService.create("Agent Scraping has begun, check system logs for progress", SnackBarType.Info);
    });
  }

  private getAgent() {
    this.settingsService.getAgent().subscribe(result => {
      this.agent = result;
    });
  }

  private getAgentStatus() {
    this.settingsService.getAgentStatus().subscribe(result => {
      this.agentStatus = result;

      this.agentStatusData.push({
        key: "Cpu Cores",
        value: this.agentStatus.cpuCores.toString(),
      });

      this.agentStatusData.push({
        key: "Cpu Usage",
        value: this.agentStatus.cpuUsagePercent.toString() + "%",
      });

      this.agentStatusData.push({
        key: "ALPR Daemon Active",
        value: this.agentStatus.alprdActive ? "Yes" : "No",
      });

      this.agentStatusData.push({
        key: "Daemon Uptime",
        value: this.agentStatus.daemonUptimeSeconds.toString() + " seconds",
      });

      this.agentStatusData.push({
        key: "Free Disk Space",
        value: this.formatBytes(this.agentStatus.diskFreeBytes),
      });

      this.agentStatusData.push({
        key: "Hostname",
        value: this.agentStatus.hostname,
      });

      this.agentStatusData.push({
        key: "Current Time",
        value: (new Date(this.agentStatus.agentEpochMs)).toString(),
      });

      this.agentStatusData.push({
        key: "Version",
        value: this.agentStatus.version,
      });
    },
    (error) => {
      this.agentStatus.isConnected = false;
    });
  }

  private formatBytes(bytes: number, decimals = 2) {
    if (!+bytes) return '0 Bytes'

    const k = 1024
    const dm = decimals < 0 ? 0 : decimals
    const sizes = ['Bytes', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB']

    const i = Math.floor(Math.log(bytes) / Math.log(k))

    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`
  }
}
