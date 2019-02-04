using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    public class InterfaceImplementingAgent : AgentBase, IInterfaceImplementingAgent
    {
        public InterfaceImplementingAgent(HttpClient client, IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            :base(client, serviceProvider, options)
        {

        }
    }
}
