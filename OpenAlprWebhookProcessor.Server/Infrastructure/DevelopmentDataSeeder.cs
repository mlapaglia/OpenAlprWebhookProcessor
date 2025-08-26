using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Users.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace OpenAlprWebhookProcessor.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class DevelopmentDataSeeder
    {
        private readonly ProcessorContext _processorContext;

        private readonly UserManager<ApplicationUser> _userManager;

        public DevelopmentDataSeeder(
            ProcessorContext processorContext,
            UserManager<ApplicationUser> userManager)
        {
            _processorContext = processorContext;
            _userManager = userManager;
        }

        public async Task SeedAsync()
        {
            await SeedUsersAsync();
            await SeedAgentsAsync();
            await SeedCamerasAsync();
            await SeedPlateGroupsAsync();
            await SeedAlertsAsync();
            await SeedIgnoresAsync();
        }

        private async Task SeedUsersAsync()
        {
            if (_userManager.Users.Any())
                return;

            var users = new[]
            {
                new { Username = "admin@example.com", Password = "Admin123!", FirstName = "Admin", LastName = "User" },
                new { Username = "developer@example.com", Password = "Dev123!", FirstName = "Developer", LastName = "User" },
                new { Username = "test@example.com", Password = "Test123!", FirstName = "Test", LastName = "User" }
            };

            foreach (var userData in users)
            {
                var user = new ApplicationUser
                {
                    UserName = userData.Username,
                    Email = userData.Username,
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, userData.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating user {userData.Username}: {error.Description}");
                    }
                }
            }
        }

        private async Task SeedAgentsAsync()
        {
            if (await _processorContext.Agents.AnyAsync())
                return;

            var agents = new[]
            {
                new Agent
                {
                    Id = Guid.NewGuid(),
                    EndpointUrl = "http://localhost:8080",
                    Hostname = "dev-agent-1",
                    Uid = Guid.NewGuid().ToString(),
                    Version = "1.0.0-dev",
                    IsDebugEnabled = true,
                    IsImageCompressionEnabled = false,
                    LastHeartbeatEpochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };

            _processorContext.Agents.AddRange(agents);
            await _processorContext.SaveChangesAsync();
        }

        private async Task SeedCamerasAsync()
        {
            if (await _processorContext.Cameras.AnyAsync())
                return;

            var cameras = new[]
            {
                new Camera
                {
                    Id = Guid.NewGuid(),
                    OpenAlprName = "Front Gate Camera",
                    OpenAlprCameraId = 1,
                    OpenAlprEnabled = true,
                    IpAddress = "localhost",
                    PlatesSeen = 0
                },
                new Camera
                {
                    Id = Guid.NewGuid(),
                    OpenAlprName = "Back Entrance Camera",
                    OpenAlprCameraId = 2,
                    OpenAlprEnabled = true,
                    IpAddress = "localhost",
                    PlatesSeen = 0
                }
            };

            _processorContext.Cameras.AddRange(cameras);
            await _processorContext.SaveChangesAsync();
        }

        private async Task SeedPlateGroupsAsync()
        {
            if (await _processorContext.PlateGroups.AnyAsync())
                return;

            var baseTime = DateTimeOffset.UtcNow.AddDays(-7);
            var plateGroups = new List<PlateGroup>();

            var plateNumbers = new[] { "ABC123", "XYZ789", "DEF456", "GHI012", "JKL345", "MNO678", "PQR901", "STU234", "VWX567", "YZA890" };
            var vehicleColors = new[] { "White", "Black", "Silver", "Red", "Blue", "Gray", "Green", "Yellow", "Orange", "Purple" };
            var vehicleMakeModels = new[] 
            { 
                "acura_ilx", "acura_mdx", "acura_rdx", "acura_tlx",
                "honda_accord", "honda_civic", "honda_pilot", "honda_crv",
                "toyota_camry", "toyota_corolla", "toyota_prius", "toyota_rav4",
                "ford_f150", "ford_mustang", "ford_explorer", "ford_escape",
                "bmw_320i", "bmw_x3", "bmw_x5", "bmw_530i",
                "mercedes_c300", "mercedes_e350", "mercedes_glc300", "mercedes_s550",
                "chevrolet_malibu", "chevrolet_equinox", "chevrolet_silverado", "chevrolet_tahoe",
                "nissan_altima", "nissan_sentra", "nissan_rogue", "nissan_pathfinder",
                "hyundai_elantra", "hyundai_sonata", "hyundai_tucson", "hyundai_santa_fe",
                "kia_optima", "kia_sorento", "kia_sportage", "kia_forte"
            };
            var vehicleTypes = new[] { "Sedan", "SUV", "Truck", "Coupe", "Hatchback", "Wagon", "Convertible", "Minivan" };

            for (int i = 0; i < 100; i++)
            {
                var makeModel = vehicleMakeModels[i % vehicleMakeModels.Length];
                var make = makeModel.Split('_')[0];
                
                var plateGroup = new PlateGroup
                {
                    Id = Guid.NewGuid(),
                    BestNumber = plateNumbers[i % plateNumbers.Length],
                    ReceivedOnEpoch = baseTime.AddHours(i * 2).ToUnixTimeMilliseconds(),
                    OpenAlprUuid = Guid.NewGuid().ToString(),
                    OpenAlprCameraId = (i % 2) + 1,
                    Confidence = 85.0 + (i % 15),
                    IsAlert = i % 10 == 0,
                    VehicleColor = vehicleColors[i % vehicleColors.Length],
                    VehicleMake = make,
                    VehicleMakeModel = makeModel,
                    VehicleType = vehicleTypes[i % vehicleTypes.Length],
                    VehicleRegion = "us-ca",
                    PossibleNumbers = new List<PlateGroupPossibleNumbers>()
                };

                plateGroups.Add(plateGroup);
            }

            _processorContext.PlateGroups.AddRange(plateGroups);
            await _processorContext.SaveChangesAsync();
        }

        private async Task SeedAlertsAsync()
        {
            if (await _processorContext.Alerts.AnyAsync())
                return;

            var alerts = new[]
            {
                new Alert
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "WANTED1",
                    Description = "Stolen Vehicle Alert",
                    IsStrictMatch = true
                },
                new Alert
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "WATCH*",
                    Description = "Watch List Pattern",
                    IsStrictMatch = false
                }
            };

            _processorContext.Alerts.AddRange(alerts);
            await _processorContext.SaveChangesAsync();
        }

        private async Task SeedIgnoresAsync()
        {
            if (await _processorContext.Ignores.AnyAsync())
                return;

            var ignores = new[]
            {
                new Ignore
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "STAFF*",
                    Description = "Staff Vehicles",
                    IsStrictMatch = false
                },
                new Ignore
                {
                    Id = Guid.NewGuid(),
                    PlateNumber = "SERVICE1",
                    Description = "Service Vehicle",
                    IsStrictMatch = true
                }
            };

            _processorContext.Ignores.AddRange(ignores);
            await _processorContext.SaveChangesAsync();
        }
    }
} 