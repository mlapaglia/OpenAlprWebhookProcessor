import { TestBed } from '@angular/core/testing';
import { VehicleLogoService } from './vehicle-logo.service';

describe('VehicleLogoService', () => {
  let service: VehicleLogoService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VehicleLogoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('extractMake', () => {
    it('should return null for empty or null input', () => {
      expect(service.extractMake('')).toBeNull();
      expect(service.extractMake(null as any)).toBeNull();
      expect(service.extractMake(undefined as any)).toBeNull();
    });

    it('should extract basic make names', () => {
      expect(service.extractMake('2018 Toyota Camry')).toBe('toyota');
      expect(service.extractMake('2020 Honda Civic')).toBe('honda');
      expect(service.extractMake('Ford F-150')).toBe('ford');
    });

    it('should handle aliases', () => {
      expect(service.extractMake('2018 Chevy Silverado')).toBe('chevrolet');
      expect(service.extractMake('2020 VW Golf')).toBe('volkswagen');
      expect(service.extractMake('Mercedes S-Class')).toBe('mercedes-benz');
      expect(service.extractMake('Rover Discovery')).toBe('land-rover');
      expect(service.extractMake('Benz C-Class')).toBe('mercedes-benz');
    });

    it('should handle compound makes', () => {
      expect(service.extractMake('2018 Land Rover Discovery')).toBe('land-rover');
      expect(service.extractMake('Mercedes Benz S-Class')).toBe('mercedes-benz');
      expect(service.extractMake('Rolls Royce Phantom')).toBe('rolls');
      expect(service.extractMake('Aston Martin DB11')).toBe('aston');
    });

    it('should skip years and model descriptors', () => {
      expect(service.extractMake('2018 2019 Toyota Camry')).toBe('toyota');
      expect(service.extractMake('2018-2020 Honda Civic')).toBe('honda');
      expect(service.extractMake('Sedan Toyota Camry')).toBe('toyota');
      expect(service.extractMake('SUV Honda Pilot')).toBe('honda');
      expect(service.extractMake('Truck Ford F-150')).toBe('ford');
    });

    it('should handle special characters and spacing', () => {
      expect(service.extractMake('  2018   Toyota   Camry  ')).toBe('toyota');
      expect(service.extractMake('2018, Toyota Camry')).toBe('toyota');
      expect(service.extractMake('2018 (Toyota) Camry')).toBe('toyota');
    });

    it('should return null for unrecognizable descriptions', () => {
      expect(service.extractMake('2018 Unknown Vehicle')).toBe('unknown');
      expect(service.extractMake('123 456 789')).toBeNull();
      expect(service.extractMake('Sedan SUV Truck')).toBeNull();
    });
  });

  describe('getLogoUrl', () => {
    it('should return correct logo URL', () => {
      expect(service.getLogoUrl('toyota')).toBe('assets/icons/vehicle-logos/toyota.png');
      expect(service.getLogoUrl('honda')).toBe('assets/icons/vehicle-logos/honda.png');
      expect(service.getLogoUrl('mercedes-benz')).toBe('assets/icons/vehicle-logos/mercedes-benz.png');
    });

    it('should normalize make names', () => {
      expect(service.getLogoUrl('  Toyota  ')).toBe('assets/icons/vehicle-logos/toyota.png');
      expect(service.getLogoUrl('HONDA')).toBe('assets/icons/vehicle-logos/honda.png');
    });
  });

  describe('getDisplayName', () => {
    it('should return special display names', () => {
      expect(service.getDisplayName('bmw')).toBe('BMW');
      expect(service.getDisplayName('gmc')).toBe('GMC');
      expect(service.getDisplayName('ram')).toBe('RAM');
      expect(service.getDisplayName('mercedes-benz')).toBe('Mercedes-Benz');
      expect(service.getDisplayName('land-rover')).toBe('Land Rover');
      expect(service.getDisplayName('rolls-royce')).toBe('Rolls-Royce');
      expect(service.getDisplayName('aston-martin')).toBe('Aston Martin');
      expect(service.getDisplayName('general-motors')).toBe('General Motors');
    });

    it('should capitalize regular make names', () => {
      expect(service.getDisplayName('toyota')).toBe('Toyota');
      expect(service.getDisplayName('honda')).toBe('Honda');
      expect(service.getDisplayName('ford')).toBe('Ford');
    });

    it('should handle compound makes', () => {
      expect(service.getDisplayName('some-compound')).toBe('Some Compound');
      expect(service.getDisplayName('multi-word-make')).toBe('Multi Word Make');
    });

    it('should normalize input', () => {
      expect(service.getDisplayName('  bmw  ')).toBe('BMW');
      expect(service.getDisplayName('TOYOTA')).toBe('Toyota');
    });
  });

  describe('getVehicleMakeInfo', () => {
    it('should return null for invalid descriptions', () => {
      expect(service.getVehicleMakeInfo('')).toBeNull();
      expect(service.getVehicleMakeInfo(null as any)).toBeNull();
      expect(service.getVehicleMakeInfo('123 456 789')).toBeNull();
    });

    it('should return complete make info', () => {
      const result = service.getVehicleMakeInfo('2018 Toyota Camry');
      expect(result).not.toBeNull();
      expect(result!.logo).toBe('assets/icons/vehicle-logos/toyota.png');
      expect(result!.displayName).toBe('Toyota');
    });

    it('should handle special makes', () => {
      const result = service.getVehicleMakeInfo('2018 BMW X5');
      expect(result).not.toBeNull();
      expect(result!.logo).toBe('assets/icons/vehicle-logos/bmw.png');
      expect(result!.displayName).toBe('BMW');
    });

    it('should handle aliases', () => {
      const result = service.getVehicleMakeInfo('2018 Chevy Silverado');
      expect(result).not.toBeNull();
      expect(result!.logo).toBe('assets/icons/vehicle-logos/chevrolet.png');
      expect(result!.displayName).toBe('Chevrolet');
    });

    it('should handle compound makes', () => {
      const result = service.getVehicleMakeInfo('2018 Land Rover Discovery');
      expect(result).not.toBeNull();
      expect(result!.logo).toBe('assets/icons/vehicle-logos/land-rover.png');
      expect(result!.displayName).toBe('Land Rover');
    });
  });

  describe('formatVehicleDescription', () => {
    it('should return original description for empty input', () => {
      expect(service.formatVehicleDescription('')).toBe('');
      expect(service.formatVehicleDescription(null as any)).toBeNull();
    });

    it('should return original description when no make is found', () => {
      const description = '2018 Unknown Vehicle';
      expect(service.formatVehicleDescription(description)).toBe(description);
    });

    it('should format basic make names', () => {
      expect(service.formatVehicleDescription('2018 toyota camry')).toBe('2018 Toyota camry');
      expect(service.formatVehicleDescription('2020 honda civic')).toBe('2020 Honda civic');
    });

    it('should format special make names', () => {
      expect(service.formatVehicleDescription('2018 bmw x5')).toBe('2018 BMW x5');
      expect(service.formatVehicleDescription('2020 gmc sierra')).toBe('2020 GMC sierra');
    });

    it('should format basic makes', () => {
      expect(service.formatVehicleDescription('2018 toyota camry')).toBe('2018 Toyota camry');
      expect(service.formatVehicleDescription('2020 honda civic')).toBe('2020 Honda civic');
    });

    it('should format compound makes', () => {
      expect(service.formatVehicleDescription('2018 land rover discovery')).toBe('2018 Land Rover discovery');
      expect(service.formatVehicleDescription('2020 mercedes benz s-class')).toBe('2020 Mercedes-Benz s-class');
    });

    it('should handle case variations', () => {
      expect(service.formatVehicleDescription('2018 TOYOTA CAMRY')).toBe('2018 Toyota CAMRY');
      expect(service.formatVehicleDescription('2018 Toyota CAMRY')).toBe('2018 Toyota CAMRY');
    });

    it('should handle hyphenated makes in description', () => {
      expect(service.formatVehicleDescription('2018 land-rover discovery')).toBe('2018 Land Rover discovery');
      expect(service.formatVehicleDescription('2018 mercedes-benz s-class')).toBe('2018 Mercedes-Benz s-class');
    });
  });
});
