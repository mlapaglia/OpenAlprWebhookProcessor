import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SettingsRoutingModule } from './settings-routing.module';
import { SettingsLayoutComponent } from './layout.component';
import { SettingsComponent } from './settings.component';
import { MatSidenavModule, } from '@angular/material/sidenav';
import { MatListModule, } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { CamerasComponent } from './cameras/cameras.component';
import { OpenalprAgentComponent } from './openalpr-agent/openalpr-agent.component';
import { CameraComponent } from './cameras/camera/camera.component';
import { MatButtonModule } from '@angular/material/button';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatDialogModule } from '@angular/material/dialog';
import { FlexLayoutModule } from '@angular/flex-layout';
import { EditCameraComponent } from './cameras/edit-camera/edit-camera.component';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AlertsComponent } from './alerts/alerts.component';
import { IgnoresComponent } from './ignores/ignores.component';
import { IgnoreComponent } from './ignores/ignore/ignore.component';
import { MatTableModule } from '@angular/material/table';

@NgModule({
  declarations: [
    SettingsLayoutComponent,
    SettingsComponent,
    CamerasComponent,
    OpenalprAgentComponent,
    CameraComponent,
    EditCameraComponent,
    AlertsComponent,
    IgnoresComponent,
    IgnoreComponent,
  ],
  imports: [
    CommonModule,
    SettingsRoutingModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatCardModule,
    MatButtonModule,
    MatGridListModule,
    FlexLayoutModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    FormsModule,
    MatButtonModule,
    MatTooltipModule,
    MatTableModule
  ]
})
export class SettingsModule { }
