using OpenAlprWebhookProcessor.Data;
using OpenAlprWebhookProcessor.Features.Users.Data;
using OpenAlprWebhookProcessor.Features.Users;

namespace Tests.TestHelpers
{
    public static class TestDataFactory
    {
        public static OpenAlprWebhookProcessor.Features.Alerts.Alert CreateTestAlert(string? plateNumber = null, string? description = null, bool strictMatch = false)
        {
            return new OpenAlprWebhookProcessor.Features.Alerts.Alert
            {
                Id = Guid.NewGuid(),
                PlateNumber = plateNumber ?? "ABC123",
                Description = description ?? "Test Alert",
                StrictMatch = strictMatch
            };
        }

        public static OpenAlprWebhookProcessor.Features.Alerts.Alert CreateTestAlertWithPlateNumber(string? plateNumber, string? description = null, bool strictMatch = false)
        {
            return new OpenAlprWebhookProcessor.Features.Alerts.Alert
            {
                Id = Guid.NewGuid(),
                PlateNumber = plateNumber,
                Description = description ?? "Test Alert",
                StrictMatch = strictMatch
            };
        }

        public static OpenAlprWebhookProcessor.Data.Alert CreateTestDbAlert(string? plateNumber = null, string? description = null, bool strictMatch = false)
        {
            return new OpenAlprWebhookProcessor.Data.Alert
            {
                Id = Guid.NewGuid(),
                PlateNumber = plateNumber ?? "ABC123",
                Description = description ?? "Test Alert",
                IsStrictMatch = strictMatch
            };
        }

        public static OpenAlprWebhookProcessor.Data.Ignore CreateTestIgnore(string? plateNumber = null, string? description = null, bool strictMatch = false)
        {
            return new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = plateNumber ?? "IGNORE123",
                Description = description ?? "Test Ignore",
                IsStrictMatch = strictMatch
            };
        }

