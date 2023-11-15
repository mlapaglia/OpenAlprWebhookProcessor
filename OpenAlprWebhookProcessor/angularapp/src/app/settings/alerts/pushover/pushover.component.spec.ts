import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PushoverComponent } from './pushover.component';

describe('PushoverComponent', () => {
  let component: PushoverComponent;
  let fixture: ComponentFixture<PushoverComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [PushoverComponent]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PushoverComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
