import { AfterViewInit, Component, OnDestroy, inject } from '@angular/core'
import { SignalrService } from 'app/signalr/signalr.service'
import { SnackbarService } from 'app/snackbar/snackbar.service'
import { SnackBarType } from 'app/snackbar/snackbartype'
import { Subscription } from 'rxjs'
import { SystemLogsService } from './system-logs.service'
import { Highlight } from 'ngx-highlightjs'
import { ReactiveFormsModule, FormsModule } from '@angular/forms'
import { MatCheckboxModule } from '@angular/material/checkbox'
import { MatButtonModule } from '@angular/material/button'

@Component({
  selector: 'app-logs',
  templateUrl: './system-logs.component.html',
  styleUrls: ['./system-logs.component.less'],
  imports: [MatButtonModule, MatCheckboxModule, ReactiveFormsModule, FormsModule, Highlight],
})
export class SystemLogsComponent implements AfterViewInit, OnDestroy {
  private signalRHub = inject(SignalrService)
  private systemLogsService = inject(SystemLogsService)
  private snackBarService = inject(SnackbarService)

  public logMessages: string[]
  public logMessagesDisplay = ''
  public onlyFailedPlateGroups = false
  public isPurging = false

  private subscriptions = new Subscription()

  ngAfterViewInit(): void {
    this.populateLogs()
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe()
  }

  public populateLogs() {
    this.systemLogsService.getLogs().subscribe((result) => {
      this.logMessages = result
      this.formatLogs()
      this.subscribeForLogs()
    })
  }

  public subscribeForLogs() {
    this.subscriptions.add(this.signalRHub.processInformationLogged.subscribe((logInformation) => {
      this.logMessages.unshift(logInformation)
      this.formatLogs()
    }))
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

  private formatLogs() {
    this.logMessages = this.logMessages.slice(0, 500)
    this.logMessagesDisplay = this.logMessages.join('\r\n')
  }
}
