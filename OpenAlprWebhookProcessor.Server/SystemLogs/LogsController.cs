using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.SystemLogs
{
    [Authorize]
    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        public LogsController()
        {
        }

        [HttpGet]
        public async Task<List<string>> GetLogs()
        {
            var currentLogFile = Directory.GetFiles("./config/")
                .LastOrDefault(x => x.Contains("log-"));

            using (var stream = System.IO.File.Open(
                currentLogFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(stream, Encoding.UTF8))
                {
                    var logs = new List<string>();

                    while (!sr.EndOfStream)
                    {
                        logs.Add(await sr.ReadLineAsync());
                    }

                    logs.Reverse();

                    return logs.Take(500).ToList();
                }
            }
        }
    }
}