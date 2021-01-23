import { Component, OnInit } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { SettingsService } from '../settings.service';
import { Ignore } from './ignore/ignore';

@Component({
  selector: 'app-ignores',
  templateUrl: './ignores.component.html',
  styleUrls: ['./ignores.component.less']
})
export class IgnoresComponent implements OnInit {
  public ignores: MatTableDataSource<Ignore>;
  public isSaving: boolean = false;

  constructor(private settingsService: SettingsService) { }

  public rowsToDisplay = [
    'plateNumber',
    'matchType',
    'description',
    'delete',
  ];

  ngOnInit(): void {
    this.getIgnores();
  }

  private getIgnores() {
    this.settingsService.getIgnores().subscribe(result => {
      this.ignores = new MatTableDataSource<Ignore>(result);
    });
  }

  public deleteIgnore(ignore: Ignore) {
    this.ignores.data.forEach((item, index) => {
      if(item === ignore) {
        this.ignores.data.splice(index,1);
      }
    });

    this.ignores._updateChangeSubscription();
  }

  public addIgnore() {
    this.ignores.data.push(new Ignore());
    this.ignores._updateChangeSubscription();
  }

  public saveIgnores() {
    this.isSaving = true;
    this.settingsService.upsertIgnores(this.ignores.data).subscribe(() => {
      this.getIgnores();
      this.isSaving = false;
    });
  }
}
