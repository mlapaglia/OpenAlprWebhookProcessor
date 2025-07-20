import { TestBed } from '@angular/core/testing'
import { ThemeStorage, DocsSiteTheme } from './theme-storage'

describe('ThemeStorage', () => {
  let service: ThemeStorage
  let mockLocalStorage: { [key: string]: string }

  const mockTheme: DocsSiteTheme = {
    name: 'test-theme',
    displayName: 'Test Theme',
    primary: '#123456',
    accent: '#654321',
    isDark: false,
    isDefault: false
  }

  const mockDarkTheme: DocsSiteTheme = {
    name: 'dark-theme',
    displayName: 'Dark Theme',
    primary: '#333333',
    accent: '#ffffff',
    isDark: true,
    isDefault: false
  }

      beforeEach(() => {
    // Simple localStorage mock that works with the actual implementation
    mockLocalStorage = {}

    // Store original localStorage
    const originalLocalStorage = window.localStorage

    // Create a simple mock that supports both property and method access
    const mockStorage = {
      getItem: (key: string) => mockLocalStorage[key] || null,
      setItem: (key: string, value: string) => { mockLocalStorage[key] = value },
      removeItem: (key: string) => { delete mockLocalStorage[key] },
      get length() { return Object.keys(mockLocalStorage).length },
      key: (index: number) => Object.keys(mockLocalStorage)[index] || null,
      clear: () => { mockLocalStorage = {} }
    }

    // Add property-style access for the storage key
    Object.defineProperty(mockStorage, ThemeStorage.storageKey, {
      get: () => mockLocalStorage[ThemeStorage.storageKey] || null,
      set: (value) => { if (value) mockLocalStorage[ThemeStorage.storageKey] = value },
      configurable: true
    })

    // Replace window.localStorage
    Object.defineProperty(window, 'localStorage', {
      value: mockStorage,
      writable: true,
      configurable: true
    })

    TestBed.configureTestingModule({
      providers: [ThemeStorage]
    })

    service = TestBed.inject(ThemeStorage)
  })

  afterEach(() => {
    // Clear mock localStorage
    mockLocalStorage = {}
  })

  describe('Service Creation', () => {
    it('should be created', () => {
      expect(service).toBeTruthy()
    })

    it('should have correct storage key', () => {
      expect(ThemeStorage.storageKey).toBe('docs-theme-storage-current-name')
    })

    it('should have onThemeUpdate event emitter', () => {
      expect(service.onThemeUpdate).toBeDefined()
      expect(typeof service.onThemeUpdate.emit).toBe('function')
      expect(typeof service.onThemeUpdate.subscribe).toBe('function')
    })
  })

  describe('storeTheme', () => {
    it('should store theme name in localStorage', () => {
      service.storeTheme(mockTheme)

      expect(mockLocalStorage[ThemeStorage.storageKey]).toBe(mockTheme.name)
    })

    it('should emit theme update event', () => {
      spyOn(service.onThemeUpdate, 'emit')

      service.storeTheme(mockTheme)

      expect(service.onThemeUpdate.emit).toHaveBeenCalledWith(mockTheme)
      expect(service.onThemeUpdate.emit).toHaveBeenCalledTimes(1)
    })

    it('should handle themes with different properties', () => {
      spyOn(service.onThemeUpdate, 'emit')

      service.storeTheme(mockDarkTheme)

      expect(mockLocalStorage[ThemeStorage.storageKey]).toBe(mockDarkTheme.name)
      expect(service.onThemeUpdate.emit).toHaveBeenCalledWith(mockDarkTheme)
    })

    it('should handle theme with minimal properties', () => {
      const minimalTheme: DocsSiteTheme = {
        name: 'minimal',
        primary: '#000',
        accent: '#fff'
      }

      service.storeTheme(minimalTheme)

      expect(mockLocalStorage[ThemeStorage.storageKey]).toBe(minimalTheme.name)
    })

    it('should overwrite existing stored theme', () => {
      const theme1: DocsSiteTheme = { name: 'theme1', primary: '#111', accent: '#222' }
      const theme2: DocsSiteTheme = { name: 'theme2', primary: '#333', accent: '#444' }

      service.storeTheme(theme1)
      expect(mockLocalStorage[ThemeStorage.storageKey]).toBe('theme1')

      service.storeTheme(theme2)
      expect(mockLocalStorage[ThemeStorage.storageKey]).toBe('theme2')
    })

    it('should handle localStorage errors gracefully', () => {
      // Simulate localStorage error
      Object.defineProperty(window, 'localStorage', {
        value: {
          get [ThemeStorage.storageKey]() { throw new Error('Storage error') },
          set [ThemeStorage.storageKey](value) { throw new Error('Storage error') }
        },
        writable: true
      })

      spyOn(service.onThemeUpdate, 'emit')

      expect(() => service.storeTheme(mockTheme)).not.toThrow()
      expect(service.onThemeUpdate.emit).toHaveBeenCalledWith(mockTheme)
    })
  })

  describe('getStoredThemeName', () => {
    it('should return stored theme name when it exists', () => {
      mockLocalStorage[ThemeStorage.storageKey] = 'stored-theme'

      const result = service.getStoredThemeName()

      expect(result).toBe('stored-theme')
    })

    it('should return null when no theme is stored', () => {
      const result = service.getStoredThemeName()

      expect(result).toBeNull()
    })

    it('should return null when localStorage is empty', () => {
      mockLocalStorage = {}

      const result = service.getStoredThemeName()

      expect(result).toBeNull()
    })

    it('should handle localStorage access errors gracefully', () => {
      // Simulate localStorage error
      Object.defineProperty(window, 'localStorage', {
        value: {
          get [ThemeStorage.storageKey]() { throw new Error('Storage error') }
        },
        writable: true
      })

      const result = service.getStoredThemeName()

      expect(result).toBeNull()
    })

    it('should handle undefined localStorage gracefully', () => {
      // Simulate missing localStorage
      Object.defineProperty(window, 'localStorage', {
        value: undefined,
        writable: true
      })

      expect(() => service.getStoredThemeName()).not.toThrow()
      const result = service.getStoredThemeName()
      expect(result).toBeNull()
    })
  })

  describe('clearStorage', () => {
    it('should remove theme from localStorage', () => {
      mockLocalStorage[ThemeStorage.storageKey] = 'theme-to-clear'

      service.clearStorage()

      expect(mockLocalStorage[ThemeStorage.storageKey]).toBeUndefined()
    })

    it('should handle clearing when no theme is stored', () => {
      expect(() => service.clearStorage()).not.toThrow()
      expect(mockLocalStorage[ThemeStorage.storageKey]).toBeUndefined()
    })

         it('should handle localStorage clear errors gracefully', () => {
       mockLocalStorage[ThemeStorage.storageKey] = 'theme-to-clear'

       // Replace removeItem with a function that throws
       const originalRemoveItem = window.localStorage.removeItem
       window.localStorage.removeItem = () => { throw new Error('Clear error') }

       expect(() => service.clearStorage()).not.toThrow()

       // Restore original
       window.localStorage.removeItem = originalRemoveItem
     })
  })

  describe('Event Management', () => {
    it('should allow subscribing to theme updates', () => {
      const subscriber = jasmine.createSpy('subscriber')

      service.onThemeUpdate.subscribe(subscriber)
      service.storeTheme(mockTheme)

      expect(subscriber).toHaveBeenCalledWith(mockTheme)
    })

    it('should allow multiple subscribers', () => {
      const subscriber1 = jasmine.createSpy('subscriber1')
      const subscriber2 = jasmine.createSpy('subscriber2')

      service.onThemeUpdate.subscribe(subscriber1)
      service.onThemeUpdate.subscribe(subscriber2)
      service.storeTheme(mockTheme)

      expect(subscriber1).toHaveBeenCalledWith(mockTheme)
      expect(subscriber2).toHaveBeenCalledWith(mockTheme)
    })

    it('should emit different themes to subscribers', () => {
      const subscriber = jasmine.createSpy('subscriber')

      service.onThemeUpdate.subscribe(subscriber)

      service.storeTheme(mockTheme)
      service.storeTheme(mockDarkTheme)

      expect(subscriber).toHaveBeenCalledTimes(2)
      expect(subscriber).toHaveBeenCalledWith(mockTheme)
      expect(subscriber).toHaveBeenCalledWith(mockDarkTheme)
    })
  })

  describe('Integration Tests', () => {
    it('should store and retrieve theme correctly', () => {
      service.storeTheme(mockTheme)

      const storedName = service.getStoredThemeName()

      expect(storedName).toBe(mockTheme.name)
    })

    it('should store, clear, and verify theme is removed', () => {
      service.storeTheme(mockTheme)
      expect(service.getStoredThemeName()).toBe(mockTheme.name)

      service.clearStorage()
      expect(service.getStoredThemeName()).toBeNull()
    })

    it('should handle complete workflow with events', () => {
      const subscriber = jasmine.createSpy('subscriber')
      service.onThemeUpdate.subscribe(subscriber)

      // Store theme
      service.storeTheme(mockTheme)
      expect(subscriber).toHaveBeenCalledWith(mockTheme)
      expect(service.getStoredThemeName()).toBe(mockTheme.name)

      // Store different theme
      service.storeTheme(mockDarkTheme)
      expect(subscriber).toHaveBeenCalledWith(mockDarkTheme)
      expect(service.getStoredThemeName()).toBe(mockDarkTheme.name)

      // Clear storage
      service.clearStorage()
      expect(service.getStoredThemeName()).toBeNull()

      expect(subscriber).toHaveBeenCalledTimes(2)
    })
  })

  describe('DocsSiteTheme Interface', () => {
    it('should handle theme with all optional properties', () => {
      const completeTheme: DocsSiteTheme = {
        name: 'complete-theme',
        displayName: 'Complete Theme',
        primary: '#primary',
        accent: '#accent',
        isDark: true,
        isDefault: true
      }

      service.storeTheme(completeTheme)

      expect(service.getStoredThemeName()).toBe(completeTheme.name)
    })

    it('should handle theme with only required properties', () => {
      const minimalTheme: DocsSiteTheme = {
        name: 'minimal-theme',
        primary: '#primary',
        accent: '#accent'
      }

      service.storeTheme(minimalTheme)

      expect(service.getStoredThemeName()).toBe(minimalTheme.name)
    })

    it('should handle theme names with special characters', () => {
      const specialTheme: DocsSiteTheme = {
        name: 'theme-with_special.chars-123',
        primary: '#000',
        accent: '#fff'
      }

      service.storeTheme(specialTheme)

      expect(service.getStoredThemeName()).toBe(specialTheme.name)
    })
  })
})
