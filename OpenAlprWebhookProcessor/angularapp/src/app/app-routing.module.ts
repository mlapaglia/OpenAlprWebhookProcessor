import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home';
import { AuthGuard } from './_helpers';
import { OpenalprAgentComponent } from './settings/openalpr-agent/openalpr-agent.component';
import { CamerasComponent } from './settings/cameras/cameras.component';
import { AlertsComponent } from './settings/alerts/alerts.component';
import { EnrichersComponent } from './settings/enrichers/enrichers.component';
import { ForwardsComponent } from './settings/forwards/forwards.component';
import { IgnoresComponent } from './settings/ignores/ignores.component';
import { SystemLogsComponent } from './settings/system-logs/system-logs.component';
import { PlatesComponent } from './plates/plates.component';
import { AddEditComponent } from './users/add-edit.component';
import { UsersComponent } from './users/users.component';
import { LoginComponent } from './account/login.component';
import { RegisterComponent } from './account/register.component';

const routes: Routes = [
    { path: '', component: HomeComponent, canActivate: [AuthGuard] },
    { path: 'account',
        children: [
            { path: 'login', component: LoginComponent },
            { path: 'register', component: RegisterComponent },
        ] },
    { 
        path: 'users',
        children: [
            { path: '', component: UsersComponent },
            { path: 'add', component: AddEditComponent },
            { path: 'edit/:id', component: AddEditComponent }
        ],
        canActivate: [AuthGuard],
    },
    { path: 'plates', component: PlatesComponent, canActivate: [AuthGuard] },
    {
        path: 'settings',
        children: [
            { path: 'agent', component: OpenalprAgentComponent },
            { path: 'alerts', component: AlertsComponent },
            { path: 'cameras', component: CamerasComponent },
            { path: 'enrichers', component: EnrichersComponent },
            { path: 'forwards', component: ForwardsComponent },
            { path: 'ignores', component: IgnoresComponent },
            { path: 'logs', component: SystemLogsComponent },
        ],
        canActivate: [AuthGuard],
    },
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule { }