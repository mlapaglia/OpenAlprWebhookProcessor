import { TestBed, type ComponentFixture } from '@angular/core/testing';

import { IgnoresComponent } from './ignores.component';
import { IgnoresService } from './ignores.service';
import { SnackbarService } from 'app/snackbar/snackbar.service';
import type { Ignore } from './ignore';
import { of } from 'rxjs';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe(IgnoresComponent.name, () => {
  let component: IgnoresComponent;
  let fixture: ComponentFixture<IgnoresComponent>;
  const ignoresServiceSpy = jasmine.createSpyObj(IgnoresService.name, ['getIgnores']);
  const snackbarServiceSpy = jasmine.createSpyObj(SnackbarService.name, ['create']);

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IgnoresComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: IgnoresService, useValue: ignoresServiceSpy },
        { provide: SnackbarService, useValue: snackbarServiceSpy },
      ],
    })
      .compileComponents();
  });

  beforeEach(() => {
    const ignores: Ignore[] = [];
    ignoresServiceSpy.getIgnores.and.returnValue(of(ignores));

    fixture = TestBed.createComponent(IgnoresComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    expect(ignoresServiceSpy.getIgnores).toHaveBeenCalled();
  });
});
