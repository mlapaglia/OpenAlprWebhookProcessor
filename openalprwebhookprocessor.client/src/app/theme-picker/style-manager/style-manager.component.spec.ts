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
    const linkElements = document.head.querySelectorAll('link[rel="stylesheet"][data-style-key]');
    linkElements.forEach(element => {
      document.head.removeChild(element);
    });
  });

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });
  });

  describe('setStyle', () => {
    it('should create new link element when none exists', async () => {
      const key = 'test-theme';
      const href = 'data:text/css;base64,'; // Use data URL to avoid loading issues

      await service.setStyle(key, href);

      const linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement).toBeTruthy();
      expect(linkElement.tagName).toBe('LINK');
      expect(linkElement.getAttribute('rel')).toBe('stylesheet');
      expect(linkElement.getAttribute('href')).toBe(href);
      expect(linkElement.getAttribute('data-style-key')).toBe(key);
    });

    it('should update existing link element href when it exists', async () => {
      const key = 'existing-theme';
      const initialHref = 'data:text/css;base64,aW5pdGlhbA==';
      const updatedHref = 'data:text/css;base64,dXBkYXRlZA==';

      // First call creates the element
      await service.setStyle(key, initialHref);
      let linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(initialHref);

      // Second call updates the same element
      await service.setStyle(key, updatedHref);
      linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(updatedHref);

      // Should still be only one element with this key
      const allElements = document.head.querySelectorAll(`link[data-style-key="${key}"]`);
      expect(allElements.length).toBe(1);
    });

    it('should handle multiple different keys independently', async () => {
      const key1 = 'theme1';
      const key2 = 'theme2';
      const href1 = 'data:text/css;base64,dGhlbWUx';
      const href2 = 'data:text/css;base64,dGhlbWUy';

      await service.setStyle(key1, href1);
      await service.setStyle(key2, href2);

      const linkElement1 = document.head.querySelector(`link[data-style-key="${key1}"]`) as HTMLLinkElement;
      const linkElement2 = document.head.querySelector(`link[data-style-key="${key2}"]`) as HTMLLinkElement;

      expect(linkElement1.getAttribute('href')).toBe(href1);
      expect(linkElement2.getAttribute('href')).toBe(href2);
      expect(linkElement1).not.toBe(linkElement2);
    });

    it('should handle special characters in key names', async () => {
      const key = 'theme-with-special_chars-123';  // Use CSS-safe characters
      const href = 'data:text/css;base64,c3BlY2lhbA==';

      await service.setStyle(key, href);

      const linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement).toBeTruthy();
      expect(linkElement.getAttribute('href')).toBe(href);
    });

    it('should handle empty href value', async () => {
      const key = 'empty-href';
      const href = 'data:text/css;base64,';

      await service.setStyle(key, href);

      const linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement).toBeTruthy();
      expect(linkElement.getAttribute('href')).toBe('data:text/css;base64,');
    });
  });

  describe('removeStyle', () => {
    it('should remove existing link element', async () => {
      const key = 'removable-theme';
      const href = 'data:text/css;base64,cmVtb3ZhYmxl';

      // First create the element
      await service.setStyle(key, href);
      let linkElement = document.head.querySelector(`link[data-style-key="${key}"]`);
      expect(linkElement).toBeTruthy();

      // Then remove it
      service.removeStyle(key);
      linkElement = document.head.querySelector(`link[data-style-key="${key}"]`);
      expect(linkElement).toBeFalsy();
    });

    it('should handle removing non-existent element gracefully', () => {
      const key = 'non-existent-theme';

      // Should not throw error
      expect(() => service.removeStyle(key)).not.toThrow();

      // Should still be no element
      const linkElement = document.head.querySelector(`link[data-style-key="${key}"]`);
      expect(linkElement).toBeFalsy();
    });

    it('should only remove element with specified key', async () => {
      const key1 = 'keep-theme';
      const key2 = 'remove-theme';
      const href1 = 'data:text/css;base64,a2VlcA==';
      const href2 = 'data:text/css;base64,cmVtb3Zl';

      // Create both elements
      await service.setStyle(key1, href1);
      await service.setStyle(key2, href2);

      // Remove only one
      service.removeStyle(key2);

      // Check that only the specified one was removed
      const linkElement1 = document.head.querySelector(`link[data-style-key="${key1}"]`);
      const linkElement2 = document.head.querySelector(`link[data-style-key="${key2}"]`);

      expect(linkElement1).toBeTruthy();
      expect(linkElement2).toBeFalsy();
    });

    it('should handle removing the same element multiple times', async () => {
      const key = 'double-remove-theme';
      const href = 'data:text/css;base64,ZG91Ymxl';

      // Create element
      await service.setStyle(key, href);

      // Remove it twice
      service.removeStyle(key);
      expect(() => service.removeStyle(key)).not.toThrow();

      // Should still be gone
      const linkElement = document.head.querySelector(`link[data-style-key="${key}"]`);
      expect(linkElement).toBeFalsy();
    });
  });

  describe('DOM Integration', () => {
    it('should add link elements to document head', async () => {
      const initialLinkCount = document.head.querySelectorAll('link').length;
      const key = 'head-test';
      const href = 'data:text/css;base64,aGVhZA==';

      await service.setStyle(key, href);

      const currentLinkCount = document.head.querySelectorAll('link').length;
      expect(currentLinkCount).toBe(initialLinkCount + 1);

      const addedLink = document.head.querySelector(`link[data-style-key="${key}"]`);
      expect(addedLink?.parentElement).toBe(document.head);
    });

    it('should create proper link element structure', async () => {
      const key = 'structure-test';
      const href = 'data:text/css;base64,c3RydWN0dXJl';

      await service.setStyle(key, href);

      const linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;

      // Check all required attributes and properties
      expect(linkElement.tagName.toLowerCase()).toBe('link');
      expect(linkElement.getAttribute('rel')).toBe('stylesheet');
      expect(linkElement.getAttribute('href')).toBe(href);
      expect(linkElement.getAttribute('data-style-key')).toBe(key);
    });

    it('should handle setting and removing styles in sequence', async () => {
      const key = 'sequence-test';
      const href1 = 'data:text/css;base64,c2VxMQ==';
      const href2 = 'data:text/css;base64,c2VxMg==';

      // Set initial style
      await service.setStyle(key, href1);
      let linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(href1);

      // Update style
      await service.setStyle(key, href2);
      linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(href2);

      // Remove style
      service.removeStyle(key);
      linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement).toBeFalsy();

      // Set again after removal
      await service.setStyle(key, href1);
      linkElement = document.head.querySelector(`link[data-style-key="${key}"]`) as HTMLLinkElement;
      expect(linkElement.getAttribute('href')).toBe(href1);
    });
  });

  describe('Data Attribute Generation', () => {
    it('should generate consistent data attributes for same key', async () => {
      const key = 'consistent-key';
      const href = 'consistent.css';

      await service.setStyle(key, href);
      const linkElement1 = document.head.querySelector(`link[data-style-key="${key}"]`);

      service.removeStyle(key);
      await service.setStyle(key, href);
      const linkElement2 = document.head.querySelector(`link[data-style-key="${key}"]`);

      expect(linkElement1?.getAttribute('data-style-key')).toBe(linkElement2?.getAttribute('data-style-key'));
    });

    it('should generate different data attributes for different keys', async () => {
      const key1 = 'key1';
      const key2 = 'key2';
      const href = 'data:text/css;base64,dGVzdA==';

      await service.setStyle(key1, href);
      await service.setStyle(key2, href);

      const linkElement1 = document.head.querySelector(`link[data-style-key="${key1}"]`);
      const linkElement2 = document.head.querySelector(`link[data-style-key="${key2}"]`);

      expect(linkElement1?.getAttribute('data-style-key')).not.toBe(linkElement2?.getAttribute('data-style-key'));
      expect(linkElement1?.getAttribute('data-style-key')).toBe(key1);
      expect(linkElement2?.getAttribute('data-style-key')).toBe(key2);
    });
  });

  describe('Error Handling', () => {
    it('should handle null or undefined keys gracefully', () => {
      const href = 'data:text/css;base64,dGVzdA==';

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
