using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    public class OtherTestAgent : AgentBase
    {
        public OtherTestAgent(HttpClient client, IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(client, serviceProvider, options)
        {
        }
    }
}
