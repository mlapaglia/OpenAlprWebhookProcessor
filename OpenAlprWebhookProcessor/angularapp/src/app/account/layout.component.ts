import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';

import { AccountService } from 'app/_services';

@Component({
    templateUrl: 'layout.component.html',
    standalone: true,
    imports: [RouterOutlet]
})
export class LayoutComponent {
    constructor(
        private router: Router,
        private accountService: AccountService
    ) {
        // redirect to home if already logged in
        if (this.accountService.userValue) {
            this.router.navigate(['/']);
        }
    }
}