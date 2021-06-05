import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { Forward } from './forward';
import { ForwardsService } from './forwards.service';

@Component({
  selector: 'app-forwards',
  templateUrl: './forwards.component.html',
  styleUrls: ['./forwards.component.less']
})
export class ForwardsComponent implements OnInit {
  public forwards: MatTableDataSource<Forward>;
  public isSaving: boolean = false;

  public rowsToDisplay = [
    'destination',
    'ignoreSslErrors',
    'delete',
  ];

  constructor(private forwardsService: ForwardsService) { }

  ngOnInit(): void {
    this.getForwards();
  }

  private getForwards() {
    this.forwardsService.getForwards().subscribe(result => {
      this.forwards = new MatTableDataSource<Forward>(result);
    });
  }

  public saveForwards() {
    this.isSaving = true;
    this.forwardsService.upsertForwards(this.forwards.data).subscribe(result => {
      this.getForwards();
      this.isSaving = false;
    })
  }

  public addForward() {
    this.forwards.data.push(new Forward());
    this.forwards._updateChangeSubscription();
  }

  public deleteForward(forward: Forward) {
    this.forwards.data.forEach((item, index) => {
      if(item === forward) {
        this.forwards.data.splice(index,1);
      }
    });

    this.forwards._updateChangeSubscription();
  }
}
