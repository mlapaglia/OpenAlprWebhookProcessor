using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenAlprWebhookProcessor.Hydrator
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class HydrationController : ControllerBase
    {
        private readonly HydrationService _hydrationService;

        public HydrationController(HydrationService hydrationService)
        {
            _hydrationService = hydrationService;
        }

        [HttpPost("start")]
        public IActionResult StartHydration()
        {
            _hydrationService.StartHydration();

            return StatusCode(202);
        }
    }
}