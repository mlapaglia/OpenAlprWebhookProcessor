import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FastSearchComponent } from './fast-search.component';

describe('FastSearchComponent', () => {
  let component: FastSearchComponent;
  let fixture: ComponentFixture<FastSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ FastSearchComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(FastSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
