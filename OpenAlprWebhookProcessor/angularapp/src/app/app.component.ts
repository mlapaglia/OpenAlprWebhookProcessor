import { Component, OnDestroy, OnInit } from '@angular/core';
import { AccountService } from './_services';
import { User } from './_models';
import { SignalrService } from './signalr/signalr.service';
import { NavigationEnd, Router } from '@angular/router';
import { NavBarService } from './_services/nav-bar.service';

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
        private router: Router) {
            this.accountService.user.subscribe(x => {
                this.topBarVisible = x.id !== undefined;
            });

            this.router.events.subscribe((routerEvent) => {
                if(routerEvent instanceof(NavigationEnd)) {
                    this.menuButtonVisible =(routerEvent as NavigationEnd).url == "/settings";
                }
            });
    }

    public ngOnInit() {
        this.signalRService.startConnection();
    }

    public ngOnDestroy() {
        this.signalRService.stopConnection();
    }

    public logout() {
        this.accountService.logout();
    }

    public selectedTabChange(event: Event) {
        console.log(event);
    }

    public settingsButtonClicked() {
        this.navBarService.settingsClicked();
    }
}