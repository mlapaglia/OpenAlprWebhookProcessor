import { Component, Input, OnInit } from '@angular/core';
import { Ignore } from './ignore';

@Component({
  selector: 'app-ignore',
  templateUrl: './ignore.component.html',
  styleUrls: ['./ignore.component.less']
})
export class IgnoreComponent implements OnInit {
  @Input() ignore: Ignore;
  
  constructor() { }



  ngOnInit(): void {
  }

  public deleteIgnore(ignore: Ignore) {
    
  }

}
