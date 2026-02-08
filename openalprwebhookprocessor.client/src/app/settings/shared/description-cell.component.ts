import { ChangeDetectionStrategy, Component, input, type OnDestroy } from '@angular/core';

import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import type { IPlateSetting } from '../shared/plate-setting.interface';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-description-cell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if(!isEditing()) {
      <span class="description-text" [class.no-description]="!setting()!.description">
        {{setting()!.description || 'No description provided'}}
      </span>
    }

    @if(isEditing() && editingSetting()) {
      <mat-form-field appearance="outline" i18n-appearance class="description-edit-field">
        <mat-label i18n>Description</mat-label>
        <textarea
          matInput
          [(ngModel)]="editingSetting()!.description"
          placeholder="Add description..." i18n-placeholder
          rows="2"
          maxlength="200">
        </textarea>
      </mat-form-field>
    }
  `,
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatInputModule
],
})
export class DescriptionCellComponent<T extends IPlateSetting> extends OnPushBaseComponent implements OnDestroy {
  readonly setting = input<T>();
  readonly isEditing = input<boolean>(false);
  readonly editingSetting = input<T | null>(null);

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }
}
