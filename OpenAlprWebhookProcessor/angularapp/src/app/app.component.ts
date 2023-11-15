import { Component, OnDestroy, OnInit } from '@angular/core';
import { AccountService } from './_services';
import { User } from './_models';
import { SignalrService } from './signalr/signalr.service';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { NavBarService } from './_services/nav-bar.service';
import { SwUpdate, VersionEvent } from '@angular/service-worker';
import { PushSubscriberService } from './_services/push-subscriber.service';
import { AlertComponent } from './_components/alert.component';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { CommonModule, NgIf } from '@angular/common';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';

@Component({
    selector: 'app',
    templateUrl: 'app.component.html',
    styleUrls: ['app.component.css'],
    standalone: true,
    imports: [NgIf, MatTabsModule, RouterLink, MatIconModule, AlertComponent, RouterOutlet, MatSidenavModule, MatListModule, CommonModule]
})
export class AppComponent implements OnInit, OnDestroy {
    user: User;
    appSettingsVisible: boolean;
    menuButtonVisible: boolean;
    topBarVisible: boolean;
    navBarVisible: boolean = false;
    
    public navItems = [
        { linkTitle: "Cameras", icon: "videocam", link: "/settings/cameras"},
        { linkTitle: "OpenALPR Agent", icon: "api", link: "/settings/agent"},
        { linkTitle: "Alerts", icon: "notifications_active", link: "/settings/alerts"},
        { linkTitle: "Ignores", icon: "alarm_off", link: "/settings/ignores"},
        { linkTitle: "Webhook Forwards", icon: "forward_to_inbox", link: "/settings/forwards"},
        { linkTitle: "System Logs", icon: "library_books", link: "/settings/logs"},
        { linkTitle: "Enrichers", icon: "merge_type", link: "/settings/enrichers"},
    ];

    constructor(
        private signalRService: SignalrService,
        private accountService: AccountService,
        private navBarService: NavBarService,
        private router: Router,
        private swUpdate: SwUpdate,
        private pushSubscriberService: PushSubscriberService) {
            this.accountService.user.subscribe(x => {
                this.topBarVisible = x.id !== undefined;
            });

            this.router.events.subscribe((routerEvent) => {
                if(routerEvent instanceof(NavigationEnd)) {
                    this.menuButtonVisible =(routerEvent as NavigationEnd).url == "/settings";
                }
            });

            this.swUpdate.unrecoverable.subscribe(event => {
                confirm('An error occurred, please reload the page.')
                {
                    window.location.reload();
                }
            });

            if (this.swUpdate.isEnabled) {
                this.swUpdate.versionUpdates.subscribe((event: VersionEvent) => {
                    switch (event.type) {
                        case 'VERSION_READY':
                          if (confirm("You're using an old version of the control panel. Want to update?")) {
                            window.location.reload();
                          }
                          break;
                        case 'VERSION_INSTALLATION_FAILED':
                          console.log(`Failed to install app version '${event.version.hash}': ${event.error}`);
                          break;
                      }
                });
            }
    }

    public ngOnInit() {
        this.signalRService.startConnection();
        this.pushSubscriberService.subscribe();
    }

    public ngOnDestroy() {
        this.signalRService.stopConnection();
    }

    public logout() {
        this.accountService.logout();
    }

    public settingsButtonClicked() {
        this.navBarVisible = !this.navBarVisible;
    }

    public handleSideNavClick() {
        this.navBarVisible = false;
    }
}
