import { AfterViewInit, Component, OnDestroy, inject } from '@angular/core'
import { CommonModule } from '@angular/common'
import { SignalrService } from 'app/signalr/signalr.service'
import { SnackbarService } from 'app/snackbar/snackbar.service'
import { SnackBarType } from 'app/snackbar/snackbartype'
import { Subscription } from 'rxjs'
import { SystemLogsService, ApiLogLevel } from './system-logs.service'
import { Highlight } from 'ngx-highlightjs'
import { ReactiveFormsModule, FormsModule } from '@angular/forms'
import { MatCheckboxModule } from '@angular/material/checkbox'
import { MatButtonModule } from '@angular/material/button'
import { MatSelectModule } from '@angular/material/select'
import { MatFormFieldModule } from '@angular/material/form-field'

@Component({
  selector: 'app-logs',
  templateUrl: './system-logs.component.html',
  styleUrls: ['./system-logs.component.less'],
  imports: [CommonModule, MatButtonModule, MatCheckboxModule, ReactiveFormsModule, FormsModule, Highlight, MatSelectModule, MatFormFieldModule],
})
export class SystemLogsComponent implements AfterViewInit, OnDestroy {
  private signalRHub = inject(SignalrService)
  private systemLogsService = inject(SystemLogsService)
  private snackBarService = inject(SnackbarService)

  public logMessages: string[]
  public logMessagesDisplay = ''
  public onlyFailedPlateGroups = false
  public isPurging = false
  public selectedLogLevel = ApiLogLevel.Information

  public readonly ApiLogLevel = ApiLogLevel
  public readonly logLevelOptions = [
    { value: ApiLogLevel.Verbose, label: 'Verbose' },
    { value: ApiLogLevel.Debug, label: 'Debug' },
    { value: ApiLogLevel.Information, label: 'Information' },
    { value: ApiLogLevel.Warning, label: 'Warning' },
    { value: ApiLogLevel.Error, label: 'Error' },
    { value: ApiLogLevel.Critical, label: 'Critical' },
  ]

  private subscriptions = new Subscription()

  ngAfterViewInit(): void {
    this.populateLogs()
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe()
  }

  public populateLogs() {
    this.unsubscribeFromLogs();
    this.systemLogsService.getLogs(this.selectedLogLevel).subscribe((result) => {
      this.logMessages = result
      this.formatLogs()
      this.subscribeForLogs()
    })
  }

  public subscribeForLogs() {
    this.subscriptions.add(this.signalRHub.processInformationLogged.subscribe((logInformation) => {
      if (logInformation.logLevel >= this.selectedLogLevel) {
        this.logMessages.unshift(logInformation.logMessage)
        this.formatLogs()
      }
    }))
  }

  public unsubscribeFromLogs() {
    this.subscriptions.unsubscribe()
    this.subscriptions = new Subscription()
  }

  public downloadPlates() {
    this.systemLogsService.getPlateGroups(this.onlyFailedPlateGroups).subscribe((blob) => {
      const objectUrl = URL.createObjectURL(blob)
      window.open(objectUrl)
    })
  }

  public deletePlates() {
    this.isPurging = true
    this.systemLogsService.deletePlates().subscribe(() => {
      this.snackBarService.create('Deleted debug plates successfully.', SnackBarType.Deleted)
      this.isPurging = false
    },
    () => {
      this.snackBarService.create('Failed to delete plates, check the logs.', SnackBarType.Error)
      this.isPurging = false
    })
  }

  public onLogLevelChange() {
    this.populateLogs()
  }

  private formatLogs() {
    this.logMessages = this.logMessages.slice(0, 500)
    this.logMessagesDisplay = this.logMessages.join('\r\n')
  }
}
