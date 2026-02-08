import { ChangeDetectionStrategy, Component, inject, type AfterViewInit, type OnDestroy } from '@angular/core';

import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { SignalrService } from 'app/signalr/signalr.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { SystemLogsService, ApiLogLevel } from './system-logs.service';
import { Highlight } from 'ngx-highlightjs';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-logs',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './system-logs.component.html',
  styleUrls: ['./system-logs.component.less'],
  imports: [
    MatButtonModule,
    MatCheckboxModule,
    ReactiveFormsModule,
    FormsModule,
    Highlight,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule
],
})
export class SystemLogsComponent extends OnPushBaseComponent implements AfterViewInit, OnDestroy {
  private readonly signalRHub = inject(SignalrService);
  private readonly systemLogsService = inject(SystemLogsService);
  private readonly snackBarService = inject(SnackbarService);

  public logMessages: string[];
  public logMessagesDisplay = '';
  public onlyFailedPlateGroups = false;
  public isPurging = false;
  public selectedLogLevel = ApiLogLevel.Information;
  public searchControl = new FormControl('');

  public readonly ApiLogLevel = ApiLogLevel;
  public readonly logLevelOptions = [
    { value: ApiLogLevel.Verbose, label: 'Verbose' },
    { value: ApiLogLevel.Debug, label: 'Debug' },
    { value: ApiLogLevel.Information, label: 'Information' },
    { value: ApiLogLevel.Warning, label: 'Warning' },
    { value: ApiLogLevel.Error, label: 'Error' },
    { value: ApiLogLevel.Critical, label: 'Critical' },
  ];

  ngAfterViewInit(): void {
    setTimeout(() => this.populateLogs(), 0);
    this.setupSearchDebounce();
  }

  private setupSearchDebounce(): void {
    this.subscribeAndMarkForCheck(
      this.searchControl.valueChanges.pipe(
        debounceTime(150),
        distinctUntilChanged(),
      ),
      () => {
        this.populateLogs();
      },
    );
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  public populateLogs() {
    this.subscribeAndMarkForCheck(
      this.systemLogsService.getLogs(this.selectedLogLevel, this.searchControl.value ?? ''),
      (result) => {
        this.logMessages = result;
        this.formatLogs();
        this.subscribeForLogs();
      },
    );
  }

  public subscribeForLogs() {
    this.subscribeAndMarkForCheck(
      this.signalRHub.processInformationLogged,
      (logInformation) => {
        if (logInformation.logLevel >= this.selectedLogLevel && this.shouldIncludeLogBySearch(logInformation.logMessage)) {
          this.logMessages.unshift(logInformation.logMessage);
          this.formatLogs();
        }
      },
    );
  }

  public downloadPlates() {
    this.subscribeAndMarkForCheck(
      this.systemLogsService.getPlateGroups(this.onlyFailedPlateGroups),
      (blob) => {
        const objectUrl = URL.createObjectURL(blob);
        window.open(objectUrl);
      },
    );
  }

  public deletePlates() {
    this.isPurging = true;
    this.markForCheck();
    this.subscribeAndMarkForCheck(
      this.systemLogsService.deletePlates(),
      () => {
        this.snackBarService.create('Deleted debug plates successfully.', SnackBarType.Deleted);
        this.isPurging = false;
      },
      () => {
        this.snackBarService.create('Failed to delete plates, check the logs.', SnackBarType.Error);
        this.isPurging = false;
      },
    );
  }

  public onLogLevelChange() {
    this.populateLogs();
  }

  private shouldIncludeLogBySearch(logMessage: string): boolean {
    const searchText = this.searchControl.value ?? '';
    if (!searchText.trim()) {
      return true;
    }
    return logMessage.toLowerCase().includes(searchText.toLowerCase());
  }

  private formatLogs() {
    this.logMessages = this.logMessages.slice(0, 500);
    this.logMessagesDisplay = this.logMessages.join('\r\n');
  }
}
