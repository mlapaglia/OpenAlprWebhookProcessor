import { Component, inject, type OnInit, ChangeDetectionStrategy } from '@angular/core';
import { first } from 'rxjs/operators';
import { AccountService } from 'app/_services';
import type { User } from 'app/_models';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { AddEditComponent } from './edit/edit.component';
import { User2FAComponent } from './user-2fa/user-2fa.component';
import { UserPasskeysComponent } from './user-passkeys/user-passkeys.component';

@Component({
  templateUrl: 'users.component.html',
  styleUrl: 'users.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterModule, MatButtonModule, MatTableModule, MatIconModule, MatProgressSpinnerModule, MatCardModule],
})
export class UsersComponent extends OnPushBaseComponent implements OnInit {
  private readonly accountService = inject(AccountService);
  private readonly dialog = inject(MatDialog);

  users: User[] = [];
  public displayedColumns: string[] = ['firstName', 'lastName', 'username', 'twoFactor', 'actions'];

  ngOnInit() {
    this.subscribeAndMarkForCheck(
      this.accountService.getAll().pipe(first()),
      (users) => {
        this.users = users;
      },
      (error) => {
        console.error('Error loading users:', error);
      },
    );
  }

  editUser(user: User) {
    const dialogRef = this.dialog.open(AddEditComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: { userId: user.id },
      disableClose: true,
    });

    this.subscribeAndMarkForCheck(
      dialogRef.afterClosed(),
      (result) => {
        if (result) {
          // Refresh the users list after successful edit
          this.ngOnInit();
        }
      },
    );
  }

  manage2FA(user: User) {
    const dialogRef = this.dialog.open(User2FAComponent, {
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      data: { userId: user.id, userName: user.username },
      disableClose: true,
    });

    this.subscribeAndMarkForCheck(
      dialogRef.afterClosed(),
      () => {
        // No need to refresh users list for 2FA changes
        // as it doesn't affect the main user data
      },
    );
  }

  managePasskeys(user: User) {
    const dialogRef = this.dialog.open(UserPasskeysComponent, {
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      data: { userId: user.id, userName: user.username },
      disableClose: true,
    });

    this.subscribeAndMarkForCheck(
      dialogRef.afterClosed(),
      () => {
        // No need to refresh users list for passkey changes
        // as it doesn't affect the main user data
      },
    );
  }

  addUser() {
    const dialogRef = this.dialog.open(AddEditComponent, {
      width: '600px',
      maxWidth: '90vw',
      data: { userId: undefined },
      disableClose: true,
    });

    this.subscribeAndMarkForCheck(
      dialogRef.afterClosed(),
      (result) => {
        if (result) {
          // Refresh the users list after successful add
          this.ngOnInit();
        }
      },
    );
  }

  deleteUser(id: number) {
    const user = this.users.find(x => x.id === id);
    if (user !== undefined) {
      user.isDeleting = true;
      this.markForCheck();
      this.subscribeAndMarkForCheck(
        this.accountService.delete(id).pipe(first()),
        () => {
          this.users = this.users.filter(x => x.id !== id);
        },
      );
    }
  }
}
