import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TitleCasePipe } from '@angular/common';
import type { PlateSettingsConfig } from './plate-settings-table.component';
import type { IPlateSetting } from './plate-setting.interface';

@Component({
  selector: 'app-settings-empty-state',
  template: `
    <div class="empty-state">
      <div class="empty-state-content">
        <mat-icon class="empty-icon">{{config.entityName === 'ignore rule' ? 'block' : 'notifications'}}</mat-icon>
        <h3 class="mat-headline-6">{{config.emptyStateTitle}}</h3>
        <p class="mat-body-1">{{config.emptyStateDescription}}</p>
        <button mat-stroked-button color="primary" (click)="onScrollToForm()">
          <mat-icon i18n>add</mat-icon>
          Add First {{config.entityName | titlecase}}
        </button>
      </div>
    </div>
  `,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    TitleCasePipe,
  ],
})
export class SettingsEmptyStateComponent<T extends IPlateSetting> {
  @Input() config!: PlateSettingsConfig<T>;

  @Output() scrollToForm = new EventEmitter<void>();

  public onScrollToForm(): void {
    this.scrollToForm.emit();
  }
}