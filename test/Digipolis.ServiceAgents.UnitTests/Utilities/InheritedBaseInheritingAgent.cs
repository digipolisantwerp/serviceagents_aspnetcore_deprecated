using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    class InheritingFromOtherClassAgent : InheritingFromBaseAgent
    {
        public InheritingFromOtherClassAgent(HttpClient client, IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(client, serviceProvider, options)
        {
        }
    }


    public class InheritingFromBaseAgent : AgentBase
    {
        public InheritingFromBaseAgent(HttpClient client, IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            :base(client, serviceProvider, options)
        {
        }
    }

}
