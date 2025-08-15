import { Injectable } from '@angular/core';

export interface VehicleMakeInfo {
  logo: string;
  displayName: string;
}

@Injectable({
  providedIn: 'root',
})
export class VehicleLogoService {
  // Aliases and special cases for make names
  private readonly makeAliases: Record<string, string> = {
    'chevy': 'chevrolet',
    'vw': 'volkswagen',
    'mercedes': 'mercedes-benz',
    'rover': 'land-rover',
    'benz': 'mercedes-benz',
  };

  // Special display names that don't follow standard capitalization
  private readonly specialDisplayNames: Record<string, string> = {
    'bmw': 'BMW',
    'gmc': 'GMC',
    'ram': 'RAM',
    'mercedes-benz': 'Mercedes-Benz',
    'land-rover': 'Land Rover',
    'rolls-royce': 'Rolls-Royce',
    'aston-martin': 'Aston Martin',
    'general-motors': 'General Motors',
  };

  /**
   * Extracts the vehicle make from a vehicle description string
   * @param vehicleDescription - The full vehicle description (e.g., "2018 Chevrolet Silverado")
   * @returns The normalized make or null if not found
   */
  extractMake(vehicleDescription: string): string | null {
    if (!vehicleDescription) {
      return null;
    }

    const description = vehicleDescription.toLowerCase().trim();
    const words = description.split(/\s+/);

    // Try aliases first
    const aliasMatch = this.findAliasMatch(words);
    if (aliasMatch) return aliasMatch;

    // Try compound aliases
    const compoundAliasMatch = this.findCompoundAliasMatch(words);
    if (compoundAliasMatch) return compoundAliasMatch;

    // Look for likely make names
    const makeMatch = this.findLikelyMake(words);
    if (makeMatch) return makeMatch;

    // Try known compound patterns
    return this.findKnownCompoundPattern(description);
  }

  private findAliasMatch(words: string[]): string | null {
    for (const word of words) {
      const cleanWord = word.replace(/[^\w-]/g, '');
      if (this.makeAliases[cleanWord]) {
        return this.makeAliases[cleanWord];
      }
    }
    return null;
  }

  private findCompoundAliasMatch(words: string[]): string | null {
    for (let i = 0; i < words.length - 1; i++) {
      const compound = `${words[i]}-${words[i + 1]}`.replace(/[^\w-]/g, '');
      if (this.makeAliases[compound]) {
        return this.makeAliases[compound];
      }
    }
    return null;
  }

  private findLikelyMake(words: string[]): string | null {
    for (const word of words) {
      const cleanWord = word.replace(/[^\w-]/g, '');

      if (this.shouldSkipWord(cleanWord)) {
        continue;
      }

      return cleanWord;
    }
    return null;
  }

  private shouldSkipWord(word: string): boolean {
    // Skip years and year ranges
    if (/^\d{4}$/.test(word) || /^\d{4}-\d{4}$/.test(word)) return true;

    // Skip short words and numbers
    if (word.length < 3 || /^\d+$/.test(word)) return true;

    // Skip common model descriptors
    const modelDescriptors = ['sedan', 'suv', 'truck', 'coupe', 'wagon', 'hatchback', 'convertible', 'van', 'pickup'];
    return modelDescriptors.includes(word);
  }

  private findKnownCompoundPattern(description: string): string | null {
    const knownCompoundPatterns = ['land rover', 'mercedes benz', 'rolls royce', 'aston martin'];
    for (const pattern of knownCompoundPatterns) {
      if (description.includes(pattern)) {
        return pattern.replace(' ', '-');
      }
    }
    return null;
  }

  /**
   * Gets the logo URL for a vehicle make
   * @param make - The normalized make name
   * @returns The logo URL
   */
  getLogoUrl(make: string): string {
    const normalizedMake = make.toLowerCase().trim();
    return `assets/icons/vehicle-logos/${normalizedMake}.png`;
  }

  /**
   * Gets the display name for a vehicle make
   * @param make - The normalized make name
   * @returns The proper display name
   */
  getDisplayName(make: string): string {
    const normalizedMake = make.toLowerCase().trim();

    // Check special display names first
    if (this.specialDisplayNames[normalizedMake]) {
      return this.specialDisplayNames[normalizedMake];
    }

    // Generate display name by capitalizing each word
    return normalizedMake
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }

  /**
   * Gets vehicle make info from a vehicle description
   * @param vehicleDescription - The full vehicle description
   * @returns VehicleMakeInfo with logo and display name, or null if not found
   */
  getVehicleMakeInfo(vehicleDescription: string): VehicleMakeInfo | null {
    const make = this.extractMake(vehicleDescription);
    if (!make) {
      return null;
    }

    return {
      logo: this.getLogoUrl(make),
      displayName: this.getDisplayName(make),
    };
  }

  /**
   * Formats the vehicle description with proper make capitalization
   * @param vehicleDescription - The original vehicle description
   * @returns Formatted description with proper make name
   */
  formatVehicleDescription(vehicleDescription: string): string {
    if (!vehicleDescription) {
      return vehicleDescription;
    }

    const make = this.extractMake(vehicleDescription);
    if (!make) {
      return vehicleDescription;
    }

    const displayName = this.getDisplayName(make);

    // Replace the make in the description with the proper display name
    const regex = new RegExp(`\\b${make.replace('-', '[-\\s]?')}\\b`, 'gi');
    return vehicleDescription.replace(regex, displayName);
  }
}
