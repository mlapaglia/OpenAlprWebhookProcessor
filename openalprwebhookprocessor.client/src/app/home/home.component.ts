import { Component, OnInit, inject } from '@angular/core';
import { User } from 'app/_models';
import { AccountService } from 'app/_services';
import { HomeService } from './home.service';
import { BarChartModule } from '@swimlane/ngx-charts';
import { MatCardModule } from '@angular/material/card';

@Component({
    templateUrl: 'home.component.html',
    imports: [MatCardModule, BarChartModule]
})
export class HomeComponent implements OnInit {
    private accountService = inject(AccountService);
    private homeService = inject(HomeService);

    user: User;
    public plateCounts: { name: Date; value: number }[];

    view: [number, number] = [700, 400];
    constructor() {
        this.user = this.accountService.userValue;
    }
    
    ngOnInit() {
        this.homeService.getPlatesCount().subscribe(result => {
            this.plateCounts = [];

            result.counts.forEach(x => {
                this.plateCounts.push(
                    {
                        name: x.date,
                        value: x.count
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
}