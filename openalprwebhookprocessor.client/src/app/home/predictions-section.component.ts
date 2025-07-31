import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import type { PredictionResult } from './prediction-response';

@Component({
  selector: 'app-predictions-section',
  templateUrl: './predictions-section.component.html',
  styleUrls: ['./predictions-section.component.css'],
  imports: [
    CommonModule,
    MatCardModule,
    MatListModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
  ],
})
export class PredictionsSectionComponent {
  @Input() nextExpected: PredictionResult | null = null;
  @Input() upcomingPredictions: PredictionResult[] = [];
  @Input() predictablePlates: PredictionResult[] = [];
  @Input() isLoadingPredictions = false;

  public formatTimeUntil(predictedTime: Date): string {
    const now = new Date();
    const predicted = new Date(predictedTime);
    const diffMs = predicted.getTime() - now.getTime();
    const diffHours = Math.round(diffMs / (1000 * 60 * 60));

    if (diffHours < 1) {
      const diffMins = Math.round(diffMs / (1000 * 60));
      return diffMins > 0 ? `${diffMins}m` : 'Now';
    } else if (diffHours < 24) {
      return `${diffHours}h`;
    } else {
      const diffDays = Math.round(diffHours / 24);
      return `${diffDays}d`;
    }
  }

  public getConfidenceColor(confidence: number): string {
    if (confidence >= 0.7) return '#4CAF50'; // Green
    if (confidence >= 0.4) return '#FF9800'; // Orange
    return '#F44336'; // Red
  }

  public getConfidenceText(confidence: number): string {
    if (confidence >= 0.7) return 'High';
    if (confidence >= 0.4) return 'Medium';
    return 'Low';
  }

  public get shouldShow(): boolean {
    return !!(this.nextExpected ||
              this.isLoadingPredictions ||
              (this.upcomingPredictions && this.upcomingPredictions.length > 0) ||
              (this.predictablePlates && this.predictablePlates.length > 0));
  }
}
