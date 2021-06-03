import { Component, OnInit } from '@angular/core';
import { User } from '@app/_models';
import { AccountService } from '@app/_services';
import { HomeService } from './home.service';
import { DayCount } from './plateCountResponse';

@Component({ templateUrl: 'home.component.html' })
export class HomeComponent implements OnInit {
    user: User;
    public plateCounts: any[];

    view: any[] = [700, 400];
    constructor(
        private accountService: AccountService,
        private homeService: HomeService) {
        this.user = this.accountService.userValue;
    }
    
    ngOnInit() {
        this.homeService.getPlatesCount().subscribe(result => {
            this.plateCounts = [];
            
            result.counts.forEach(x => {
                this.plateCounts.push(
                {
                    name: x.date,
                    value: x.count,
                });
            });
        });
    }

      // options
    showXAxis = true;
    showYAxis = true;
    gradient = false;
    showLegend = false;
    showXAxisLabel = true;
    xAxisLabel = 'Date';
    showYAxisLabel = true;
    yAxisLabel = 'Plates Seen';

    colorScheme = {
        domain: ['#5AA454', '#A10A28', '#C7B42C', '#AAAAAA']
    };

    onSelect(event) {
        console.log(event);
      }
}