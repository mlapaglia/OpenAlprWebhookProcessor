import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CameraMaskComponent } from './camera-mask.component';

describe('CameraMaskComponent', () => {
  let component: CameraMaskComponent;
  let fixture: ComponentFixture<CameraMaskComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [CameraMaskComponent]
});
    fixture = TestBed.createComponent(CameraMaskComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
