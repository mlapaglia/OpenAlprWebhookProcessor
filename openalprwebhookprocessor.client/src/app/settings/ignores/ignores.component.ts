import { Component, inject, type OnInit, type OnDestroy } from '@angular/core';
import type { FormGroup } from '@angular/forms';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';

import { SettingsService } from '../settings.service';
import { Ignore } from './ignore';

@Component({
  selector: 'app-ignores',
  templateUrl: './ignores.component.html',
  styleUrls: ['./ignores.component.less'],
  imports: [
    CommonModule,
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
  ],
})
export class IgnoresComponent implements OnInit, OnDestroy {
  private readonly settingsService = inject(SettingsService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly subscriptions = new Subscription();

  // UI State
  public isLoading = false;
  public hasError = false;
  public isSaving = false;
  public editingIgnore: Ignore | null = null;

  // Data
  public ignores: Ignore[] = [];

  // Forms
  public quickAddForm: FormGroup;
  public editForm: FormGroup;

  constructor() {
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

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  public loadIgnores(): void {
    this.isLoading = true;
    this.hasError = false;

    const sub = this.settingsService.getIgnores().subscribe({
      next: (ignores) => {
        this.ignores = ignores;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading ignores:', error);
        this.hasError = true;
        this.isLoading = false;
        this.snackBar.open('Failed to load ignore rules', 'Close', { duration: 5000 });
      },
    });

    this.subscriptions.add(sub);
  }

  public addIgnore(): void {
    if (this.quickAddForm.invalid || this.isSaving) return;

    const formValue = this.quickAddForm.value;
    const newIgnore = new Ignore({
      plateNumber: formValue.plateNumber.trim().toUpperCase(),
      description: formValue.description?.trim() || '',
      strictMatch: formValue.strictMatch,
    });

    this.saveIgnores([...this.ignores, newIgnore], 'Ignore rule added successfully');
    this.quickAddForm.reset({
      plateNumber: '',
      description: '',
      strictMatch: true,
    });
  }

  public startEdit(ignore: Ignore): void {
    this.editingIgnore = ignore;
    this.editForm.patchValue({
      plateNumber: ignore.plateNumber,
      description: ignore.description,
      strictMatch: ignore.strictMatch,
    });
  }

  public saveEdit(): void {
    if (this.editForm.invalid || this.isSaving || !this.editingIgnore) return;

    const formValue = this.editForm.value;
    const updatedIgnore = new Ignore({
      ...this.editingIgnore,
      plateNumber: formValue.plateNumber.trim().toUpperCase(),
      description: formValue.description?.trim() || '',
      strictMatch: formValue.strictMatch,
    });

    const updatedIgnores = this.ignores.map(ignore =>
      ignore.id === this.editingIgnore!.id ? updatedIgnore : ignore,
    );

    this.saveIgnores(updatedIgnores, 'Ignore rule updated successfully');
  }

  public cancelEdit(): void {
    this.editingIgnore = null;
    this.editForm.reset();
  }

  public deleteIgnore(ignore: Ignore): void {
    if (this.isSaving) return;

    const updatedIgnores = this.ignores.filter(i => i.id !== ignore.id);
    this.saveIgnores(updatedIgnores, 'Ignore rule deleted successfully');
  }

  private saveIgnores(ignores: Ignore[], successMessage: string): void {
    this.isSaving = true;

    const sub = this.settingsService.upsertIgnores(ignores).subscribe({
      next: () => {
        this.ignores = ignores;
        this.editingIgnore = null;
        this.isSaving = false;
        this.snackBar.open(successMessage, 'Close', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error saving ignores:', error);
        this.isSaving = false;
        this.snackBar.open('Failed to save ignore rules', 'Close', { duration: 5000 });
      },
    });

    this.subscriptions.add(sub);
  }
}
