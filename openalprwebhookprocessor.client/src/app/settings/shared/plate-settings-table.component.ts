import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, inject, TemplateRef } from '@angular/core'
import { CommonModule } from '@angular/common'
import { MatTableDataSource, MatTableModule } from '@angular/material/table'
import { MatButtonModule } from '@angular/material/button'
import { MatOptionModule } from '@angular/material/core'
import { MatSelectModule } from '@angular/material/select'
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms'
import { MatInputModule } from '@angular/material/input'
import { MatFormFieldModule } from '@angular/material/form-field'
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner'
import { MatIconModule } from '@angular/material/icon'
import { MatTooltipModule } from '@angular/material/tooltip'
import { MatSnackBar } from '@angular/material/snack-bar'
import { MatCardModule } from '@angular/material/card'
import { MatChipsModule } from '@angular/material/chips'
import { IPlateSetting } from './plate-setting.interface'
import { Observable, Subscription } from 'rxjs'

export interface PlateSettingsConfig<T extends IPlateSetting> {
  title: string
  subtitle: string
  emptyStateTitle: string
  emptyStateDescription: string
  addButtonText: string
  entityName: string // e.g., 'alert rule', 'ignore rule'
  createNew: () => T
  service: {
    getAll: () => Observable<T[]>
    upsert: (items: T[]) => Observable<any>
  }
}

@Component({
  selector: 'app-plate-settings-table',
  templateUrl: './plate-settings-table.component.html',
  styleUrls: ['./plate-settings-table.component.less'],
  imports: [
    CommonModule,
    MatTableModule, 
    MatFormFieldModule, 
    MatInputModule, 
    FormsModule,
    ReactiveFormsModule,
    MatSelectModule, 
    MatOptionModule, 
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatTooltipModule,
    MatCardModule,
    MatChipsModule
  ],
})
export class PlateSettingsTableComponent<T extends IPlateSetting> implements OnInit, OnDestroy {
  @Input() config!: PlateSettingsConfig<T>
  @Input() additionalContent?: TemplateRef<any>
  @Output() settingsChanged = new EventEmitter<T[]>()

  private snackBar = inject(MatSnackBar)
  private formBuilder = inject(FormBuilder)
  private subscriptions = new Subscription()

  public settings = new MatTableDataSource<T>([])
  public settingForm: FormGroup
  public isSaving = false
  public isLoading = false
  public hasError = false
  public isAddingSetting = false
  public editingId: string | null = null
  public editingSetting: T | null = null

  public readonly displayedColumns = [
    'plateNumber',
    'description',
    'actions',
  ]

  constructor() {
    this.settingForm = this.formBuilder.group({
      plateNumber: ['', [Validators.required, Validators.minLength(2)]],
      strictMatch: [true, [Validators.required]],
      description: ['']
    })
  }

  ngOnInit(): void {
    if (!this.config) {
      throw new Error('PlateSettingsTableComponent requires a config input')
    }
    this.loadSettings()
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe()
  }

  public loadSettings(): void {
    this.isLoading = true
    this.hasError = false
    
    const subscription = this.config.service.getAll().subscribe({
      next: (result) => {
        this.settings.data = result
        this.isLoading = false
        this.settingsChanged.emit(result)
      },
      error: () => {
        this.hasError = true
        this.isLoading = false
        this.snackBar.open(`Failed to load ${this.config.entityName}s. Please try again.`, 'Close', {
          duration: 5000
        })
      }
    })
    
    this.subscriptions.add(subscription)
  }

  public onAddSetting(): void {
    if (this.settingForm.invalid || this.isAddingSetting) {
      return
    }

    const plateNumber = this.settingForm.get('plateNumber')?.value?.trim()
    
    if (this.isDuplicatePlate(plateNumber)) {
      this.settingForm.get('plateNumber')?.setErrors({ duplicate: true })
      return
    }

    this.isAddingSetting = true

    const newSetting = this.config.createNew()
    Object.assign(newSetting, {
      plateNumber: plateNumber,
      strictMatch: this.settingForm.get('strictMatch')?.value ?? true,
      description: this.settingForm.get('description')?.value?.trim() || ''
    })

    const currentData = [...this.settings.data]
    currentData.push(newSetting)
    this.settings.data = currentData
    this.settingsChanged.emit(this.settings.data)

    this.resetForm()
    this.isAddingSetting = false
    
    this.snackBar.open(`${this.config.entityName} added. Remember to save your changes.`, 'Close', {
      duration: 3000
    })
  }

  public resetForm(): void {
    this.settingForm.reset({
      plateNumber: '',
      strictMatch: true,
      description: ''
    })
    this.settingForm.get('plateNumber')?.setErrors(null)
  }

  public startEdit(setting: T): void {
    this.cancelEdit()
    this.editingId = setting.id || this.generateTempId(setting)
    this.editingSetting = { ...setting } as T
  }

  public cancelEdit(): void {
    this.editingId = null
    this.editingSetting = null
  }

  public saveEdit(setting: T): void {
    if (!this.editingSetting) return

    if (!this.editingSetting.plateNumber?.trim()) {
      this.snackBar.open('Plate number is required.', 'Close', {
        duration: 3000
      })
      return
    }

    if (this.editingSetting.plateNumber.trim().length < 2) {
      this.snackBar.open('Plate number must be at least 2 characters.', 'Close', {
        duration: 3000
      })
      return
    }

    if (this.isDuplicatePlateForEdit(this.editingSetting.plateNumber.trim(), setting)) {
      this.snackBar.open('This plate number already exists.', 'Close', {
        duration: 3000
      })
      return
    }

    Object.assign(setting, {
      plateNumber: this.editingSetting.plateNumber.trim(),
      strictMatch: this.editingSetting.strictMatch,
      description: this.editingSetting.description?.trim() || ''
    })

    this.cancelEdit()
    this.settingsChanged.emit(this.settings.data)
    this.snackBar.open('Changes saved locally. Remember to save all changes.', 'Close', {
      duration: 3000
    })
  }

  public isEditing(setting: T): boolean {
    const id = setting.id || this.generateTempId(setting)
    return this.editingId === id
  }

  public deleteSetting(settingToDelete: T): void {
    const currentData = [...this.settings.data]
    const filteredData = currentData.filter(setting => setting !== settingToDelete)
    this.settings.data = filteredData
    this.settingsChanged.emit(this.settings.data)
  }

  public confirmDelete(setting: T): void {
    const plateNumber = setting.plateNumber || 'Unknown'
    
    if (confirm(`Are you sure you want to delete the ${this.config.entityName} for "${plateNumber}"?`)) {
      this.deleteSetting(setting)
      this.snackBar.open(`${this.config.entityName} removed. Remember to save your changes.`, 'Close', {
        duration: 3000
      })
    }
  }

  public scrollToForm(): void {
    const formElement = document.querySelector('.add-setting-card')
    if (formElement) {
      formElement.scrollIntoView({ behavior: 'smooth', block: 'start' })
    }
  }

  public saveSettings(): void {
    if (this.isSaving) {
      return
    }

    const validSettings = this.settings.data.filter(setting => 
      setting.plateNumber && setting.plateNumber.trim().length > 0
    )

    if (validSettings.length === 0) {
      this.snackBar.open(`Please add at least one ${this.config.entityName} with a plate number.`, 'Close', {
        duration: 3000
      })
      return
    }

    this.isSaving = true
    
    const subscription = this.config.service.upsert(validSettings).subscribe({
      next: () => {
        this.isSaving = false
        this.snackBar.open(`${this.config.entityName}s saved successfully!`, 'Close', {
          duration: 3000
        })
        this.loadSettings()
      },
      error: () => {
        this.isSaving = false
        this.snackBar.open(`Failed to save ${this.config.entityName}s. Please try again.`, 'Close', {
          duration: 5000
        })
      }
    })
    
    this.subscriptions.add(subscription)
  }

  public trackByFn(index: number, item: T): any {
    return item.id || index
  }

  private isDuplicatePlate(plateNumber: string): boolean {
    if (!plateNumber) return false
    
    return this.settings.data.some(setting => 
      setting.plateNumber?.toLowerCase() === plateNumber.toLowerCase()
    )
  }

  private isDuplicatePlateForEdit(plateNumber: string, currentSetting: T): boolean {
    if (!plateNumber) return false
    
    return this.settings.data.some(setting => 
      setting !== currentSetting && 
      setting.plateNumber?.toLowerCase() === plateNumber.toLowerCase()
    )
  }

  private generateTempId(setting: T): string {
    return `temp_${setting.plateNumber}_${setting.strictMatch}_${Date.now()}`
  }
} 