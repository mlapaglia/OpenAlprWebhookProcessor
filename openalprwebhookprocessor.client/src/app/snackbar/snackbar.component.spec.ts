import type { ComponentFixture } from '@angular/core/testing';
import { TestBed } from '@angular/core/testing';
import { MatSnackBarRef, MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { SnackbarComponent } from './snackbar.component';
import { SnackBar } from './snackbar';
import { SnackBarType } from './snackbartype';
import { By } from '@angular/platform-browser';

describe('SnackbarComponent', () => {
  let component: SnackbarComponent;
  let fixture: ComponentFixture<SnackbarComponent>;
  let mockSnackBarRef: jasmine.SpyObj<MatSnackBarRef<SnackbarComponent>>;

  const createComponent = (snackBarData: SnackBar) => {
    TestBed.configureTestingModule({
      imports: [SnackbarComponent, MatButtonModule, MatIconModule],
      providers: [
        { provide: MatSnackBarRef, useValue: mockSnackBarRef },
        { provide: MAT_SNACK_BAR_DATA, useValue: snackBarData },
      ],
    });
    fixture = TestBed.createComponent(SnackbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  };

  beforeEach(() => {
    mockSnackBarRef = jasmine.createSpyObj('MatSnackBarRef', ['dismiss']);
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);
      expect(component).toBeTruthy();
    });

    it('should inject snack bar data', () => {
      const snackBarData = new SnackBar({ message: 'Test message', snackType: SnackBarType.Info });
      createComponent(snackBarData);
      expect(component.data).toBe(snackBarData);
    });

    it('should inject snack bar reference', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);
      expect(component.snackBarRef).toBe(mockSnackBarRef);
    });
  });

  describe('getIcon getter', () => {
    it('should return warning icon for Alert type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Alert });
      createComponent(snackBarData);
      expect(component.getIcon).toBe('warning');
    });

    it('should return info icon for Info type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);
      expect(component.getIcon).toBe('info');
    });

    it('should return wifi icon for Connected type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Connected });
      createComponent(snackBarData);
      expect(component.getIcon).toBe('wifi');
    });

    it('should return wifi_off icon for Disconnected type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Disconnected });
      createComponent(snackBarData);
      expect(component.getIcon).toBe('wifi_off');
    });

    it('should return save icon for Saved type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Saved });
      createComponent(snackBarData);
      expect(component.getIcon).toBe('save');
    });

    it('should return delete icon for Deleted type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Deleted });
      createComponent(snackBarData);
      expect(component.getIcon).toBe('delete');
    });

    it('should return check_circle icon for Successful type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Successful });
      createComponent(snackBarData);
      expect(component.getIcon).toBe('check_circle');
    });

    it('should return error icon for Error type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Error });
      createComponent(snackBarData);
      expect(component.getIcon).toBe('error');
    });
  });

  describe('getIconColor getter', () => {
    it('should return orange color for Alert type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Alert });
      createComponent(snackBarData);
      expect(component.getIconColor).toBe('#ff9800');
    });

    it('should return blue color for Info type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);
      expect(component.getIconColor).toBe('#2196f3');
    });

    it('should return blue color for Connected type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Connected });
      createComponent(snackBarData);
      expect(component.getIconColor).toBe('#2196f3');
    });

    it('should return blue color for Saved type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Saved });
      createComponent(snackBarData);
      expect(component.getIconColor).toBe('#2196f3');
    });

    it('should return green color for Successful type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Successful });
      createComponent(snackBarData);
      expect(component.getIconColor).toBe('#4caf50');
    });

    it('should return red color for Error type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Error });
      createComponent(snackBarData);
      expect(component.getIconColor).toBe('#f44336');
    });

    it('should return red color for Disconnected type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Disconnected });
      createComponent(snackBarData);
      expect(component.getIconColor).toBe('#f44336');
    });

    it('should return red color for Deleted type', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Deleted });
      createComponent(snackBarData);
      expect(component.getIconColor).toBe('#f44336');
    });
  });

  describe('dismiss method', () => {
    it('should call snackBarRef.dismiss when dismiss is called', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      component.dismiss();

      expect(mockSnackBarRef.dismiss).toHaveBeenCalled();
    });
  });

  describe('Template rendering', () => {
    it('should display the primary message', () => {
      const snackBarData = new SnackBar({ message: 'Primary message', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      const primaryMessage = fixture.debugElement.query(By.css('.primary-message'));
      expect(primaryMessage.nativeElement.textContent).toBe('Primary message');
    });

    it('should display the secondary message when provided', () => {
      const snackBarData = new SnackBar({
        message: 'Primary message',
        message2: 'Secondary message',
        snackType: SnackBarType.Info,
      });
      createComponent(snackBarData);

      const secondaryMessage = fixture.debugElement.query(By.css('.secondary-message'));
      expect(secondaryMessage).toBeTruthy();
      expect(secondaryMessage.nativeElement.textContent).toBe('Secondary message');
    });

    it('should not display secondary message when not provided', () => {
      const snackBarData = new SnackBar({ message: 'Primary message', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      const secondaryMessage = fixture.debugElement.query(By.css('.secondary-message'));
      expect(secondaryMessage).toBeFalsy();
    });

    it('should display the correct icon', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Alert });
      createComponent(snackBarData);

      const icon = fixture.debugElement.query(By.css('.snackbar-icon'));
      expect(icon.nativeElement.textContent.trim()).toBe('warning');
    });

    it('should apply the correct icon color', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Alert });
      createComponent(snackBarData);

      const icon = fixture.debugElement.query(By.css('.snackbar-icon'));
      expect(icon.nativeElement.style.color).toBe('rgb(255, 152, 0)'); // #ff9800 in rgb
    });

    it('should have close button that calls dismiss', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      const closeButton = fixture.debugElement.query(By.css('.snackbar-action'));
      closeButton.nativeElement.click();

      expect(mockSnackBarRef.dismiss).toHaveBeenCalled();
    });

    it('should display close icon in action button', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      const closeIcon = fixture.debugElement.query(By.css('.snackbar-action mat-icon'));
      expect(closeIcon.nativeElement.textContent.trim()).toBe('close');
    });
  });

  describe('Edge cases', () => {
    it('should handle empty message', () => {
      const snackBarData = new SnackBar({ message: '', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      const primaryMessage = fixture.debugElement.query(By.css('.primary-message'));
      expect(primaryMessage.nativeElement.textContent).toBe('');
    });

    it('should handle empty secondary message', () => {
      const snackBarData = new SnackBar({
        message: 'Primary',
        message2: '',
        snackType: SnackBarType.Info,
      });
      createComponent(snackBarData);

      const secondaryMessage = fixture.debugElement.query(By.css('.secondary-message'));
      expect(secondaryMessage).toBeFalsy();
    });

    it('should handle null secondary message', () => {
      const snackBarData = new SnackBar({
        message: 'Primary',
        message2: null as any,
        snackType: SnackBarType.Info,
      });
      createComponent(snackBarData);

      const secondaryMessage = fixture.debugElement.query(By.css('.secondary-message'));
      expect(secondaryMessage).toBeFalsy();
    });

    it('should handle undefined secondary message', () => {
      const snackBarData = new SnackBar({
        message: 'Primary',
        message2: undefined as any,
        snackType: SnackBarType.Info,
      });
      createComponent(snackBarData);

      const secondaryMessage = fixture.debugElement.query(By.css('.secondary-message'));
      expect(secondaryMessage).toBeFalsy();
    });
  });

  describe('Component structure', () => {
    it('should have correct CSS classes', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      expect(fixture.debugElement.query(By.css('.snackbar-content'))).toBeTruthy();
      expect(fixture.debugElement.query(By.css('.snackbar-icon'))).toBeTruthy();
      expect(fixture.debugElement.query(By.css('.snackbar-messages'))).toBeTruthy();
      expect(fixture.debugElement.query(By.css('.snackbar-action'))).toBeTruthy();
      expect(fixture.debugElement.query(By.css('.primary-message'))).toBeTruthy();
    });

    it('should have matSnackBarLabel directive', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      const messagesDiv = fixture.debugElement.query(By.css('[matSnackBarLabel]'));
      expect(messagesDiv).toBeTruthy();
    });

    it('should have matSnackBarAction directive', () => {
      const snackBarData = new SnackBar({ message: 'Test', snackType: SnackBarType.Info });
      createComponent(snackBarData);

      const actionButton = fixture.debugElement.query(By.css('[matSnackBarAction]'));
      expect(actionButton).toBeTruthy();
    });
  });
});
