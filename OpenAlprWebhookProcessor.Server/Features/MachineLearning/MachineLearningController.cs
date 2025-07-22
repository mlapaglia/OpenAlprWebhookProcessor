using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.Features.MachineLearning.Commands.TriggerTraining;
using OpenAlprWebhookProcessor.Features.MachineLearning.Commands.UpsertConfiguration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Models;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetConfiguration;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelInfo;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetModelStatus;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTopPredictions;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.GetTrainingStatus;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictBatch;
using OpenAlprWebhookProcessor.Features.MachineLearning.Queries.PredictNextSeen;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Features.MachineLearning
{
    /// <summary>
    /// API controller for machine learning predictions on license plate patterns.
    /// Provides endpoints for single predictions, batch predictions, and model management.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MachineLearningController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MachineLearningController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("predict")]
        public async Task<ActionResult<LicensePlatePredictionResult>> PredictNextSeen(
            [FromBody] LicensePlateInput input, 
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(input?.LicensePlate))
            {
                return BadRequest("License plate is required");
            }

            try
            {
                var query = new PredictNextSeenQuery(input);
                var prediction = await _mediator.Send(query, cancellationToken);
                return Ok(prediction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Error generating prediction");
            }
        }

        [HttpPost("predict/batch")]
        public async Task<ActionResult<List<LicensePlatePredictionResult>>> PredictBatch(
            [FromBody] List<LicensePlateInput> inputs, 
            CancellationToken cancellationToken)
        {
            if (inputs == null || inputs.Count == 0)
            {
                return BadRequest("At least one license plate input is required");
            }

            if (inputs.Count > 100)
            {
                return BadRequest("Maximum 100 predictions per batch");
            }

            try
            {
                var query = new PredictBatchQuery(inputs);
                var predictions = await _mediator.Send(query, cancellationToken);
                return Ok(predictions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Error generating batch predictions");
            }
        }

        [HttpGet("predict/top")]
        public async Task<ActionResult<List<LicensePlatePredictionResult>>> GetTopPredictions(
            [FromQuery] int count = 10, 
            [FromQuery] int withinHours = 168,
            CancellationToken cancellationToken = default)
        {
            if (count <= 0 || count > 50)
            {
                return BadRequest("Count must be between 1 and 50");
            }

            if (withinHours <= 0 || withinHours > 8760) // Max 1 year
            {
                return BadRequest("WithinHours must be between 1 and 8760");
            }

            try
            {
                var query = new GetTopPredictionsQuery(count, TimeSpan.FromHours(withinHours));
                var predictions = await _mediator.Send(query, cancellationToken);
                
                return Ok(predictions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Error retrieving top predictions");
            }
        }

        [HttpGet("model/status")]
        public async Task<ActionResult<ModelStatusDto>> GetModelStatus(CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetModelStatusQuery();
                var result = await _mediator.Send(query, cancellationToken);
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "Error checking model status");
            }
        }

        [HttpPost("model/retrain")]
        public async Task<ActionResult<TrainingResultDto>> TriggerTraining(CancellationToken cancellationToken = default)
        {
            try
            {
                var command = new TriggerTrainingCommand(User.Identity?.Name ?? "Unknown");
                var result = await _mediator.Send(command, cancellationToken);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch
            {
                return StatusCode(500, new TrainingResultDto 
                { 
                    Message = "Error triggering model training", 
                    Timestamp = DateTime.UtcNow, 
                    Success = false 
                });
            }
        }

        [HttpGet("model/info")]
        public async Task<ActionResult<ModelInfoDto>> GetModelInfo(CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetModelInfoQuery();
                var result = await _mediator.Send(query, cancellationToken);
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "Error retrieving model information");
            }
        }

        [HttpGet("training/status")]
        public async Task<ActionResult<TrainingStatusDto>> GetTrainingStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new GetTrainingStatusQuery();
                var result = await _mediator.Send(query, cancellationToken);
                return Ok(result);
            }
            catch
            {
                return StatusCode(500, "Error retrieving training status");
            }
        }

        [HttpGet("configuration")]
        public async Task<ActionResult<MachineLearningConfigDto>> GetConfiguration()
        {
            var result = await _mediator.Send(new GetConfigurationQuery());
            return Ok(result);
        }

        [HttpPut("configuration")]
        public async Task<ActionResult<Unit>> UpsertConfiguration(
            [FromBody] MachineLearningConfigDto request)
        {
            var command = new UpsertConfigurationCommand
            {
                Configuration = request,
                UpdatedBy = User.Identity?.Name ?? "System"
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
} 