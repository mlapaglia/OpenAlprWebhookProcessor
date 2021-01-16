import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OpenalprAgentComponent } from './openalpr-agent.component';

describe('OpenalprAgentComponent', () => {
  let component: OpenalprAgentComponent;
  let fixture: ComponentFixture<OpenalprAgentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OpenalprAgentComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OpenalprAgentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
