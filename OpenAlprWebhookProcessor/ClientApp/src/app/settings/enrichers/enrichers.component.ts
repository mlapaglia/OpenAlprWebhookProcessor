import { animate, style, transition, trigger } from '@angular/animations';
import { Component, OnInit } from '@angular/core';
import { Enricher } from './enricher';
import { EnrichersService } from './enrichers.service';

@Component({
  selector: 'app-enrichers',
  templateUrl: './enrichers.component.html',
  styleUrls: ['./enrichers.component.less'],
  animations: [
    trigger(
      'inOutAnimation', 
      [
        transition(
          ':enter', [
            style({ height: 0, opacity: 0 }),
            animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', 
                    style({ height: '*', opacity: 1 }))]),
        transition(
          ':leave', [
            style({ height: '*', opacity: 1 }),
            animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)', 
                    style({ height: 0, opacity: 0 }))])
      ])]
})
export class EnrichersComponent implements OnInit {
  public isTesting: boolean;
  public isSaving: boolean;

  public enricher: Enricher;
  
  constructor(private enricherService: EnrichersService) { }

  ngOnInit(): void {
    this.enricherService.getEnricher().subscribe(result => {
      this.enricher = result;
    })
  }

  public testEnricher() {

  }

  public saveEnricher() {

  }
}
