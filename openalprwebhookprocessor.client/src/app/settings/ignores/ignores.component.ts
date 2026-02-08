import { Component, inject, type OnInit, type OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { type FormGroup, FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';


import { Ignore } from './ignore';
import { IgnoresService } from './ignores.service';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';
import { RefreshButtonComponent } from 'app/shared/refresh-button/refresh-button.component';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import { SnackBarType } from 'app/snackbar/snackbartype';

@Component({
  selector: 'app-ignores',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './ignores.component.html',
  styleUrls: ['./ignores.component.less'],
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
    RefreshButtonComponent
],
})
export class IgnoresComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly ignoresService = inject(IgnoresService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackbarService = inject(SnackbarService);

  public isLoading = false;
  public hasError = false;
  public isSaving = false;
  public editingIgnore: Ignore | null = null;
  public deletingIgnoreIds = new Set<string>();

  public ignores: Ignore[] = [];

  public quickAddForm: FormGroup;
  public editForm: FormGroup;

  constructor() {
    super();
    this.quickAddForm = this.formBuilder.group({
      plateNumber: ['', [Validators.required, Validators.minLength(1)]],
      description: [''],
      strictMatch: [true],
    });

    this.editForm = this.formBuilder.group({
      plateNumber: ['', [Validators.required, Validators.minLength(1)]],
      description: [''],
      strictMatch: [true],
    });
  }

  ngOnInit(): void {
    this.loadIgnores();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  public loadIgnores(): void {
    this.isLoading = true;
    this.hasError = false;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.ignoresService.getIgnores(),
      (ignores) => {
        this.ignores = ignores;
        this.isLoading = false;
      },
      _ => {
        this.hasError = true;
        this.isLoading = false;
        this.snackbarService.create('Failed to load ignore rules', SnackBarType.Error);
      },
    );
  }

  public addIgnore(): void {
    if (this.quickAddForm.invalid || this.isSaving) return;

    const formValue = this.quickAddForm.value;
    const newIgnore = new Ignore({
      plateNumber: formValue.plateNumber.trim().toUpperCase(),
      description: formValue.description?.trim() ?? '',
      strictMatch: formValue.strictMatch,
    });

    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.ignoresService.addIgnore(newIgnore),
      () => {
        this.isSaving = false;
        this.quickAddForm.reset({ strictMatch: true });
        this.loadIgnores();
        this.snackbarService.create('Ignore rule added successfully', SnackBarType.Saved);
      },
      _ => {
        this.isSaving = false;
        this.snackbarService.create('Failed to add ignore rule', SnackBarType.Error);
      },
    );
  }

  public startEdit(ignore: Ignore): void {
    this.editingIgnore = ignore;
    this.editForm.patchValue({
      plateNumber: ignore.plateNumber,
      description: ignore.description,
      strictMatch: ignore.strictMatch,
    });

    this.markForCheck();
  }

  public saveEdit(): void {
    if (this.editForm.invalid || this.isSaving || !this.editingIgnore) return;

    const formValue = this.editForm.value;
    const updatedIgnore = new Ignore({
      ...this.editingIgnore,
      plateNumber: formValue.plateNumber.trim().toUpperCase(),
      description: formValue.description?.trim() ?? '',
      strictMatch: formValue.strictMatch,
    });

    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.ignoresService.updateIgnore(updatedIgnore),
      () => {
        this.editingIgnore = null;
        this.isSaving = false;
        this.editForm.reset();
        this.loadIgnores();
        this.snackbarService.create('Ignore rule updated successfully', SnackBarType.Saved);
      },
      (error) => {
        console.error('Error updating ignore:', error);
        this.isSaving = false;
        this.snackbarService.create('Failed to update ignore rule', SnackBarType.Error, error as string);
      },
    );
  }

  public cancelEdit(): void {
    this.editingIgnore = null;
    this.editForm.reset();
    this.markForCheck();
  }

  public isDeleting(ignoreId: string): boolean {
    return this.deletingIgnoreIds.has(ignoreId);
  }

  public deleteIgnore(ignore: Ignore): void {
    if (!ignore.id || this.deletingIgnoreIds.has(ignore.id)) return;

    this.deletingIgnoreIds.add(ignore.id);
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.ignoresService.deleteIgnore(ignore.id),
      () => {
        this.ignores = this.ignores.filter(i => i.id !== ignore.id);
        this.deletingIgnoreIds.delete(ignore.id);
        this.snackbarService.create('Ignore rule deleted successfully', SnackBarType.Successful);
      },
      (error) => {
        this.deletingIgnoreIds.delete(ignore.id);
        this.snackbarService.create('Failed to delete ignore rule', SnackBarType.Error, error as string);
      },
    );
  }
}
