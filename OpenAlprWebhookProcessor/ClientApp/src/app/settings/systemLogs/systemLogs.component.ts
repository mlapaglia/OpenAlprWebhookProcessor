import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { SignalrService } from '@app/signalr/signalr.service';
import { Subscription } from 'rxjs';
import { SystemLogsService } from './systemLogs.service';

@Component({
  selector: 'app-logs',
  templateUrl: './systemLogs.component.html',
  styleUrls: ['./systemLogs.component.less']
})
export class SystemLogsComponent implements OnInit, AfterViewInit, OnDestroy {
  public logMessages: string = '';
  public onlyFailedPlateGroups: boolean = false;
  private subscriptions = new Subscription();

  constructor(
    private signalRHub: SignalrService,
    private systemLogsService: SystemLogsService) { }

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
    this.subscribeForLogs();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public subscribeForLogs() {
    this.subscriptions.add(this.signalRHub.processInformationLogged.subscribe(logInformation => {
        this.logMessages = logInformation + "\r\n" + this.logMessages;
    }));
  }

  public downloadPlates() {
    this.systemLogsService.getPlateGroups(this.onlyFailedPlateGroups).subscribe(blob => {
      var objectUrl = URL.createObjectURL(blob);
      window.open(objectUrl);
    });
  }
}
