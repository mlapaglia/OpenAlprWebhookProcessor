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
      const mockLink = {
        href: '',
        download: '',
        click: jasmine.createSpy('click'),
      } as any;

      const createElementSpy = spyOn(document, 'createElement').and.returnValue(mockLink);
      spyOn(document.body, 'appendChild').and.stub();
      spyOn(document.body, 'removeChild').and.stub();
      spyOn(window.URL, 'createObjectURL').and.returnValue('mock-url');
      spyOn(window.URL, 'revokeObjectURL').and.stub();

      component.downloadCodes();

      expect(createElementSpy).toHaveBeenCalledWith('a');
      expect(mockLink.download).toBe('recovery-codes.txt');
      expect(mockLink.click).toHaveBeenCalled();
      expect(document.body.appendChild).toHaveBeenCalledWith(mockLink);
      expect(document.body.removeChild).toHaveBeenCalledWith(mockLink);
    });
  });

  describe('print codes', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('recoveryCodes', ['code1', 'code2', 'code3']);
    });

    it('should open print window', () => {
      const mockElements = {
        html: { appendChild: jasmine.createSpy('appendChild') },
        head: { appendChild: jasmine.createSpy('appendChild') },
        title: { textContent: '' },
        body: { appendChild: jasmine.createSpy('appendChild') },
        h2: { textContent: '' },
        p: { textContent: '' },
        ul: { appendChild: jasmine.createSpy('appendChild') },
        li: { textContent: '' },
      };

      const mockWindow = {
        document: {
          createElement: jasmine.createSpy('createElement').and.callFake((tag: string) => {
            switch (tag) {
              case 'html': return mockElements.html;
              case 'head': return mockElements.head;
              case 'title': return mockElements.title;
              case 'body': return mockElements.body;
              case 'h2': return mockElements.h2;
              case 'p': return mockElements.p;
              case 'ul': return mockElements.ul;
              case 'li': return { ...mockElements.li };
              default: return {};
            }
          }),
          appendChild: jasmine.createSpy('appendChild'),
        },
        print: jasmine.createSpy('print'),
      };

      spyOn(window, 'open').and.returnValue(mockWindow as any);

      component.printCodes();

      expect(window.open).toHaveBeenCalledWith('', '_blank');
      expect(mockWindow.document.createElement).toHaveBeenCalledWith('html');
      expect(mockWindow.document.createElement).toHaveBeenCalledWith('head');
      expect(mockWindow.document.createElement).toHaveBeenCalledWith('title');
      expect(mockWindow.document.createElement).toHaveBeenCalledWith('body');
      expect(mockWindow.print).toHaveBeenCalled();
    });

    it('should handle null window', () => {
      spyOn(window, 'open').and.returnValue(null);

      expect(() => component.printCodes()).not.toThrow();
    });
  });
});
