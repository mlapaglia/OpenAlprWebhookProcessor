using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.UpsertPlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates;
using OpenAlprWebhookProcessor.LicensePlates;
using OpenAlprWebhookProcessor.LicensePlates.SearchLicensePlates;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/licensePlates")]
    public class LicensePlatesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LicensePlatesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("search")]
        public async Task<ActionResult<SearchLicensePlateResponse>> SearchPlates(
            [FromBody] SearchLicensePlateRequest request,
            CancellationToken cancellationToken)
        {
            var query = new SearchLicensePlatesQuery
            {
                PlateNumber = request.PlateNumber,
                StrictMatch = request.StrictMatch,
                RegexSearchEnabled = request.RegexSearchEnabled,
                StartSearchOn = request.StartSearchOn,
                EndSearchOn = request.EndSearchOn,
                FilterIgnoredPlates = request.FilterIgnoredPlates,
                VehicleColor = request.VehicleColor,
                VehicleMake = request.VehicleMake,
                VehicleModel = request.VehicleModel,
                VehicleType = request.VehicleType,
                VehicleRegion = request.VehicleRegion,
                FilterPlatesSeenLessThan = request.FilterPlatesSeenLessThan,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("edit")]
        public async Task<ActionResult> UpsertPlate(
            [FromBody] LicensePlate licensePlate,
            CancellationToken cancellationToken)
        {
            var command = new UpsertPlateCommand
            {
                Id = licensePlate.Id,
                PlateNumber = licensePlate.PlateNumber,
                Description = licensePlate.Description,
                IsAlert = licensePlate.IsAlert,
                IsIgnore = licensePlate.IsIgnore,
                VehicleColor = licensePlate.VehicleColor,
                VehicleMake = licensePlate.VehicleMake,
                VehicleModel = licensePlate.VehicleModel,
                VehicleType = licensePlate.VehicleType,
                VehicleRegion = licensePlate.VehicleRegion,
                Latitude = licensePlate.Latitude,
                Longitude = licensePlate.Longitude,
                ReceivedOnEpoch = licensePlate.ReceivedOnEpoch,
                IsStrictMatch = licensePlate.IsStrictMatch,
                IsPatternMatch = licensePlate.IsPatternMatch,
                IsEnriched = licensePlate.IsEnriched,
                EnrichedData = licensePlate.EnrichedData,
                Notes = licensePlate.Notes
            };

            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        // TODO: Add other endpoints using MediatR pattern
        // - GetPlate
        // - GetLicensePlateCounts
        // - GetMostSeenPlates
        // - GetStatistics
        // - GetPlateFilters
        // - DeletePlate
        // - EnrichPlate
    }
} 