import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import type { IPlateSetting } from './plate-setting.interface';

@Component({
  selector: 'app-settings-header',
  template: `
    <mat-card-header>
      <mat-card-title i18n class="mat-headline-6">Current {{config.title}}</mat-card-title>
      <mat-card-subtitle i18n>
        {{itemCount}} {{itemCount === 1 ? (config.entityName) : (config.entityName + 's')}} configured
      </mat-card-subtitle>
      <div class="header-spacer"></div>
      <button 
        mat-raised-button 
        color="primary" 
        [disabled]="isSaving || itemCount === 0"
        (click)="onSaveSettings()"
        class="save-button">
        @if(!isSaving) {
          <mat-icon i18n>save</mat-icon>
        }
        @if(isSaving) {
          <mat-spinner diameter="20" />
        }
        {{ isSaving ? 'Saving...' : 'Save All Changes' }}
      </button>
    </mat-card-header>
  `,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
})
export class SettingsHeaderComponent<T extends IPlateSetting> {
  @Input() config!: PlateSettingsConfig<T>;
  @Input() itemCount = 0;
  @Input() isSaving = false;

  @Output() saveSettings = new EventEmitter<void>();

  public onSaveSettings(): void {
    this.saveSettings.emit();
  }
}