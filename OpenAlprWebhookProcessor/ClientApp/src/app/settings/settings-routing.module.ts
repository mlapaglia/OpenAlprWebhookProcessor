import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SettingsLayoutComponent } from './layout.component';
import { SettingsComponent } from './settings.component';

const routes: Routes = [
    {
        path: '', component: SettingsComponent,
        children: [
            { path: '', component: SettingsComponent },
        ]
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class SettingsRoutingModule { }