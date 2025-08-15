import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-most-seen-plates',
  templateUrl: './most-seen-plates.component.html',
  styleUrls: ['./most-seen-plates.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatCardModule,
    MatListModule,
    MatChipsModule,
  ],
})
export class MostSeenPlatesComponent {
  readonly mostSeenCounts = input<{ name: string, value: number }[]>([]);
}
