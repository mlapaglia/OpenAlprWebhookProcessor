import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OpenalprWebserverComponent } from './openalpr-webserver.component';

describe('OpenalprWebserverComponent', () => {
  let component: OpenalprWebserverComponent;
  let fixture: ComponentFixture<OpenalprWebserverComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OpenalprWebserverComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OpenalprWebserverComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
