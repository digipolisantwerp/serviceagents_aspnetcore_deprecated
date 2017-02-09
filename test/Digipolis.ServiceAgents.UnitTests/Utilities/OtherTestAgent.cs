using Microsoft.Extensions.Options;
using System;
using Digipolis.ServiceAgents.Settings;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    public class OtherTestAgent : AgentBase
    {
        public OtherTestAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(serviceProvider, options)
        {
        }
    }
}
