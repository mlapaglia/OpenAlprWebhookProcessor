import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ForwardsComponent } from './forwards.component';

describe('ForwardsComponent', () => {
  let component: ForwardsComponent;
  let fixture: ComponentFixture<ForwardsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ForwardsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ForwardsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
