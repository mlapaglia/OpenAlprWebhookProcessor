import { NgModule } from '@angular/core';
import { RouterModule, type Routes } from '@angular/router';
import { HomeComponent } from './home';
import { AuthGuard, LoginGuard } from './_helpers';
import { OpenalprAgentComponent } from './settings/openalpr-agent/openalpr-agent.component';
import { CamerasComponent } from './settings/cameras/cameras.component';
import { AlertsComponent } from './settings/alerts/alerts.component';
import { EnrichersComponent } from './settings/enrichers/enrichers.component';
import { ForwardsComponent } from './settings/forwards/forwards.component';
import { IgnoresComponent } from './settings/ignores/ignores.component';
import { MachineLearningComponent } from './settings/machine-learning/machine-learning/machine-learning.component';
import { SystemLogsComponent } from './settings/system-logs/system-logs.component';
import { PlatesComponent } from './plates/plates.component';
import { AddEditComponent } from './settings/users/edit/edit.component';
import { UsersComponent } from './settings/users/users.component';
import { User2FAComponent } from './settings/users/user-2fa/user-2fa.component';
import { LoginComponent } from './account/login/login.component';
import { RegisterComponent } from './account/register/register.component';
import { Verify2FAComponent } from './account/verify-2fa/verify-2fa.component';
import { Setup2FAComponent } from './account/setup-2fa/setup-2fa.component';
import { DebugComponent } from './settings/debug/debug.component';

const routes: Routes = [
  { path: '', component: HomeComponent, canActivate: [AuthGuard] },
  {
    path: 'account',
    children: [
      { path: 'login', component: LoginComponent, canActivate: [LoginGuard] },
      { path: 'register', component: RegisterComponent },
      { path: 'verify-2fa', component: Verify2FAComponent },
      { path: 'setup-2fa', component: Setup2FAComponent, canActivate: [AuthGuard] },
      { path: '', redirectTo: 'login', pathMatch: 'full' },
    ],
  },
  { path: 'plate/:id', component: PlatesComponent, canActivate: [AuthGuard] },
  { path: 'plates', component: PlatesComponent, canActivate: [AuthGuard] },
  {
    path: 'settings',
    children: [
      { path: 'agent', component: OpenalprAgentComponent },
      { path: 'alerts', component: AlertsComponent },
      { path: 'cameras', component: CamerasComponent },
      { path: 'debug', component: DebugComponent },
      { path: 'enrichers', component: EnrichersComponent },
      { path: 'forwards', component: ForwardsComponent },
      { path: 'ignores', component: IgnoresComponent },
      { path: 'logs', component: SystemLogsComponent },
      { path: 'machine-learning', component: MachineLearningComponent },
      {
        path: 'users',
        children: [
          { path: '', component: UsersComponent },
          { path: 'add', component: AddEditComponent },
          { path: 'edit/:id', component: AddEditComponent },
          { path: '2fa/:id', component: User2FAComponent },
        ],
      },
      { path: '', redirectTo: 'cameras', pathMatch: 'full' },
    ],
    canActivate: [AuthGuard],
  },
  { path: '**', redirectTo: '/', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule { }
