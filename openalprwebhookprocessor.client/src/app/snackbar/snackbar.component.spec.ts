import { ComponentFixture, TestBed } from '@angular/core/testing'
import { SnackbarComponent } from './snackbar.component'
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar'
import { SnackBar } from './snackbar'
import { SnackBarType } from './snackbartype'
import { MatIconModule } from '@angular/material/icon'
import { DebugElement } from '@angular/core'
import { By } from '@angular/platform-browser'

describe('SnackbarComponent', () => {
  let component: SnackbarComponent
  let fixture: ComponentFixture<SnackbarComponent>

  const createComponent = (snackBarData: Partial<SnackBar>) => {
    TestBed.configureTestingModule({
      imports: [SnackbarComponent, MatIconModule],
      providers: [
        {
          provide: MAT_SNACK_BAR_DATA,
          useValue: new SnackBar(snackBarData),
        },
      ],
    }).compileComponents()

    fixture = TestBed.createComponent(SnackbarComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  }

  describe('Component Initialization', () => {
    beforeEach(async () => {
      await createComponent({
        message: 'test',
        message2: 'test123',
        snackType: SnackBarType.Info,
      })
    })

    it('should create', () => {
      expect(component).toBeTruthy()
    })

    it('should have injected data', () => {
      expect(component.data).toBeDefined()
      expect(component.data.message).toBe('test')
      expect(component.data.message2).toBe('test123')
      expect(component.data.snackType).toBe(SnackBarType.Info)
    })
  })

  describe('Icon Generation', () => {
    it('should return correct icon for Alert type', async () => {
      await createComponent({ snackType: SnackBarType.Alert })
      expect(component.getIcon).toBe('taxi_alert')
    })

    it('should return correct icon for Info type', async () => {
      await createComponent({ snackType: SnackBarType.Info })
      expect(component.getIcon).toBe('info')
    })

    it('should return correct icon for Connected type', async () => {
      await createComponent({ snackType: SnackBarType.Connected })
      expect(component.getIcon).toBe('signal_wifi_4_bar')
    })

    it('should return correct icon for Disconnected type', async () => {
      await createComponent({ snackType: SnackBarType.Disconnected })
      expect(component.getIcon).toBe('signal_cellular_off')
    })

    it('should return correct icon for Saved type', async () => {
      await createComponent({ snackType: SnackBarType.Saved })
      expect(component.getIcon).toBe('saved')
    })

    it('should return correct icon for Deleted type', async () => {
      await createComponent({ snackType: SnackBarType.Deleted })
      expect(component.getIcon).toBe('delete')
    })

    it('should return correct icon for Successful type', async () => {
      await createComponent({ snackType: SnackBarType.Successful })
      expect(component.getIcon).toBe('check')
    })

    it('should return correct icon for Error type', async () => {
      await createComponent({ snackType: SnackBarType.Error })
      expect(component.getIcon).toBe('error')
    })
  })

  describe('Template Rendering', () => {
    beforeEach(async () => {
      await createComponent({
        message: 'Primary message',
        message2: 'Secondary message',
        snackType: SnackBarType.Info,
      })
    })

    it('should display the primary message', () => {
      const messageElement = fixture.debugElement.query(
        By.css('.snack-container > div:nth-child(2) > div:first-child span')
      )
      expect(messageElement.nativeElement.textContent.trim()).toBe('Primary message')
    })

    it('should display the secondary message', () => {
      const message2Element = fixture.debugElement.query(
        By.css('.snack-container > div:nth-child(2) > div:last-child span')
      )
      expect(message2Element.nativeElement.textContent.trim()).toBe('Secondary message')
    })

    it('should display the correct icon', () => {
      const iconElement = fixture.debugElement.query(By.css('mat-icon'))
      expect(iconElement.nativeElement.textContent.trim()).toBe('info')
    })

    it('should have correct container structure', () => {
      const containerElement = fixture.debugElement.query(By.css('.snack-container'))
      expect(containerElement).toBeTruthy()
      expect(containerElement.nativeElement.style.display).toBe('flex')
    })

  })

  describe('Template Rendering without Secondary Message', () => {
    beforeEach(async () => {
      await createComponent({
        message: 'Only primary message',
        snackType: SnackBarType.Info,
      })
    })

    it('should render without secondary message when not provided', () => {
      const message2Element = fixture.debugElement.query(
        By.css('.snack-container > div:nth-child(2) > div:last-child span')
      )
      expect(message2Element.nativeElement.textContent.trim()).toBe('')
    })
  })

  describe('Different SnackBar Types Template Rendering', () => {
    it('should render Alert icon in template', async () => {
      await createComponent({
        message: 'Alert message',
        snackType: SnackBarType.Alert,
      })

      const iconElement = fixture.debugElement.query(By.css('mat-icon'))
      expect(iconElement.nativeElement.textContent.trim()).toBe('taxi_alert')
    })

    it('should render Error icon in template', async () => {
      await createComponent({
        message: 'Error message',
        snackType: SnackBarType.Error,
      })

      const iconElement = fixture.debugElement.query(By.css('mat-icon'))
      expect(iconElement.nativeElement.textContent.trim()).toBe('error')
    })

    it('should render Successful icon in template', async () => {
      await createComponent({
        message: 'Success message',
        snackType: SnackBarType.Successful,
      })

      const iconElement = fixture.debugElement.query(By.css('mat-icon'))
      expect(iconElement.nativeElement.textContent.trim()).toBe('check')
    })
  })
})
