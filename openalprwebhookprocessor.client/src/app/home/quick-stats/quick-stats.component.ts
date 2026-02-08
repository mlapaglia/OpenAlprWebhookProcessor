import { Component, ChangeDetectionStrategy, input } from '@angular/core';

import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { QuickStats } from './../home.service';

@Component({
  selector: 'app-quick-stats',
  templateUrl: './quick-stats.component.html',
  styleUrls: ['./quick-stats.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatCardModule,
    MatIconModule,
    MatGridListModule,
    MatProgressSpinnerModule,
  ],
})
export class QuickStatsComponent {
  readonly quickStats = input<QuickStats | null>(null);
  readonly isLoadingStats = input<boolean>(false);
  readonly quickStatCols = input<number>(4);
}
