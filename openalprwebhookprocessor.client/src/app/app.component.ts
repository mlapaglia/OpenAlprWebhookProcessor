import { Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { AccountService } from './_services';
import type { User } from './_models';
import { SignalrService } from './signalr/signalr.service';
import { RouterLink, RouterOutlet } from '@angular/router';
import { SwUpdate, type VersionEvent } from '@angular/service-worker';
import { PushSubscriberService } from './_services/push-subscriber.service';
import { AlertComponent } from './_components/alert.component';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { CommonModule } from '@angular/common';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { ThemePickerComponent } from './theme-picker/theme-picker.component';
import { Subscription } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';

@Component({
  selector: 'app-app',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.css'],
  imports: [
    MatTabsModule, RouterLink, MatIconModule, AlertComponent, RouterOutlet,
    MatSidenavModule, MatListModule, MatDividerModule, CommonModule, ThemePickerComponent,
    MatButtonModule, MatMenuModule, MatToolbarModule,
  ],
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly signalRService = inject(SignalrService);
  private readonly accountService = inject(AccountService);
  private readonly swUpdate = inject(SwUpdate);
  private readonly pushSubscriberService = inject(PushSubscriberService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly breakpointObserver = inject(BreakpointObserver);

  user: User;
  topBarVisible = false;
  isSignalrConnected: boolean;
  isMobile = false;
  sidenavOpened = true;

  public settingsNavItems = [
    { linkTitle: 'Cameras', icon: 'videocam', link: '/settings/cameras' },
    { linkTitle: 'OpenALPR Agent', icon: 'api', link: '/settings/agent' },
    { linkTitle: 'Alerts', icon: 'notifications_active', link: '/settings/alerts' },
    { linkTitle: 'Ignores', icon: 'alarm_off', link: '/settings/ignores' },
    { linkTitle: 'Webhook Forwards', icon: 'forward_to_inbox', link: '/settings/forwards' },
    { linkTitle: 'Machine Learning', icon: 'psychology', link: '/settings/machine-learning' },
    { linkTitle: 'System Logs', icon: 'library_books', link: '/settings/logs' },
    { linkTitle: 'Enrichers', icon: 'merge_type', link: '/settings/enrichers' },
    { linkTitle: 'Users', icon: 'person', link: '/settings/users' },
    { linkTitle: 'Debug', icon: 'bug_report', link: '/settings/debug' },
  ];

  private readonly eventSubscriptions = new Subscription();

  constructor() {
    this.accountService.user.subscribe((x) => {
      this.user = x;
      this.topBarVisible = x.id !== undefined;

      if (x.jwtToken) {
        this.signalRService.startConnection();
      } else {
        this.signalRService.stopConnection();
      }
    });

    // Setup mobile detection
    this.eventSubscriptions.add(
      this.breakpointObserver.observe([Breakpoints.HandsetPortrait, Breakpoints.TabletPortrait])
        .subscribe(result => {
          this.isMobile = result.matches;
          if (this.isMobile) {
            this.sidenavOpened = false;
          } else {
            this.sidenavOpened = true;
          }
        }),
    );

    this.swUpdate.unrecoverable.subscribe(() => {
      this.snackBar.open('An error occurred, please reload the page.', 'Reload', { duration: 0 })
        .onAction().subscribe(() => window.location.reload());
    });

    if (this.swUpdate.isEnabled) {
      this.swUpdate.versionUpdates.subscribe((event: VersionEvent) => {
        switch (event.type) {
          case 'VERSION_READY':
            this.snackBar.open('A new version is available', 'Update', { duration: 0 })
              .onAction().subscribe(() => window.location.reload());
            break;
          case 'VERSION_INSTALLATION_FAILED':
            break;
        }
      });
    }
  }

  public ngOnInit() {
    this.subscribeForUpdates();
    this.pushSubscriberService.subscribe();
  }

  public ngOnDestroy() {
    this.signalRService.stopConnection();
  }

  public logout() {
    this.accountService.logout();
  }

  public subscribeForUpdates() {
    this.eventSubscriptions.add(this.signalRService.connectionStatusChanged.subscribe((status) => {
      this.isSignalrConnected = status;
    }));
  }

  public toggleSidenav() {
    this.sidenavOpened = !this.sidenavOpened;
  }

  public closeSidenavOnMobile() {
    if (this.isMobile) {
      this.sidenavOpened = false;
    }
  }

  title = 'openalprwebhookprocessor.client';
}
