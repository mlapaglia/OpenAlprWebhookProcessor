import { TestBed, type ComponentFixture, fakeAsync, tick } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatDialog } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';
import { UsersComponent } from './users.component';
import { AccountService } from 'app/_services';
import type { User } from 'app/_models';

describe('UsersComponent', () => {
  let component: UsersComponent;
  let fixture: ComponentFixture<UsersComponent>;
  let mockAccountService: jasmine.SpyObj<AccountService>;
  let mockDialog: jasmine.SpyObj<MatDialog>;

  beforeEach(async () => {
    mockAccountService = jasmine.createSpyObj('AccountService', ['getAll', 'delete']);
    mockDialog = jasmine.createSpyObj('MatDialog', ['open']);

    await TestBed.configureTestingModule({
      imports: [UsersComponent, NoopAnimationsModule],
      providers: [
        { provide: AccountService, useValue: mockAccountService },
        { provide: MatDialog, useValue: mockDialog },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UsersComponent);
    component = fixture.componentInstance;
  });

  describe('component initialization', () => {
    it('should create', () => {
      mockAccountService.getAll.and.returnValue(of([]));
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.users).toEqual([]);
      expect(component.displayedColumns).toEqual(['firstName', 'lastName', 'username', 'twoFactor', 'actions']);
    });

    it('should load users on init', () => {
      const mockUsers: User[] = [
        { id: 1, firstName: 'John', lastName: 'Doe', username: 'john.doe' } as User,
        { id: 2, firstName: 'Jane', lastName: 'Smith', username: 'jane.smith' } as User,
      ];

      mockAccountService.getAll.and.returnValue(of(mockUsers));
      fixture.detectChanges();

      expect(mockAccountService.getAll).toHaveBeenCalled();
      expect(component.users).toEqual(mockUsers);
    });

    it('should handle error when loading users', () => {
      spyOn(console, 'error');
      mockAccountService.getAll.and.returnValue(throwError(() => new Error('Load failed')));

      fixture.detectChanges();

      expect(console.error).toHaveBeenCalledWith('Error loading users:', jasmine.any(Error));
    });
  });

  describe('editUser', () => {
    it('should open dialog with correct data', () => {
      const user: User = { id: 1, firstName: 'John', lastName: 'Doe', username: 'john.doe' } as User;
      const dialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
      dialogRef.afterClosed.and.returnValue(of(null));

      mockDialog.open.and.returnValue(dialogRef);
      mockAccountService.getAll.and.returnValue(of([]));

      component.editUser(user);

      expect(mockDialog.open).toHaveBeenCalledWith(jasmine.any(Function), {
        width: '600px',
        maxWidth: '90vw',
        data: { userId: user.id },
        disableClose: true,
      });
    });

    it('should refresh users list when dialog returns result', () => {
      const user: User = { id: 1, firstName: 'John', lastName: 'Doe', username: 'john.doe' } as User;
      const dialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
      dialogRef.afterClosed.and.returnValue(of(true));

      mockDialog.open.and.returnValue(dialogRef);
      mockAccountService.getAll.and.returnValue(of([]));
      spyOn(component, 'ngOnInit');

      component.editUser(user);

      expect(component.ngOnInit).toHaveBeenCalled();
    });

    it('should not refresh users list when dialog returns no result', () => {
      const user: User = { id: 1, firstName: 'John', lastName: 'Doe', username: 'john.doe' } as User;
      const dialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
      dialogRef.afterClosed.and.returnValue(of(null));

      mockDialog.open.and.returnValue(dialogRef);
      mockAccountService.getAll.and.returnValue(of([]));
      spyOn(component, 'ngOnInit');

      component.editUser(user);

      expect(component.ngOnInit).not.toHaveBeenCalled();
    });
  });

  describe('manage2FA', () => {
    it('should open dialog with correct data', () => {
      const user: User = { id: 1, firstName: 'John', lastName: 'Doe', username: 'john.doe' } as User;
      const dialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
      dialogRef.afterClosed.and.returnValue(of(null));

      mockDialog.open.and.returnValue(dialogRef);

      component.manage2FA(user);

      expect(mockDialog.open).toHaveBeenCalledWith(jasmine.any(Function), {
        width: '600px',
        maxWidth: '90vw',
        maxHeight: '90vh',
        data: { userId: user.id, userName: user.username },
        disableClose: true,
      });
    });
  });

  describe('addUser', () => {
    it('should open dialog with undefined userId', () => {
      const dialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
      dialogRef.afterClosed.and.returnValue(of(null));

      mockDialog.open.and.returnValue(dialogRef);

      component.addUser();

      expect(mockDialog.open).toHaveBeenCalledWith(jasmine.any(Function), {
        width: '600px',
        maxWidth: '90vw',
        data: { userId: undefined },
        disableClose: true,
      });
    });

    it('should refresh users list when dialog returns result', () => {
      const dialogRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
      dialogRef.afterClosed.and.returnValue(of(true));

      mockDialog.open.and.returnValue(dialogRef);
      mockAccountService.getAll.and.returnValue(of([]));
      spyOn(component, 'ngOnInit');

      component.addUser();

      expect(component.ngOnInit).toHaveBeenCalled();
    });
  });

  describe('deleteUser', () => {
    beforeEach(() => {
      const mockUsers: User[] = [
        { id: 1, firstName: 'John', lastName: 'Doe', username: 'john.doe', isDeleting: false } as User,
        { id: 2, firstName: 'Jane', lastName: 'Smith', username: 'jane.smith', isDeleting: false } as User,
      ];
      component.users = mockUsers;
    });

    it('should delete user successfully', fakeAsync(() => {
      mockAccountService.delete.and.returnValue(of({}));

      component.deleteUser(1);
      tick();

      expect(mockAccountService.delete).toHaveBeenCalledWith(1);
      expect(component.users.find(x => x.id === 1)).toBeUndefined();
    }));

    it('should remove user from list after successful deletion', fakeAsync(() => {
      mockAccountService.delete.and.returnValue(of({}));

      component.deleteUser(1);
      tick();

      expect(component.users.length).toBe(1);
      expect(component.users.find(x => x.id === 1)).toBeUndefined();
      expect(component.users.find(x => x.id === 2)).toBeDefined();
    }));

    it('should not delete user if user not found', () => {
      mockAccountService.delete.and.returnValue(of({}));

      component.deleteUser(999);

      expect(mockAccountService.delete).not.toHaveBeenCalled();
    });


  });
});
