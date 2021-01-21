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
  
  constructor(private settingsService: SettingsService) { }

  public rowsToDisplay = [
    'plateNumber',
    'matchType',
    'description',
    'delete',
  ];

  ngOnInit(): void {
    this.settingsService.getIgnores().subscribe(result => {
      this.ignores = new MatTableDataSource<Ignore>(result);
    });
  }

  public deleteIgnore(ignore: Ignore) {
    const arr = this.ignores.data.filter(item => item.id !== ignore.id);
  }

  public addIgnore() {
    this.ignores.data.push(new Ignore());
    this.ignores._updateChangeSubscription();
  }
}
