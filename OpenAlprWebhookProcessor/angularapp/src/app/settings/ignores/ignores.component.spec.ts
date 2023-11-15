import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IgnoresComponent } from './ignores.component';

describe('IgnoresComponent', () => {
  let component: IgnoresComponent;
  let fixture: ComponentFixture<IgnoresComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [IgnoresComponent]
})
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(IgnoresComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
