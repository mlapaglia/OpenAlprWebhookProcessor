import { Component, Input, OnInit } from '@angular/core';
import { Ignore } from './ignore';
import { MatButtonModule } from '@angular/material/button';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
    selector: 'app-ignore',
    templateUrl: './ignore.component.html',
    styleUrls: ['./ignore.component.less'],
    standalone: true,
    imports: [MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule, MatSelectModule, MatOptionModule, MatButtonModule]
})
export class IgnoreComponent implements OnInit {
  @Input() ignore: Ignore;
  
  constructor() { }

  ngOnInit(): void {
  }

  public deleteIgnore(ignore: Ignore) {
  }
}
