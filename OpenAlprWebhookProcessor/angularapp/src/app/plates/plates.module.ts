import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlatesRoutingModule } from './plates-routing.module';
import { PlatesComponent } from './plates.component';
import { PlatesLayoutComponent } from './layout.component';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PlateComponent } from './plate/plate.component';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { EditPlateComponent } from './edit-plate/edit-plate.component';
import { MatDialogModule } from '@angular/material/dialog';

@NgModule({
    imports: [
        CommonModule,
        PlatesRoutingModule,
        MatPaginatorModule,
        MatTableModule,
        MatIconModule,
        MatCardModule,
        MatDatepickerModule,
        MatNativeDateModule,
        MatFormFieldModule,
        MatIconModule,
        MatSelectModule,
        MatInputModule,
        MatButtonModule,
        MatFormFieldModule,
        MatProgressSpinnerModule,
        MatExpansionModule,
        ReactiveFormsModule,
        FormsModule,
        MatCheckboxModule,
        MatDividerModule,
        NgxChartsModule,
        MatAutocompleteModule,
        MatDialogModule,
        PlatesLayoutComponent,
        PlatesComponent,
        PlateComponent,
        EditPlateComponent,
    ]
})
export class PlatesModule { }
