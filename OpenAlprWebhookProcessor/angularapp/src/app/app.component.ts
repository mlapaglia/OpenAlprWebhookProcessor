import { Component, OnDestroy, OnInit } from '@angular/core';
import { AccountService } from './_services';
import { User } from './_models';
import { SignalrService } from './signalr/signalr.service';
import { NavigationEnd, Router } from '@angular/router';
import { NavBarService } from './_services/nav-bar.service';
import { SwUpdate, VersionEvent, VersionReadyEvent } from '@angular/service-worker';
import { PushSubscriberService } from './_services/push-subscriber.service';

@Component({
    selector: 'app',
    templateUrl: 'app.component.html',
    styleUrls: ['app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
    user: User;
    appSettingsVisible: boolean;
    menuButtonVisible: boolean;
    topBarVisible: boolean;

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
                        case 'VERSION_DETECTED':
                          console.log(`Downloading new app version: ${event.version.hash}`);
                          break;
                        case 'VERSION_READY':
                          console.log(`Current app version: ${(event as VersionReadyEvent).currentVersion}, New app version: ${(event as VersionReadyEvent).latestVersion}`);
                          if(confirm("You're using an old version of the control panel. Want to update?")) {
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
        this.navBarService.settingsClicked();
    }
}