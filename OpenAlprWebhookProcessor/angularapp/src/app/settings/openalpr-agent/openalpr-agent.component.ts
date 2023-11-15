import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { SettingsService } from '../settings.service';
import { Agent } from './agent';
import { AgentStatus } from './agentStatus';
import { PlateStatisticsData } from 'app/plates/plate/plateStatistics';
import { SignalrService } from 'app/signalr/signalr.service';
import { Subscription } from 'rxjs';
import { MatTable, MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { NgIf, NgStyle } from '@angular/common';

@Component({
    selector: 'app-openalpr-agent',
    templateUrl: './openalpr-agent.component.html',
    styleUrls: ['./openalpr-agent.component.less'],
    standalone: true,
    imports: [NgIf, MatCardModule, MatIconModule, NgStyle, MatProgressSpinnerModule, MatTableModule, MatButtonModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule, MatTooltipModule, MatCheckboxModule]
})
export class OpenalprAgentComponent implements OnInit, OnDestroy {
  @ViewChild('agentStatusTable') table: MatTable<any>;

  public agent: Agent;
  public agentStatus: AgentStatus;
  public agentStatusData: PlateStatisticsData[] = [];
  public displayedColumns: string[] = ['key', 'value'];

  public isSaving: boolean = false;
  public isHydrating: boolean = false;
  public isLoadingAgentStatus: boolean = false;

  private eventSubscriptions = new Subscription();
  
  constructor(
    private settingsService: SettingsService,
    private snackBarService: SnackbarService,
    private signalRHub: SignalrService) { }

  ngOnInit(): void {
    this.getAgent();
    this.getAgentStatus();

    this.eventSubscriptions.add(this.signalRHub.openAlprAgentConnectionStatusChanged.subscribe((connected: boolean) => {
      this.getAgentStatus();
    }));
  }

  ngOnDestroy(): void {
    this.eventSubscriptions.unsubscribe();
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
    this.isLoadingAgentStatus = true;
    this.settingsService.getAgentStatus().subscribe(result => {
      this.agentStatus = result;
      this.agentStatusData = new Array<PlateStatisticsData>();

      if (this.agentStatus.isConnected) {
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
      } else {
        this.agentStatusData.push({
          key: "Last Heartbeat",
          value: (new Date(this.agent.lastHeartbeatEpochMs)).toString(),
        });
      }

      this.isLoadingAgentStatus = false;
      this.table.renderRows();
      
    },
    (error) => {
      this.isLoadingAgentStatus = false;
      this.agentStatus = new AgentStatus();
      this.agentStatus.isConnected = false;
      this.agentStatusData = new Array<PlateStatisticsData>();
      this.table.renderRows();
    });
  }

  public enableAgent() {
    this.settingsService.enableAgent(this.agent.id).subscribe(() => {
      this.getAgentStatus();
    });
  }

  public disableAgent() {
    this.settingsService.disableAgent(this.agent.id).subscribe(() => {
      this.getAgentStatus();
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
