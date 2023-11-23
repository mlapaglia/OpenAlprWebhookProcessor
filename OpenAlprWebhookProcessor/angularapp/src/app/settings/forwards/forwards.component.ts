import { Component, OnInit } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Forward } from './forward';
import { ForwardsService } from './forwards.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
    selector: 'app-forwards',
    templateUrl: './forwards.component.html',
    styleUrls: ['./forwards.component.less'],
    standalone: true,
    imports: [MatTableModule, MatFormFieldModule, MatInputModule, ReactiveFormsModule, FormsModule, MatCheckboxModule, MatButtonModule]
})
export class ForwardsComponent implements OnInit {
    public forwards: MatTableDataSource<Forward>;
    public isSaving: boolean = false;

    public rowsToDisplay = [
        'destination',
        'forwardSinglePlates',
        'forwardGroupPreviews',
        'forwardGroups',
        'ignoreSslErrors',
        'delete'
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
        this.forwardsService.upsertForwards(this.forwards.data).subscribe(() => {
            this.getForwards();
            this.isSaving = false;
        });
    }

    public addForward() {
        this.forwards.data.push(new Forward());
        this.forwards._updateChangeSubscription();
    }

    public deleteForward(forward: Forward) {
        this.forwards.data.forEach((item, index) => {
            if (item === forward) {
                this.forwards.data.splice(index, 1);
            }
        });

        this.forwards._updateChangeSubscription();
    }
}
