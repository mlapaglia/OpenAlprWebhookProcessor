import { ChangeDetectionStrategy, Component, inject, input, type OnDestroy, type OnInit } from '@angular/core';
import { SettingsService } from '../../settings.service';
import { SignalrService } from 'app/signalr/signalr.service';
import { AgentVideoStreams, type VideoStream } from '../videoStream';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { NgStyle } from '@angular/common';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { ImageModalComponent } from './image-modal/image-modal.component';
import type { Agent } from '../agent';

@Component({
  selector: 'app-agent-video-streams',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './agent-video-streams.component.html',
  styleUrls: ['./agent-video-streams.component.less'],
  imports:
  [
    MatCardModule, MatIconModule, NgStyle,
    MatProgressSpinnerModule, MatTableModule, MatButtonModule, MatTooltipModule,
  ],
})
export class AgentVideoStreamsComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly settingsService = inject(SettingsService);
  private readonly signalRHub = inject(SignalrService);
  private readonly dialog = inject(MatDialog);

  public readonly agent = input<Agent>();

  public agentVideoStreams?: AgentVideoStreams;
  public displayedColumns: string[] = ['cameraName', 'url', 'isStreaming', 'fps', 'totalPlateReads', 'lastPlateRead', 'lastUpdate', 'actions'];
  public mobileDisplayedColumns: string[] = ['cameraName', 'isStreaming', 'fps', 'totalPlateReads'];

  public isLoading = false;
  public expandedRow: unknown = null;

  ngOnInit(): void {
    this.getAgentVideoStreams();

    this.subscribeAndMarkForCheck(
      this.signalRHub.openAlprAgentConnectionStatusChanged,
      () => {
        this.getAgentVideoStreams();
      });
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private getAgentVideoStreams() {
    this.isLoading = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.settingsService.getAgentVideoStreams(),
      (result) => {
        this.agentVideoStreams = result;
        if ((this.agentVideoStreams.videoStreams as VideoStream[] | undefined)?.length) {
          this.agentVideoStreams.videoStreams.forEach((stream: unknown) => {
            const s = stream as {
              lastPlateRead?: number;
              lastUpdate?: number;
              formattedLastPlateRead?: string;
              formattedLastUpdate?: string;
            };
            s.formattedLastPlateRead = this.formatEpochTime(s.lastPlateRead ?? 0);
            s.formattedLastUpdate = this.formatEpochTime(s.lastUpdate ?? 0);
          });
        }
        this.isLoading = false;
      },
      () => {
        this.isLoading = false;
        this.agentVideoStreams = new AgentVideoStreams();
        this.agentVideoStreams.isConnected = false;
      });
  }

  public formatEpochTime(epochMs: number): string {
    if (!epochMs || epochMs === 0) {
      return 'Never';
    }
    return new Date(epochMs).toLocaleString();
  }

  public expandRow(row: unknown): void {
    this.expandedRow = this.expandedRow === row ? null : row;
    this.markForCheck();
  }

  public onRowKeyDown(event: KeyboardEvent, row: unknown): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.expandRow(row);
    }
  }

  public get statusText(): string {
    if (this.isLoading) {
      return 'Loading...';
    }
    if (this.agentVideoStreams?.isConnected) {
      return `Connected (${this.agentVideoStreams.videoStreams.length} streams)`;
    }
    return 'Disconnected';
  }

  public get hasConnectedStreams(): boolean {
    return !!(this.agentVideoStreams &&
              this.agentVideoStreams.isConnected &&
              this.agentVideoStreams.videoStreams.length > 0 &&
              !this.isLoading);
  }

  public get hasNoStreams(): boolean {
    return !!(this.agentVideoStreams &&
              this.agentVideoStreams.isConnected &&
              this.agentVideoStreams.videoStreams.length === 0 &&
              !this.isLoading);
  }
}
