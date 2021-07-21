import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EnrichersComponent } from './enrichers.component';

describe('EnrichersComponent', () => {
  let component: EnrichersComponent;
  let fixture: ComponentFixture<EnrichersComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EnrichersComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EnrichersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
