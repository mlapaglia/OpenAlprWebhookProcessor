import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-most-seen-plates',
  templateUrl: './most-seen-plates.component.html',
  styleUrls: ['./most-seen-plates.component.css'],
  imports: [
    CommonModule,
    MatCardModule,
    MatListModule,
    MatChipsModule,
  ],
})
export class MostSeenPlatesComponent {
  @Input() mostSeenCounts: { name: string, value: number }[] = [];
}