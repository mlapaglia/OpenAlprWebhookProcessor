import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlatesRoutingModule } from './plates-routing.module';
import { PlatesComponent } from './plates.component';
import { PlatesLayoutComponent } from './layout.component';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';

@NgModule({
  declarations: [
    PlatesLayoutComponent,
    PlatesComponent,
  ],
  imports: [
    CommonModule,
    PlatesRoutingModule,
    MatPaginatorModule,
    MatTableModule,
    MatIconModule,
  ]
})
export class PlatesModule { }
