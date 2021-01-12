import { Component } from '@angular/core';

import { AccountService } from './_services';
import { User } from './_models';
import { FormControl } from '@angular/forms';
import { PlateService } from './plates/plate.service';

@Component({
    selector: 'app',
    templateUrl: 'app.component.html',
    styleUrls: ['app.component.css']
})
export class AppComponent {
    user: User;
    public hydrationInProgress: boolean = false;

    constructor(
        private accountService: AccountService,
        private plateService: PlateService) {
            this.accountService.user.subscribe(x => this.user = x);
    }

    public logout() {
        this.accountService.logout();
    }

    public hydrate() {
        this.hydrationInProgress = true;
        this.plateService.hydrateDatabase().subscribe(_ => {
            this.hydrationInProgress = false;
        })
    }
}