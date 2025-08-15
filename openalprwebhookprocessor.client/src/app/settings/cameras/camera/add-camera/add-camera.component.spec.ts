import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';

import { AddCameraComponent } from './add-camera.component';

describe('AddCameraComponent', () => {
  let component: AddCameraComponent;
  let fixture: ComponentFixture<AddCameraComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddCameraComponent],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AddCameraComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display add new camera title', () => {
    const titleElement = fixture.debugElement.query(By.css('mat-card-title'));
    expect(titleElement.nativeElement.textContent.trim()).toBe('Add new camera');
  });

  it('should display add icon', () => {
    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    expect(iconElement).toBeTruthy();
    expect(iconElement.nativeElement.textContent.trim()).toBe('add_box');
  });

  it('should emit add event when icon is clicked', () => {
    spyOn(component.add, 'emit');

    const iconElement = fixture.debugElement.query(By.css('mat-icon'));
    iconElement.nativeElement.click();

    expect(component.add.emit).toHaveBeenCalledWith();
  });

  it('should emit add event when addCamera method is called', () => {
    spyOn(component.add, 'emit');

    component.addCamera();

    expect(component.add.emit).toHaveBeenCalledWith();
  });
});
