import { ChangeDetectionStrategy, Component, inject, type OnDestroy, type OnInit } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Forward } from './forward';
import { ForwardsService } from './forwards.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { OnPushBaseComponent } from 'app/_helpers/onpush-base.component';

@Component({
  selector: 'app-forwards',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './forwards.component.html',
  styleUrls: ['./forwards.component.less'],
  imports: [
    MatTableModule, MatFormFieldModule, MatInputModule,
    ReactiveFormsModule, FormsModule, MatCheckboxModule, MatButtonModule,
    MatCardModule, MatIconModule,
  ],
})
export class ForwardsComponent extends OnPushBaseComponent implements OnInit, OnDestroy {
  private readonly forwardsService = inject(ForwardsService);

  public forwards: MatTableDataSource<Forward>;
  public isSaving = false;

  public rowsToDisplay = [
    'destination',
    'forwardSinglePlates',
    'forwardGroupPreviews',
    'forwardGroups',
    'ignoreSslErrors',
    'delete',
  ];

  ngOnInit(): void {
    this.getForwards();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  private getForwards(): void {
    this.subscribeAndMarkForCheck(
      this.forwardsService.getForwards(),
      (result) => {
        this.forwards = new MatTableDataSource<Forward>(result);
      },
      (error) => {
        console.error('Failed to load forwards:', error);
      },
    );
  }

  public saveForwards(): void {
    this.isSaving = true;
    this.markForCheck();

    this.subscribeAndMarkForCheck(
      this.forwardsService.upsertForwards(this.forwards.data),
      () => {
        this.getForwards();
        this.isSaving = false;
      },
      (error) => {
        console.error('Failed to save forwards:', error);
        this.isSaving = false;
      },
    );
  }

  public addForward(): void {
    this.forwards.data.push(new Forward());
    this.forwards._updateChangeSubscription();
    this.markForCheck();
  }

  public deleteForward(forward: Forward): void {
    this.forwards.data.forEach((item, index) => {
      if (item === forward) {
        this.forwards.data.splice(index, 1);
      }
    });

    this.forwards._updateChangeSubscription();
    this.markForCheck();
  }
}
