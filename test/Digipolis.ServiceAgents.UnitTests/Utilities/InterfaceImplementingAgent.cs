using Microsoft.Extensions.Options;
using System;
using Digipolis.ServiceAgents.Settings;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    public class InterfaceImplementingAgent : AgentBase, IInterfaceImplementingAgent
    {
        public InterfaceImplementingAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            :base(serviceProvider, options)
        {

        }
    }
}
