import { TestBed } from '@angular/core/testing';
import { StyleManager } from './style-manager.component';

describe('StyleManager', () => {
  let service: StyleManager;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [StyleManager],
    });
    service = TestBed.inject(StyleManager);
  });

  afterEach(() => {
    // Clean up any link elements created during tests
    const linkElements = document.head.querySelectorAll('link[rel="stylesheet"]');
    linkElements.forEach(element => {
      if (element.classList.toString().includes('style-manager-')) {
        document.head.removeChild(element);
      }
    });
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('setStyle', () => {
    it('should create new link element when none exists', () => {
      const key = 'test-theme';
      const href = 'test-theme.css';

      service.setStyle(key, href);

      const linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement).toBeTruthy();
      expect(linkElement.tagName).toBe('LINK');
      expect(linkElement.getAttribute('rel')).toBe('stylesheet');
      expect(linkElement.getAttribute('href')).toBe(href);
      expect(linkElement.classList.contains(`style-manager-${key}`)).toBe(true);
    });

    it('should update existing link element href when it exists', () => {
      const key = 'existing-theme';
      const initialHref = 'initial-theme.css';
      const updatedHref = 'updated-theme.css';

      // First call creates the element
      service.setStyle(key, initialHref);
      let linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(initialHref);

      // Second call updates the same element
      service.setStyle(key, updatedHref);
      linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(updatedHref);

      // Should still be only one element with this key
      const allElements = document.head.querySelectorAll(`.style-manager-${key}`);
      expect(allElements.length).toBe(1);
    });

    it('should handle multiple different keys independently', () => {
      const key1 = 'theme1';
      const key2 = 'theme2';
      const href1 = 'theme1.css';
      const href2 = 'theme2.css';

      service.setStyle(key1, href1);
      service.setStyle(key2, href2);

      const linkElement1 = document.head.querySelector(`.style-manager-${key1}`) as HTMLLinkElement;
      const linkElement2 = document.head.querySelector(`.style-manager-${key2}`) as HTMLLinkElement;

      expect(linkElement1.getAttribute('href')).toBe(href1);
      expect(linkElement2.getAttribute('href')).toBe(href2);
      expect(linkElement1).not.toBe(linkElement2);
    });

    it('should handle special characters in key names', () => {
      const key = 'theme-with-special_chars-123';  // Use CSS-safe characters
      const href = 'special-theme.css';

      service.setStyle(key, href);

      const linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement).toBeTruthy();
      expect(linkElement.getAttribute('href')).toBe(href);
    });

    it('should handle empty href value', () => {
      const key = 'empty-href';
      const href = '';

      service.setStyle(key, href);

      const linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement).toBeTruthy();
      expect(linkElement.getAttribute('href')).toBe('');
    });
  });

  describe('removeStyle', () => {
    it('should remove existing link element', () => {
      const key = 'removable-theme';
      const href = 'removable-theme.css';

      // First create the element
      service.setStyle(key, href);
      let linkElement = document.head.querySelector(`.style-manager-${key}`);
      expect(linkElement).toBeTruthy();

      // Then remove it
      service.removeStyle(key);
      linkElement = document.head.querySelector(`.style-manager-${key}`);
      expect(linkElement).toBeFalsy();
    });

    it('should handle removing non-existent element gracefully', () => {
      const key = 'non-existent-theme';

      // Should not throw error
      expect(() => service.removeStyle(key)).not.toThrow();

      // Should still be no element
      const linkElement = document.head.querySelector(`.style-manager-${key}`);
      expect(linkElement).toBeFalsy();
    });

    it('should only remove element with specified key', () => {
      const key1 = 'keep-theme';
      const key2 = 'remove-theme';
      const href1 = 'keep-theme.css';
      const href2 = 'remove-theme.css';

      // Create both elements
      service.setStyle(key1, href1);
      service.setStyle(key2, href2);

      // Remove only one
      service.removeStyle(key2);

      // Check that only the specified one was removed
      const linkElement1 = document.head.querySelector(`.style-manager-${key1}`);
      const linkElement2 = document.head.querySelector(`.style-manager-${key2}`);

      expect(linkElement1).toBeTruthy();
      expect(linkElement2).toBeFalsy();
    });

    it('should handle removing the same element multiple times', () => {
      const key = 'double-remove-theme';
      const href = 'double-remove-theme.css';

      // Create element
      service.setStyle(key, href);

      // Remove it twice
      service.removeStyle(key);
      expect(() => service.removeStyle(key)).not.toThrow();

      // Should still be gone
      const linkElement = document.head.querySelector(`.style-manager-${key}`);
      expect(linkElement).toBeFalsy();
    });
  });

  describe('DOM Integration', () => {
    it('should add link elements to document head', () => {
      const initialLinkCount = document.head.querySelectorAll('link').length;
      const key = 'head-test';
      const href = 'head-test.css';

      service.setStyle(key, href);

      const currentLinkCount = document.head.querySelectorAll('link').length;
      expect(currentLinkCount).toBe(initialLinkCount + 1);

      const addedLink = document.head.querySelector(`.style-manager-${key}`);
      expect(addedLink?.parentElement).toBe(document.head);
    });

    it('should create proper link element structure', () => {
      const key = 'structure-test';
      const href = 'structure-test.css';

      service.setStyle(key, href);

      const linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;

      // Check all required attributes and properties
      expect(linkElement.tagName.toLowerCase()).toBe('link');
      expect(linkElement.getAttribute('rel')).toBe('stylesheet');
      expect(linkElement.getAttribute('href')).toBe(href);
      expect(linkElement.classList.contains(`style-manager-${key}`)).toBe(true);
    });

    it('should handle setting and removing styles in sequence', () => {
      const key = 'sequence-test';
      const href1 = 'sequence1.css';
      const href2 = 'sequence2.css';

      // Set initial style
      service.setStyle(key, href1);
      let linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(href1);

      // Update style
      service.setStyle(key, href2);
      linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(href2);

      // Remove style
      service.removeStyle(key);
      linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement).toBeFalsy();

      // Set again after removal
      service.setStyle(key, href1);
      linkElement = document.head.querySelector(`.style-manager-${key}`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(href1);
    });
  });

  describe('Class Name Generation', () => {
    it('should generate consistent class names for same key', () => {
      const key = 'consistent-key';
      const href = 'consistent.css';

      service.setStyle(key, href);
      const linkElement1 = document.head.querySelector(`.style-manager-${key}`);

      service.removeStyle(key);
      service.setStyle(key, href);
      const linkElement2 = document.head.querySelector(`.style-manager-${key}`);

      expect(linkElement1?.className).toBe(linkElement2?.className);
    });

    it('should generate different class names for different keys', () => {
      const key1 = 'key1';
      const key2 = 'key2';
      const href = 'test.css';

      service.setStyle(key1, href);
      service.setStyle(key2, href);

      const linkElement1 = document.head.querySelector(`.style-manager-${key1}`);
      const linkElement2 = document.head.querySelector(`.style-manager-${key2}`);

      expect(linkElement1?.className).not.toBe(linkElement2?.className);
      expect(linkElement1?.classList.contains(`style-manager-${key1}`)).toBe(true);
      expect(linkElement2?.classList.contains(`style-manager-${key2}`)).toBe(true);
    });
  });

  describe('Error Handling', () => {
    it('should handle null or undefined keys gracefully', () => {
      const href = 'test.css';

      expect(() => service.setStyle(null as any, href)).not.toThrow();
      expect(() => service.setStyle(undefined as any, href)).not.toThrow();
      expect(() => service.removeStyle(null as any)).not.toThrow();
      expect(() => service.removeStyle(undefined as any)).not.toThrow();
    });

    it('should handle null or undefined href gracefully', () => {
      const key = 'null-href-test';

      expect(() => service.setStyle(key, null as any)).not.toThrow();
      expect(() => service.setStyle(key, undefined as any)).not.toThrow();
    });
  });
});