        public static OpenAlprWebhookProcessor.Data.Camera CreateTestCamera(string? openAlprName = null, long? openAlprCameraId = null)
        {
            return new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprName = openAlprName ?? "Test Camera",
                OpenAlprCameraId = openAlprCameraId ?? 1,
                OpenAlprEnabled = true,
                IpAddress = "192.168.1.100",
                PlatesSeen = 0
            };
        }

        public static OpenAlprWebhookProcessor.Data.PlateGroup CreateTestPlateGroup(string? plateNumber = null, long? receivedOnEpoch = null)
        {
            return new OpenAlprWebhookProcessor.Data.PlateGroup
            {
                Id = Guid.NewGuid(),
                BestNumber = plateNumber ?? "TEST123",
                ReceivedOnEpoch = receivedOnEpoch ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OpenAlprUuid = Guid.NewGuid().ToString(),
                OpenAlprCameraId = 1,
                Confidence = 90.5,
                IsAlert = false,
                PossibleNumbers = new List<PlateGroupPossibleNumbers>()
            };
        }

        // Overloaded method with additional parameters for testing
        public static OpenAlprWebhookProcessor.Data.PlateGroup CreateTestPlateGroupDetailed(
            string? plateNumber = null, 
            long? receivedOnEpoch = null,
            string? vehicleColor = null,
            string? vehicleMakeModel = null,
            string? vehicleType = null,
            string? vehicleRegion = null,
            long? epochTimeMs = null,
            int? seenCount = null)
        {
            return new OpenAlprWebhookProcessor.Data.PlateGroup
            {
                Id = Guid.NewGuid(),
                BestNumber = plateNumber ?? "TEST123",
                ReceivedOnEpoch = epochTimeMs ?? receivedOnEpoch ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                OpenAlprUuid = Guid.NewGuid().ToString(),
                OpenAlprCameraId = 1,
                Confidence = 90.5,
                IsAlert = false,
                PossibleNumbers = new List<PlateGroupPossibleNumbers>(),
                VehicleColor = vehicleColor,
                VehicleMakeModel = vehicleMakeModel,
                VehicleType = vehicleType,
                VehicleRegion = vehicleRegion ?? "us-ca"
            };
        }

        public static OpenAlprWebhookProcessor.Data.Agent CreateTestAgent(string? endpointUrl = null, string? hostname = null)
        {
            return new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = endpointUrl ?? "http://test.local",
                Hostname = hostname ?? "test-hostname",
                Uid = Guid.NewGuid().ToString(),
                Version = "1.0.0",
                IsDebugEnabled = false,
                IsImageCompressionEnabled = false,
                LastHeartbeatEpochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        private static int _userIdCounter = 1;

        public static User CreateTestUser(string? username = null, string? firstName = null, string? lastName = null)
        {
            var id = Interlocked.Increment(ref _userIdCounter);
            return new User
            {
                Id = id,
                Username = username ?? $"testuser{id}",
                FirstName = firstName ?? "Test",
                LastName = lastName ?? "User",
                PasswordHash = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 },
                RefreshTokens = new List<RefreshToken>()
            };
        }

        public static OpenAlprWebhookProcessor.Data.WebhookForward CreateTestWebhookForward(string? forwardUrl = null)
        {
            return new OpenAlprWebhookProcessor.Data.WebhookForward
            {
                Id = Guid.NewGuid(),
                FowardingDestination = new Uri(forwardUrl ?? "http://test-forward.local"),
                IgnoreSslErrors = false,
                ForwardSinglePlates = true,
                ForwardGroupPreviews = true,
                ForwardGroups = true
            };
        }

        public static OpenAlprWebhookProcessor.CameraUpdateService.Camera CreateTestCameraUpdateServiceCamera(string? openAlprName = null, long? openAlprCameraId = null)
        {
            return new OpenAlprWebhookProcessor.CameraUpdateService.Camera
            {
                Id = Guid.NewGuid(),
                OpenAlprName = openAlprName ?? "Test Camera",
                OpenAlprCameraId = openAlprCameraId ?? 1,
                PlatesSeen = 0,
                IpAddress = "192.168.1.100"
            };
        }

        public static OpenAlprWebhookProcessor.Data.Enricher CreateTestEnricher(bool isEnabled = true)
        {
            return new OpenAlprWebhookProcessor.Data.Enricher
            {
                Id = Guid.NewGuid(),
                IsEnabled = isEnabled,
                ApiKey = "test-api-key"
            };
        }

        public static OpenAlprWebhookProcessor.Features.LicensePlate CreateTestLicensePlate(string? plateNumber = null)
        {
            return new OpenAlprWebhookProcessor.Features.LicensePlate
            {
                Id = Guid.NewGuid(),
                OpenAlprCameraId = 1,
                VehicleDescription = "2023 Toyota Camry",
                PlateNumber = plateNumber ?? "ABC123",
                Region = "US",
                PossiblePlateNumbers = "ABC123, AEC123",
                OpenAlprProcessingTimeMs = 150.5,
                ProcessedPlateConfidence = 95.8,
                IsAlert = false,
                IsIgnore = false,
                AlertDescription = "",
                ReceivedOn = DateTimeOffset.UtcNow,
                Direction = 90.0,
                ImageUrl = new Uri("/api/images/test-uuid", UriKind.Relative),
                CropImageUrl = new Uri("/api/images/crop/test-uuid", UriKind.Relative),
                Notes = "Test notes",
                CanBeEnriched = true
            };
        }

        public static OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates.SearchLicensePlateRequest CreateTestSearchLicensePlateRequest(string? plateNumber = null)
        {
            return new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates.SearchLicensePlateRequest
            {
                PlateNumber = plateNumber ?? "ABC123",
                StrictMatch = false,
                RegexSearchEnabled = false,
                StartSearchOn = DateTimeOffset.UtcNow.AddDays(-30),
                EndSearchOn = DateTimeOffset.UtcNow,
                FilterIgnoredPlates = false,
                VehicleColor = "Red",
                VehicleMake = "Toyota",
                VehicleModel = "Camry",
                VehicleType = "Car",
                VehicleRegion = "US",
                FilterPlatesSeenLessThan = 0,
                PageNumber = 0,
                PageSize = 50
            };
        }

        public static OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates.SearchLicensePlateResponse CreateTestSearchLicensePlateResponse()
        {
            return new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates.SearchLicensePlateResponse
            {
                Plates = new List<OpenAlprWebhookProcessor.Features.LicensePlate>
                {
                    CreateTestLicensePlate("ABC123"),
                    CreateTestLicensePlate("XYZ789")
                },
                TotalCount = 2
            };
        }

        public static OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts.GetLicensePlateCountsResponse CreateTestGetLicensePlateCountsResponse()
        {
            return new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts.GetLicensePlateCountsResponse
            {
                Counts = new List<OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts.DayCount>
                {
                    new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts.DayCount { Date = DateTimeOffset.UtcNow.AddDays(-1), Count = 10 },
                    new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts.DayCount { Date = DateTimeOffset.UtcNow, Count = 15 }
                }
            };
        }

        public static OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates.GetMostSeenPlatesResponse CreateTestGetMostSeenPlatesResponse()
        {
            return new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates.GetMostSeenPlatesResponse
            {
                Counts = new List<OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates.MostSeenCount>
                {
                    new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates.MostSeenCount { PlateNumber = "ABC123", Count = 25 },
                    new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates.MostSeenCount { PlateNumber = "XYZ789", Count = 20 }
                }
            };
        }

        public static OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics.PlateStatistics CreateTestPlateStatistics()
        {
            return new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics.PlateStatistics
            {
                Last90Days = 15,
                TotalSeen = 50,
                FirstSeen = DateTimeOffset.UtcNow.AddDays(-100),
                LastSeen = DateTimeOffset.UtcNow.AddDays(-1)
            };
        }

        public static OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters.GetLicensePlateFiltersResponse CreateTestGetLicensePlateFiltersResponse()
        {
            return new OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters.GetLicensePlateFiltersResponse
            {
                VehicleMakes = new List<string> { "Toyota", "Honda", "Ford" },
                VehicleModels = new List<string> { "Camry", "Accord", "Focus" },
                VehicleTypes = new List<string> { "Car", "Truck", "SUV" },
                VehicleYears = new List<string> { "2020", "2021", "2022", "2023" },
                VehicleColors = new List<string> { "Red", "Blue", "White", "Black" },
                VehicleRegions = new List<string> { "US", "CA", "EU" }
            };
        }

        public static OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent.AgentDto CreateTestAgentDto()
        {
            return new OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgent.AgentDto
            {
                Id = Guid.NewGuid(),
                EndpointUrl = "http://test-agent.local",
                Hostname = "test-hostname",
                Uid = "test-uid-12345",
                OpenAlprWebServerUrl = "http://openalpr-server.local",
                Latitude = 40.7128,
                Longitude = -74.0060,
                SunriseOffset = 30,
                SunsetOffset = -30,
                TimezoneOffset = -5.0,
                IsDebugEnabled = true,
                IsImageCompressionEnabled = false,
                LastHeartbeatEpochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                ScheduledScrapingIntervalMinutes = 60,
                NextScrapeInMinutes = 30
            };
        }

        public static OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentStatus.AgentStatusDto CreateTestAgentStatusDto()
        {
            return new OpenAlprWebhookProcessor.Features.Settings.Queries.GetAgentStatus.AgentStatusDto
            {
                IsConnected = true,
                Hostname = "test-hostname",
                Version = "1.2.3",
                CpuCores = 4,
                CpuUsagePercent = 25.5m,
                DaemonUptimeSeconds = 86400,
                DiskFreeBytes = 1000000000,
                SystemUptimeSeconds = 172800,
                AgentEpochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                AlprdActive = true,
                LastHeartbeatEpochMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
        }

        public static OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores.IgnoreDto CreateTestIgnoreDto(string? plateNumber = null)
        {
            return new OpenAlprWebhookProcessor.Features.Settings.Queries.GetIgnores.IgnoreDto
            {
                Id = Guid.NewGuid(),
                PlateNumber = plateNumber ?? "IGNORE123",
                StrictMatch = false,
                Description = "Test ignore description"
            };
        }

        public static OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards.WebhookForwardDto CreateTestWebhookForwardDto()
        {
            return new OpenAlprWebhookProcessor.Features.Settings.Queries.GetWebhookForwards.WebhookForwardDto
            {
                Id = Guid.NewGuid(),
                Destination = new Uri("http://test-forward.local/webhook"),
                IgnoreSslErrors = false,
                ForwardGroupPreviews = true,
                ForwardSinglePlates = true,
                ForwardGroups = true
            };
        }

        public static OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers.EnricherDto CreateTestEnricherDto()
        {
            return new OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers.EnricherDto
            {
                Id = Guid.NewGuid(),
                IsEnabled = true,
                ApiKey = "test-api-key-12345",
                EnricherType = OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate.EnricherType.LicenseDateDataApi,
                EnrichmentType = OpenAlprWebhookProcessor.Features.Settings.Queries.GetEnrichers.EnrichmentType.Always
            };
        }

        public static OpenAlprWebhookProcessor.Features.Users.AuthenticateRequest CreateTestAuthenticateRequest(string? username = null, string? password = null)
        {
            return new OpenAlprWebhookProcessor.Features.Users.AuthenticateRequest
            {
                Username = username ?? "testuser",
                Password = password ?? "testpassword"
            };
        }

        public static OpenAlprWebhookProcessor.Features.Users.AuthenticateResponse CreateTestAuthenticateResponse()
        {
            var user = CreateTestUser();
            return new OpenAlprWebhookProcessor.Features.Users.AuthenticateResponse(
                user,
                "test-jwt-token",
                "test-refresh-token"
            );
        }

        public static OpenAlprWebhookProcessor.Features.Users.Register.RegisterModel CreateTestRegisterModel(string? username = null)
        {
            return new OpenAlprWebhookProcessor.Features.Users.Register.RegisterModel
            {
                FirstName = "Test",
                LastName = "User",
                Username = username ?? "testuser",
                Password = "testpassword123"
            };
        }

        public static OpenAlprWebhookProcessor.Features.Users.UpdateModel CreateTestUpdateModel()
        {
            return new OpenAlprWebhookProcessor.Features.Users.UpdateModel
            {
                FirstName = "Updated",
                LastName = "User",
                Username = "updateduser",
                Password = "updatedpassword123"
            };
        }

        public static OpenAlprWebhookProcessor.Features.Users.RevokeTokenRequest CreateTestRevokeTokenRequest(string? token = null)
        {
            return new OpenAlprWebhookProcessor.Features.Users.RevokeTokenRequest
            {
                Token = token ?? "test-refresh-token"
            };
        }

        public static List<OpenAlprWebhookProcessor.Features.Users.RefreshToken> CreateTestRefreshTokens()
        {
            return new List<OpenAlprWebhookProcessor.Features.Users.RefreshToken>
            {
                new OpenAlprWebhookProcessor.Features.Users.RefreshToken
                {
                    Token = "test-refresh-token-1",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1"
                },
                new OpenAlprWebhookProcessor.Features.Users.RefreshToken
                {
                    Token = "test-refresh-token-2",
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = "127.0.0.1"
                }
            };
        }

        // ImageRelay test data factory methods
        public static OpenAlprWebhookProcessor.Data.Agent CreateTestAgentWithCompression(string? endpointUrl = "https://test-agent.local", bool isImageCompressionEnabled = false)
        {
            return new OpenAlprWebhookProcessor.Data.Agent
            {
                Id = Guid.NewGuid(),
                EndpointUrl = endpointUrl,
                IsImageCompressionEnabled = isImageCompressionEnabled,
                Uid = "test-agent-uid",
                Hostname = "test-hostname"
            };
        }

        public static OpenAlprWebhookProcessor.Data.Camera CreateTestCamera(OpenAlprWebhookProcessor.Features.Cameras.Configuration.CameraManufacturer? manufacturer = null, Guid? id = null, long? openAlprCameraId = null)
        {
            return new OpenAlprWebhookProcessor.Data.Camera
            {
                Id = id ?? Guid.NewGuid(),
                Manufacturer = manufacturer ?? OpenAlprWebhookProcessor.Features.Cameras.Configuration.CameraManufacturer.Hikvision,
                IpAddress = "192.168.1.100",
                OpenAlprCameraId = openAlprCameraId ?? 1,
                PlatesSeen = 10,
                ModelNumber = "Test Model",
                OpenAlprName = "Test Camera",
                CameraUsername = "admin",
                CameraPassword = "password123",
                UpdateOverlayTextUrl = "http://192.168.1.100/overlay",
                UpdateOverlayEnabled = true,
                UpdateDayNightModeEnabled = false,
                OpenAlprEnabled = true
            };
        }

        public static OpenAlprWebhookProcessor.Data.PlateGroup CreateTestPlateGroupForImageRelay(string? openAlprUuid = null, string? plateCoordinates = null)
        {
            return new OpenAlprWebhookProcessor.Data.PlateGroup
            {
                Id = Guid.NewGuid(),
                OpenAlprUuid = openAlprUuid ?? "test-uuid-123",
                PlateCoordinates = plateCoordinates ?? "x=100,y=200,w=300,h=400",
                BestNumber = "ABC123",
                OpenAlprCameraId = 1,
                ReceivedOnEpoch = 1234567890,
                VehicleRegion = "us-ca",
                VehicleColor = "Red",
                VehicleMake = "Toyota",
                VehicleMakeModel = "Camry",
                VehicleType = "Car",
                VehicleYear = "2023",
                Direction = 90.0,
                OpenAlprProcessingTimeMs = 150.5,
                Confidence = 95.8,
                IsAlert = false,
                AlertDescription = "",
                Notes = "Test notes",
                IsEnriched = false,
                VehicleImage = null,
                PlateImage = null
            };
        }

        public static OpenAlprWebhookProcessor.Data.VehicleImage CreateTestVehicleImage(bool isCompressed = false)
        {
            return new OpenAlprWebhookProcessor.Data.VehicleImage
            {
                Id = Guid.NewGuid(),
                Jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 }, // JPEG header
                IsCompressed = isCompressed
            };
        }

        public static OpenAlprWebhookProcessor.Data.PlateImage CreateTestPlateImage(bool isCompressed = false)
        {
            return new OpenAlprWebhookProcessor.Data.PlateImage
            {
                Id = Guid.NewGuid(),
                Jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 }, // JPEG header
                IsCompressed = isCompressed
            };
        }

        public static OpenAlprWebhookProcessor.Data.PlateGroup CreateTestPlateGroupWithImages(string? openAlprUuid = null, bool withVehicleImage = true, bool withPlateImage = true)
        {
            var plateGroup = CreateTestPlateGroupForImageRelay(openAlprUuid);
            
            if (withVehicleImage)
            {
                plateGroup.VehicleImage = CreateTestVehicleImage();
            }
            
            if (withPlateImage)
            {
                plateGroup.PlateImage = CreateTestPlateImage();
            }
            
            return plateGroup;
        }

        public static byte[] CreateTestJpegBytes()
        {
            return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };
        }

        public static OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate.EnrichedLicensePlate CreateTestEnrichedLicensePlate()
        {
            return new OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate.EnrichedLicensePlate
            {
                Make = "Toyota",
                Style = "Sedan"
            };
        }

        public static OpenAlprWebhookProcessor.Data.Ignore CreateTestIgnoreEntity(string plateNumber = "IGNORE123")
        {
            return new OpenAlprWebhookProcessor.Data.Ignore
            {
                Id = Guid.NewGuid(),
                PlateNumber = plateNumber
            };
        }

        public static OpenAlprWebhookProcessor.Data.Alert CreateTestAlertEntity(string plateNumber = "ALERT123")
        {
            return new OpenAlprWebhookProcessor.Data.Alert
            {
                Id = Guid.NewGuid(),
                PlateNumber = plateNumber
            };
        }
    }
} 