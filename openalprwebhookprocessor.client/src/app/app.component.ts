import { Component, inject, type OnDestroy, type OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AccountService } from './_services';
import type { User } from './_models';
import { SignalrService } from './signalr/signalr.service';
import { Router, RouterLink, RouterOutlet, NavigationEnd } from '@angular/router';
import { SwUpdate } from '@angular/service-worker';
import { filter } from 'rxjs/operators';
import { PushSubscriberService } from './_services/push-subscriber.service';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { CommonModule } from '@angular/common';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { ThemePickerComponent } from './theme-picker/theme-picker.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { OnPushBaseComponent } from './_helpers/onpush-base.component';

@Component({
  selector: 'app-app',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatTabsModule, RouterLink, MatIconModule, RouterOutlet,
    MatSidenavModule, MatListModule, MatDividerModule, CommonModule, ThemePickerComponent,
    MatButtonModule, MatMenuModule, MatToolbarModule,
  ],
})
export class AppComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
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
  private isAuthenticated = false;

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

  title = 'openalprwebhookprocessor.client';

  constructor() {
    super();

    // Subscribe to user changes for authentication status and SignalR connection
    this.subscribeAndMarkForCheck(
      this.accountService.user,
      (x) => {
        this.user = x;
        this.isAuthenticated = !!x.id;

        // Update topbar visibility based on both auth status and route
        this.updateTopBarVisibility();

        if (x.id) {
          this.signalRService.startConnection();
        } else {
          this.signalRService.stopConnection();
        }
      },
    );

    // Subscribe to route changes to control navbar visibility
    this.subscribeAndMarkForCheck(
      this.router.events.pipe(filter(event => event instanceof NavigationEnd)),
      (_event: NavigationEnd) => {
        // Update topbar visibility when route changes
        this.updateTopBarVisibility();
      },
    );
  }

  public ngOnInit() {
    this.subscribeForUpdates();
    void this.pushSubscriberService.subscribe();

    this.subscribeAndMarkForCheck(
      this.breakpointObserver.observe(['(max-width: 768px)']),
      (result) => {
        const wasMobile = this.isMobile;
        this.isMobile = result.matches;

        // Only change sidenav state when crossing the breakpoint, not on every resize
        if (wasMobile !== this.isMobile) {
          this.sidenavOpened = !this.isMobile; // Open sidenav on desktop, close on mobile
        }
      },
    );

    // Check for service worker updates
    if (this.swUpdate.isEnabled) {
      this.subscribeAndMarkForCheck(
        this.swUpdate.versionUpdates,
        (event) => {
          if (event.type === 'VERSION_READY') {
            const snackBarRef = this.snackBar.open('New version available', 'Reload', {
              duration: 10000, // Auto dismiss after 10 seconds
            });

            snackBarRef.onAction().subscribe(() => {
              window.location.reload();
            });
          }
        },
      );
    }
  }

  override ngOnDestroy() {
    // Stop SignalR connection
    this.signalRService.stopConnection();

    super.ngOnDestroy();
  }

  public logout() {
    this.accountService.logout();
  }

  public subscribeForUpdates() {
    this.subscribeAndMarkForCheck(
      this.signalRService.connectionStatusChanged,
      (status) => {
        this.isSignalrConnected = status;
      },
    );
  }

  public toggleSidenav() {
    this.sidenavOpened = !this.sidenavOpened;
    this.markForCheck();
  }

  public closeSidenavOnMobile() {
    if (this.isMobile) {
      this.sidenavOpened = false;
      this.markForCheck();
    }
  }

  public onNavigationKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.closeSidenavOnMobile();
    }
  }

  public onLogoutKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.logout();
    }
  }

  private updateTopBarVisibility() {
    const currentRoute = this.router.url;
    const isAccountRoute = currentRoute.startsWith('/account/');

    // Show topbar only if user is authenticated AND not on account pages
    this.topBarVisible = this.isAuthenticated && !isAccountRoute;
  }
}
