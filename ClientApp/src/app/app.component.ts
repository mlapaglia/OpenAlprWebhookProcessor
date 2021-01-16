import { Component } from '@angular/core';

import { AccountService } from './_services';
import { User } from './_models';

@Component({
    selector: 'app',
    templateUrl: 'app.component.html',
    styleUrls: ['app.component.css']
})
export class AppComponent {
    user: User;
    appPlatesVisible: boolean;
    appSettingsVisible: boolean;
    
    constructor(
        private accountService: AccountService) {
            this.accountService.user.subscribe(x => this.user = x);
    }

    public logout() {
        this.accountService.logout();
    }
}