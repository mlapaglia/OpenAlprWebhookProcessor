import { Component, OnDestroy, OnInit } from '@angular/core';

import { AccountService } from './_services';
import { User } from './_models';
import { SignalrService } from './signalr/signalr.service';

@Component({
    selector: 'app',
    templateUrl: 'app.component.html',
    styleUrls: ['app.component.css']
})
export class AppComponent implements OnInit, OnDestroy {
    user: User;
    appPlatesVisible: boolean;
    appSettingsVisible: boolean;
    
    constructor(
        private signalRService: SignalrService,
        private accountService: AccountService) {
            this.accountService.user.subscribe(x => this.user = x);
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
}