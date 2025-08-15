import { TestBed } from '@angular/core/testing';

import { LocalStorageService } from './local-storage.service';

describe('LocalStorageService', () => {
  let service: LocalStorageService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LocalStorageService);

    // Clear localStorage before each test
    localStorage.clear();
  });

  afterEach(() => {
    // Clean up localStorage after each test
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('setData', () => {
    it('should store string data in localStorage', () => {
      const key = 'testKey';
      const data = 'testValue';

      service.setData(key, data);

      const storedValue = localStorage.getItem(key);
      expect(storedValue).toBe(JSON.stringify(data));
    });

    it('should store number data in localStorage', () => {
      const key = 'numberKey';
      const data = 42;

      service.setData(key, data);

      const storedValue = localStorage.getItem(key);
      expect(storedValue).toBe(JSON.stringify(data));
    });

    it('should overwrite existing data', () => {
      const key = 'overwriteKey';
      const initialData = 'initial';
      const newData = 'updated';

      service.setData(key, initialData);
      service.setData(key, newData);

      const storedValue = localStorage.getItem(key);
      expect(storedValue).toBe(JSON.stringify(newData));
    });
  });

  describe('getData', () => {
    it('should retrieve stored data from localStorage', () => {
      const key = 'retrieveKey';
      const data = 'retrieveValue';
      localStorage.setItem(key, JSON.stringify(data));

      const result = service.getData(key);

      expect(result).toBe(JSON.stringify(data));
    });

    it('should return empty string when key does not exist', () => {
      const result = service.getData('nonExistentKey');

      expect(result).toBe('');
    });

    it('should return empty string when localStorage returns null', () => {
      spyOn(localStorage, 'getItem').and.returnValue(null);

      const result = service.getData('testKey');

      expect(result).toBe('');
    });
  });

  describe('removeData', () => {
    it('should remove data from localStorage', () => {
      const key = 'removeKey';
      const data = 'removeValue';
      localStorage.setItem(key, JSON.stringify(data));

      service.removeData(key);

      const result = localStorage.getItem(key);
      expect(result).toBeNull();
    });

    it('should not throw error when removing non-existent key', () => {
      expect(() => service.removeData('nonExistentKey')).not.toThrow();
    });
  });
});
