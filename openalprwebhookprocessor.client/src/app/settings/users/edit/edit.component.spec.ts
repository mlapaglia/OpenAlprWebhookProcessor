import { TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { AddEditComponent } from './edit.component';
import { AccountService, AlertService } from 'app/_services';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

describe('AddEditComponent', () => {
  let component: AddEditComponent;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockAlertService: jasmine.SpyObj<AlertService>;
  let mockDialogRef: jasmine.SpyObj<MatDialogRef<AddEditComponent>>;

  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', ['getById', 'add', 'update']);
    mockAlertService = jasmine.createSpyObj('AlertService', ['clear', 'success', 'error']);
    mockDialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);

    await TestBed.configureTestingModule({
      imports: [AddEditComponent, ReactiveFormsModule, NoopAnimationsModule],
      providers: [
        { provide: AccountService, useValue: mockAccountService },
        { provide: AlertService, useValue: mockAlertService },
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: { userId: null } },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AddEditComponent);
    component = fixture.componentInstance;
  });

  describe('ngOnInit', () => {
    it('should initialize form in add mode', () => {
      component.data = { userId: undefined };

      component.ngOnInit();

      expect(component.isAddMode).toBe(true);
      expect(component.form).toBeDefined();
      expect(component.form.get('firstName')?.hasError('required')).toBe(true);
      expect(component.form.get('password')?.hasError('required')).toBe(true);
    });

    it('should initialize form in edit mode and load user data', () => {
      const mockUser = {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: '',
        twoFactorEnabled: false,
        isDeleting: false,
      };
      component.data = { userId: '1' };
      mockAccountService.getById.and.returnValue(of(mockUser));

      component.ngOnInit();

      expect(component.isAddMode).toBe(false);
      expect(component.form).toBeDefined();
      expect(mockAccountService.getById).toHaveBeenCalledWith('1');
    });
  });

  describe('form validation', () => {
    beforeEach(() => {
      component.data = { userId: undefined };
      component.ngOnInit();
    });

    it('should be invalid when required fields are empty', () => {
      expect(component.form.invalid).toBe(true);
      expect(component.form.get('firstName')?.errors?.['required']).toBe(true);
      expect(component.form.get('lastName')?.errors?.['required']).toBe(true);
      expect(component.form.get('username')?.errors?.['required']).toBe(true);
    });

    it('should be valid when all required fields are filled', () => {
      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: 'password123',
      });

      expect(component.form.valid).toBe(true);
    });
  });

  describe('onSubmit', () => {
    beforeEach(() => {
      component.data = { userId: undefined };
      component.ngOnInit();
    });

    it('should not submit if form is invalid', () => {
      component.onSubmit();

      expect(component.submitted).toBe(true);
      expect(mockAccountService.add).not.toHaveBeenCalled();
    });

    it('should add user in add mode', () => {
      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: 'password123',
      });
      mockAccountService.add.and.returnValue(of({}));

      component.onSubmit();

      expect(mockAccountService.add).toHaveBeenCalledWith(jasmine.objectContaining({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: 'password123',
      }));
      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });

    it('should update user in edit mode', () => {
      const mockUser = {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: '',
        twoFactorEnabled: false,
        isDeleting: false,
      };
      component.data = { userId: '1' };
      mockAccountService.getById.and.returnValue(of(mockUser));
      mockAccountService.update.and.returnValue(of({}));

      component.ngOnInit();
      component.form.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: '',
      });

      component.onSubmit();

      expect(mockAccountService.update).toHaveBeenCalledWith('1', jasmine.objectContaining({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        password: '',
      }));
      expect(mockDialogRef.close).toHaveBeenCalledWith(true);
    });
  });

  describe('onCancel', () => {
    it('should close dialog with false result', () => {
      component.onCancel();
      expect(mockDialogRef.close).toHaveBeenCalledWith(false);
    });
  });
});
