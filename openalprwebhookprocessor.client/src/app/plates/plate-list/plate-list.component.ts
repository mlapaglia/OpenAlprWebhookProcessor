import { Component, ChangeDetectionStrategy, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { PageEvent } from '@angular/material/paginator';
import { MatPaginatorModule } from '@angular/material/paginator';
import { PlateItemComponent, type PlateData } from '../plate-item/plate-item.component';

@Component({
  selector: 'app-plate-list',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    MatExpansionModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    PlateItemComponent,
  ],
  templateUrl: './plate-list.component.html',
  styleUrl: './plate-list.component.less',
})
export class PlateListComponent {
  readonly plates = input.required<PlateData[]>();
  readonly isLoading = input.required<boolean>();
  readonly pageSize = input.required<number>();
  readonly totalNumberOfPlates = input.required<number>();

  readonly plateOpened = output<string>();
  readonly plateClosed = output<string>();
  readonly enrichPlate = output<string>();
  readonly editPlate = output<string>();
  readonly alertPlate = output<string>();
  readonly ignorePlate = output<string>();
  readonly searchForPlate = output<string>();
  readonly paginatorChange = output<PageEvent>();

  onPlateOpened(plateId: string) {
    this.plateOpened.emit(plateId);
  }

  onPlateClosed(plateId: string) {
    this.plateClosed.emit(plateId);
  }

  onEnrichPlate(plateId: string) {
    this.enrichPlate.emit(plateId);
  }

  onEditPlate(plateId: string) {
    this.editPlate.emit(plateId);
  }

  onAlertPlate(plateId: string) {
    this.alertPlate.emit(plateId);
  }

  onIgnorePlate(plateId: string) {
    this.ignorePlate.emit(plateId);
  }

  onSearchForPlate(plateId: string) {
    this.searchForPlate.emit(plateId);
  }

  onPaginatorPage(event: PageEvent) {
    this.paginatorChange.emit(event);
  }
}
