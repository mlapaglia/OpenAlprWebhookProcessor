export interface VehicleFilters {
  cameras?: string[]
  
  vehicleMakes?: string[]

  vehicleModels?: string[]

  vehicleMakeModelMap?: { [make: string]: string[] }

  vehicleTypes?: string[]

  vehicleYears?: string[]

  vehicleColors?: string[]

  vehicleRegions?: string[]
}
