﻿using Microsoft.EntityFrameworkCore;
using OpenAlprWebhookProcessor.Data;
using System.Threading.Tasks;

namespace OpenAlprWebhookProcessor.Settings.GetCameras
{
    public class GetAgentRequestHandler
    {
        private readonly ProcessorContext _processorContext;

        public GetAgentRequestHandler(ProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        public async Task<Agent> HandleAsync()
        {
            var agent = await _processorContext.Agents.FirstOrDefaultAsync();

            if (agent == null)
            {
                return new Agent();
            }

            return new Agent()
            {
                EndpointUrl = agent.EndpointUrl,
                Hostname = agent.Hostname,
                OpenAlprWebServerApiKey = agent.OpenAlprWebServerApiKey,
                OpenAlprWebServerUrl = agent.OpenAlprWebServerUrl,
                Uid = agent.Uid,
                Version = agent.Version,
            };
        }
    }
}
