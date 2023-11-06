import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WebpushComponent } from './webpush.component';

describe('WebpushComponent', () => {
  let component: WebpushComponent;
  let fixture: ComponentFixture<WebpushComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [WebpushComponent]
    });
    fixture = TestBed.createComponent(WebpushComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
