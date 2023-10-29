import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { SignalrService } from 'app/signalr/signalr.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';
import { Subscription } from 'rxjs';
import { SystemLogsService } from './systemLogs.service';

@Component({
  selector: 'app-logs',
  templateUrl: './systemLogs.component.html',
  styleUrls: ['./systemLogs.component.less']
})
export class SystemLogsComponent implements OnInit, AfterViewInit, OnDestroy {
  public logMessages: string[];
  public logMessagesDisplay: string = '';
  public onlyFailedPlateGroups: boolean = false;
  public isPurging: boolean = false;

  private subscriptions = new Subscription();

  constructor(
    private signalRHub: SignalrService,
    private systemLogsService: SystemLogsService,
    private snackBarService: SnackbarService) { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.populateLogs();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public populateLogs() {
    this.systemLogsService.getLogs().subscribe(result => {
      this.logMessages = result;
      this.formatLogs();
      this.subscribeForLogs();
    })
  }
  public subscribeForLogs() {
    this.subscriptions.add(this.signalRHub.processInformationLogged.subscribe(logInformation => {
      this.logMessages.unshift(logInformation);
      this.formatLogs();
    }));
  }

  public downloadPlates() {
    this.systemLogsService.getPlateGroups(this.onlyFailedPlateGroups).subscribe(blob => {
      var objectUrl = URL.createObjectURL(blob);
      window.open(objectUrl);
    });
  }

  public deletePlates() {
    this.isPurging = true;
    this.systemLogsService.deletePlates().subscribe(_ => {
      this.snackBarService.create("Deleted debug plates successfully.", SnackBarType.Deleted);
      this.isPurging = false;
    },
    _ => {
      this.snackBarService.create("Failed to delete plates, check the logs.", SnackBarType.Error);
      this.isPurging = false;
    });
  }

  private formatLogs() {
    this.logMessages = this.logMessages.slice(0, 500);
    this.logMessagesDisplay = this.logMessages.join("\r\n");
  }
}
