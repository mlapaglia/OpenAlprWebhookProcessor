import { TestBed, type ComponentFixture } from '@angular/core/testing';

import { RecoveryCodesDisplayComponent } from './recovery-codes-display.component';

describe('RecoveryCodesDisplayComponent', () => {
  let component: RecoveryCodesDisplayComponent;
  let fixture: ComponentFixture<RecoveryCodesDisplayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RecoveryCodesDisplayComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(RecoveryCodesDisplayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('component initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize with default values', () => {
      expect(component.recoveryCodes()).toEqual([]);
      expect(component.canGenerate()).toBe(false);
    });
  });

  describe('recovery codes display', () => {
    it('should display recovery codes when provided', () => {
      fixture.componentRef.setInput('recoveryCodes', ['code1', 'code2', 'code3']);
      fixture.detectChanges();

      expect(component.recoveryCodes().length).toBe(3);
    });

    it('should not display when no codes are provided', () => {
      fixture.componentRef.setInput('recoveryCodes', []);
      fixture.detectChanges();

      expect(component.recoveryCodes().length).toBe(0);
    });
  });

  describe('generate new codes', () => {
    it('should emit generateNewCodes event', () => {
      spyOn(component.generateNewCodes, 'emit');

      component.onGenerateNewCodes();

      expect(component.generateNewCodes.emit).toHaveBeenCalled();
    });
  });

  describe('download codes', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('recoveryCodes', ['code1', 'code2', 'code3']);
    });

    it('should create download link', () => {
      spyOn(document, 'createElement').and.callThrough();
      spyOn(document.body, 'appendChild').and.stub();
      spyOn(document.body, 'removeChild').and.stub();

      const mockLink = {
        href: '',
        download: '',
        click: jasmine.createSpy('click'),
      } as any;

      (document.createElement as jasmine.Spy).and.returnValue(mockLink);

      component.downloadCodes();

      expect(document.createElement).toHaveBeenCalledWith('a');
      expect(mockLink.download).toBe('recovery-codes.txt');
      expect(mockLink.click).toHaveBeenCalled();
    });
  });

  describe('print codes', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('recoveryCodes', ['code1', 'code2', 'code3']);
    });

    it('should open print window', () => {
      const mockWindow = {
        document: {
          write: jasmine.createSpy('write'),
          close: jasmine.createSpy('close'),
        },
        print: jasmine.createSpy('print'),
      };

      spyOn(window, 'open').and.returnValue(mockWindow as any);

      component.printCodes();

      expect(window.open).toHaveBeenCalledWith('', '_blank');
      expect(mockWindow.document.write).toHaveBeenCalled();
      expect(mockWindow.document.close).toHaveBeenCalled();
      expect(mockWindow.print).toHaveBeenCalled();
    });

    it('should handle null window', () => {
      spyOn(window, 'open').and.returnValue(null);

      expect(() => component.printCodes()).not.toThrow();
    });
  });
});
