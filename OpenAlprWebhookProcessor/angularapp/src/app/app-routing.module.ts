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

const accountModule = () => import('./account/account.module').then(x => x.AccountModule);
const usersModule = () => import('./users/users.module').then(x => x.UsersModule);
const platesModule = () => import('./plates/plates.module').then(x => x.PlatesModule);

const routes: Routes = [
    { path: '', component: HomeComponent, canActivate: [AuthGuard] },
    { path: 'users', loadChildren: usersModule, canActivate: [AuthGuard] },
    { path: 'account', loadChildren: accountModule },
    { path: 'plates', loadChildren: platesModule, canActivate: [AuthGuard] },
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