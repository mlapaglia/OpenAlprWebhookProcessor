import { Component, OnInit, inject } from '@angular/core';
import { first } from 'rxjs/operators';
import { AccountService } from 'app/_services';
import { User } from 'app/_models';

import { RouterLink, RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';

@Component({
    templateUrl: 'users.component.html',
    imports: [RouterLink, RouterModule, MatButtonModule, MatTableModule]
})
export class UsersComponent implements OnInit {
    private accountService = inject(AccountService);

    users: User[] = [];
    public displayedColumns: string[] = ['firstName', 'lastName', 'username', 'actions'];

    ngOnInit() {
        this.accountService.getAll()
            .pipe(first())
            .subscribe(users => this.users = users);
    }

    deleteUser(id: string) {
        const user = this.users.find(x => x.id === id);
        if (user !== undefined) {
            user.isDeleting = true;
            this.accountService.delete(id)
                .pipe(first())
                .subscribe(() => this.users = this.users.filter(x => x.id !== id));
        }
    }
}