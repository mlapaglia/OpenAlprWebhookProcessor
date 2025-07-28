using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.DeletePlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EditPlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Commands.EnrichPlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetLicensePlateCounts;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetMostSeenPlates;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlate;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetPlateFilters;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetStatistics;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetHourlyStats;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.GetQuickStats;
using OpenAlprWebhookProcessor.Features.LicensePlates.Queries.SearchLicensePlates;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.LicensePlates
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
                FilterPlatesSeenLessThan = request.FilterPlatesSeenLessThan ?? 0,
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
            var command = new EditPlateCommand
            {
                Id = licensePlate.Id,
                PlateNumber = licensePlate.PlateNumber,
                Notes = licensePlate.Notes
            };

            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LicensePlate>> GetPlate(
            Guid id,
            CancellationToken cancellationToken)
        {
            var query = new GetPlateQuery(id);
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("counts")]
        public async Task<ActionResult<GetLicensePlateCountsResponse>> GetLicensePlateCounts(
            [FromQuery] DateTimeOffset? startDate,
            [FromQuery] DateTimeOffset? endDate,
            CancellationToken cancellationToken)
        {
            // Default to last 30 days if no dates provided
            var start = startDate ?? DateTimeOffset.UtcNow.AddDays(-30);
            var end = endDate ?? DateTimeOffset.UtcNow;
            
            var query = new GetLicensePlateCountsQuery(start, end);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("most-seen")]
        public async Task<ActionResult<GetMostSeenPlatesResponse>> GetMostSeenPlates(
            [FromQuery] DateTimeOffset? startDate,
            [FromQuery] DateTimeOffset? endDate,
            CancellationToken cancellationToken,
            [FromQuery] int limit = 10)
        {
            var query = new GetMostSeenPlatesQuery(startDate, endDate, limit);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("statistics/{PlateNumber}")]
        public async Task<ActionResult> GetStatistics(
            string plateNumber,
            CancellationToken cancellationToken)
        {
            var query = new GetStatisticsQuery(plateNumber);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("filters")]
        public async Task<ActionResult> GetPlateFilters(CancellationToken cancellationToken)
        {
            var query = new GetPlateFiltersQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeletePlate(
            Guid id,
            CancellationToken cancellationToken)
        {
            var command = new DeletePlateCommand(id);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPost("{id:guid}/enrich")]
        public async Task<ActionResult> EnrichPlate(
            Guid id,
            CancellationToken cancellationToken)
        {
            var command = new EnrichPlateCommand(id);
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpGet("stats/hourly")]
        public async Task<ActionResult<GetHourlyStatsResponse>> GetHourlyStats(
            CancellationToken cancellationToken)
        {
            var query = new GetHourlyStatsQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("stats/quick")]
        public async Task<ActionResult<GetQuickStatsResponse>> GetQuickStats(
            CancellationToken cancellationToken)
        {
            var query = new GetQuickStatsQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
} 