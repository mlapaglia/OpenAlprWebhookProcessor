import { Component, OnInit, inject } from '@angular/core'
import { CommonModule } from '@angular/common'
import { SettingsService } from '../settings.service'
import { Ignore } from './ignore'
import { PlateSettingsTableComponent, PlateSettingsConfig } from '../shared/plate-settings-table.component'

@Component({
  selector: 'app-ignores',
  templateUrl: './ignores.component.html',
  styleUrls: ['./ignores.component.less'],
  imports: [
    CommonModule,
    PlateSettingsTableComponent
  ],
})
export class IgnoresComponent implements OnInit {
  private settingsService = inject(SettingsService)

  public ignoresConfig: PlateSettingsConfig<Ignore> = {
    title: 'License Plate Ignores',
    subtitle: 'Configure license plates to ignore during processing. These plates will be filtered out from alerts and notifications.',
    emptyStateTitle: 'No Ignore Rules',
    emptyStateDescription: 'You haven\'t created any ignore rules yet. Add your first rule above to get started.',
    addButtonText: 'Add Ignore Rule',
    entityName: 'ignore rule',
    createNew: () => new Ignore({
      plateNumber: '',
      strictMatch: true,
      description: ''
    }),
    service: {
      getAll: () => this.settingsService.getIgnores(),
      upsert: (items: Ignore[]) => this.settingsService.upsertIgnores(items)
    }
  }

  ngOnInit(): void {
    // Initialization is handled by the shared component
  }

  public onIgnoresChanged(ignores: Ignore[]): void {
    // Handle any specific logic when ignores change if needed
    console.log('Ignores changed:', ignores)
  }
}
