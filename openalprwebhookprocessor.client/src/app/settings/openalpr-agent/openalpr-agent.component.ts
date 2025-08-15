import { ChangeDetectionStrategy, Component, viewChild, inject, type OnDestroy, type OnInit } from '@angular/core';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { SettingsService } from '../settings.service';
import type { Agent } from './agent';
import { AgentStatus } from './agentStatus';
import type { PlateStatisticsData } from 'app/plates/plate/plateStatistics';
import { SignalrService } from 'app/signalr/signalr.service';
import { MatTableModule, type MatTable } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { NgStyle } from '@angular/common';
import { AgentVideoStreamsComponent } from './agent-video-streams/agent-video-streams.component';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

@Component({
  selector: 'app-openalpr-agent',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './openalpr-agent.component.html',
  styleUrls: ['./openalpr-agent.component.less'],
  imports: [
    MatCardModule, MatIconModule, NgStyle, MatProgressSpinnerModule, MatTableModule,
    MatButtonModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule,
    MatTooltipModule, MatCheckboxModule, MatTabsModule, AgentVideoStreamsComponent,
    RefreshButtonComponent,
  ],
})
export class OpenalprAgentComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly settingsService = inject(SettingsService);
  private readonly snackBarService = inject(SnackbarService);
  private readonly signalRHub = inject(SignalrService);

  readonly table = viewChild<MatTable<PlateStatisticsData[]>>('agentStatusTable');

  public agent?: Agent;
  public agentStatus: AgentStatus;
  public agentStatusData: PlateStatisticsData[] = [];
  public displayedColumns: string[] = ['key', 'value'];

  public isSaving = false;
  public isDisabling = false;
  public isEnabling = false;
  public isHydrating = false;
  public isLoadingAgentStatus = false;

  ngOnInit(): void {
    this.getAgent();
    this.getAgentStatus();

    this.subscribeAndMarkForCheck(
      this.signalRHub.openAlprAgentConnectionStatusChanged,
      () => {
        this.getAgentStatus();
      },
    );
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  public saveAgent() {
    this.isSaving = true;
    this.markForCheck();

    if (this.agent) {
      this.subscribeAndMarkForCheck(
        this.settingsService.upsertAgent(this.agent),
        () => {
          this.isSaving = false;
          this.getAgent();
        },
      );
    }
  }

  public scrapeAgent() {
    this.isHydrating = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.settingsService.startAgentScrape(),
      () => {
        this.isHydrating = false;
        this.snackBarService.create('Agent Scraping has begun, check system logs for progress', SnackBarType.Info);
      },
    );
  }

  private getAgent() {
    this.subscribeAndMarkForCheck(
      this.settingsService.getAgent(),
      (result) => {
        this.agent = result;
      },
    );
  }

  private getAgentStatus() {
    this.isLoadingAgentStatus = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.settingsService.getAgentStatus(),
      (result) => {
        this.agentStatus = result;
        this.agentStatusData = [];

        if (this.agentStatus.isConnected) {
          this.buildConnectedAgentData();
        } else {
          this.buildDisconnectedAgentData();
        }

        this.finalizeAgentStatusUpdate();
      },
      () => {
        this.handleAgentStatusError();
      },
    );
  }

  private buildConnectedAgentData(): void {
    this.agentStatusData.push(
      {
        key: 'Cpu Cores',
        value: this.agentStatus.cpuCores.toString(),
      },
      {
        key: 'Cpu Usage',
        value: `${this.agentStatus.cpuUsagePercent.toString()}%`,
      },
      {
        key: 'ALPR Daemon Active',
        value: this.agentStatus.alprdActive ? 'Yes' : 'No',
      },
      {
        key: 'Daemon Uptime',
        value: `${this.agentStatus.daemonUptimeSeconds.toString()} seconds`,
      },
      {
        key: 'Free Disk Space',
        value: this.formatBytes(this.agentStatus.diskFreeBytes),
      },
      {
        key: 'Hostname',
        value: this.agentStatus.hostname,
      },
      {
        key: 'Current Time',
        value: new Date(this.agentStatus.agentEpochMs).toString(),
      },
      {
        key: 'Version',
        value: this.agentStatus.version,
      },
    );
  }

  private buildDisconnectedAgentData(): void {
    this.agentStatusData.push({
      key: 'Last Heartbeat',
      value: this.agent?.lastHeartbeatEpochMs
        ? new Date(this.agent.lastHeartbeatEpochMs).toString()
        : 'Unknown',
    });
  }

  private finalizeAgentStatusUpdate(): void {
    this.isLoadingAgentStatus = false;
    this.table()?.renderRows();
  }

  private handleAgentStatusError(): void {
    this.isLoadingAgentStatus = false;
    this.agentStatus = new AgentStatus();
    this.agentStatus.isConnected = false;
    this.agentStatusData = [];
    this.table()?.renderRows();
  }

  public enableAgent() {
    if (this.agent) {
      this.isEnabling = true;
      this.markForCheck();

      this.subscribeAndMarkForCheck(
        this.settingsService.enableAgent(this.agent.id),
        () => {
          this.isEnabling = false;
          this.getAgentStatus();
        },
        () => {
          this.isEnabling = false;
          this.snackBarService.create('Failed to enable agent', SnackBarType.Error);
        });
    }
  }

  public disableAgent() {
    if (this.agent) {
      this.isDisabling = true;
      this.markForCheck();

      this.subscribeAndMarkForCheck(
        this.settingsService.disableAgent(this.agent.id),
        () => {
          this.isDisabling = false;
          this.getAgentStatus();
        },
        () => {
          this.isDisabling = false;
          this.snackBarService.create('Failed to disable agent', SnackBarType.Error);
        });
    }
  }

  private formatBytes(bytes: number, decimals = 2) {
    if (!+bytes) return '0 Bytes';

    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
  }
}
