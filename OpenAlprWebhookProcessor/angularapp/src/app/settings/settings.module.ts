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
import { EditCameraComponent } from './cameras/edit-camera/edit-camera.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AlertsComponent } from './alerts/alerts.component';
import { IgnoresComponent } from './ignores/ignores.component';
import { IgnoreComponent } from './ignores/ignore/ignore.component';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ForwardsComponent } from './forwards/forwards.component';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { SystemLogsComponent } from './systemLogs/systemLogs.component';
import { HighlightModule } from 'ngx-highlightjs';
import { PushoverComponent } from './alerts/pushover/pushover.component';
import { EnrichersComponent } from './enrichers/enrichers.component';
import { MatRadioModule } from '@angular/material/radio';

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
    ForwardsComponent,
    SystemLogsComponent,
    PushoverComponent,
    EnrichersComponent,
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
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatTooltipModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSlideToggleModule,
    MatCheckboxModule,
    MatRadioModule,
    HighlightModule,
  ]
})
export class SettingsModule { }
