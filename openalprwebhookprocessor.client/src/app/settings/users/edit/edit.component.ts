import { Component, inject, type OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule, FormsModule, type FormGroup } from '@angular/forms';
import { first } from 'rxjs/operators';
import { AccountService, AlertService } from 'app/_services';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';

interface DialogData {
  userId?: string;
}

@Component({
  templateUrl: 'edit.component.html',
  styleUrl: 'edit.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    RefreshButtonComponent,
  ],
})
export class AddEditComponent extends OnPushBaseComponent implements OnInit {
  data = inject<DialogData>(MAT_DIALOG_DATA);

  private readonly formBuilder = inject(FormBuilder);
  private readonly accountService = inject(AccountService);
  private readonly alertService = inject(AlertService);
  private readonly dialogRef = inject(MatDialogRef<AddEditComponent>);

  form: FormGroup;
  id: string;
  isAddMode: boolean;
  loading = false;
  submitted = false;

  ngOnInit() {
    this.id = this.data.userId ?? '';
    this.isAddMode = !this.id;

    // password not required in edit mode
    const passwordValidators = [Validators.minLength(6)];
    if (this.isAddMode) {
      passwordValidators.push(Validators.required);
    }

    this.form = this.formBuilder.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', passwordValidators],
    });

    if (!this.isAddMode) {
      this.subscribeAndMarkForCheck(
        this.accountService.getById(this.id).pipe(first()),
        (user) => {
          this.form.patchValue(user);
        },
        (error) => {
          this.alertService.error(String(error));
        },
      );
    }
  }

  // convenience getter for easy access to form fields
  get f() {
    return this.form.controls;
  }

  // Helper methods for template validation
  get showFirstNameError(): boolean {
    return this.submitted && !!this.f.firstName.errors?.['required'];
  }

  get showLastNameError(): boolean {
    return this.submitted && !!this.f.lastName.errors?.['required'];
  }

  get showUsernameError(): boolean {
    return this.submitted && !!this.f.username.errors?.['required'];
  }

  get showPasswordRequiredError(): boolean {
    return this.submitted && !!this.f.password.errors?.['required'];
  }

  get showPasswordMinLengthError(): boolean {
    return this.submitted && !!this.f.password.errors?.['minlength'];
  }

  onSubmit() {
    this.submitted = true;
    this.markForCheck();

    // reset alerts on submit
    this.alertService.clear();

    // stop here if form is invalid
    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.markForCheck();

    if (this.isAddMode) {
      this.addUser();
    } else {
      this.updateUser();
    }
  }

  onCancel() {
    this.dialogRef.close(false);
  }

  private addUser() {
    this.subscribeAndMarkForCheck(
      this.accountService.add(this.form.value).pipe(first()),
      () => {
        this.alertService.success('User added successfully', true);
        this.loading = false;
        this.dialogRef.close(true);
      },
      (error) => {
        this.alertService.error(String(error));
        this.loading = false;
      },
    );
  }

  private updateUser() {
    this.subscribeAndMarkForCheck(
      this.accountService.update(this.id, this.form.value).pipe(first()),
      () => {
        this.alertService.success('Update successful', true);
        this.loading = false;
        this.dialogRef.close(true);
      },
      (error) => {
        this.alertService.error(String(error));
        this.loading = false;
      },
    );
  }
}
