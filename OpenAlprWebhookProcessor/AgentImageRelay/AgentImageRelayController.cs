using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAlprWebhookProcessor.AgentImageRelay.GetImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.AgentImageRelay
{
    [Authorize]
    [ApiController]
    [Route("images")]
    public class AgentImageRelayController : Controller
    {
        private readonly GetImageHandler _getImageHandler;

        public AgentImageRelayController(GetImageHandler getImageHandler)
        {
            _getImageHandler = getImageHandler;
        }

        [HttpGet("{imageId}")]
        public async Task<Stream> GetImage(
            string imageId,
            CancellationToken cancellationToken)
        {
            return await _getImageHandler.GetImageFromAgentAsync(imageId, cancellationToken);
        }

        [HttpGet("crop/{imageId}")]
        public async Task<Stream> GetCropImage(
            string imageId,
            string x1,
            string x2,
            string y1,
            string y2,
            CancellationToken cancellationToken)
        {
            return await _getImageHandler.GetCropImageFromAgentAsync($"{imageId}?x1={x1}&x2={x2}&y1={y1}&y2={y2}", cancellationToken);
        }
    }
}
