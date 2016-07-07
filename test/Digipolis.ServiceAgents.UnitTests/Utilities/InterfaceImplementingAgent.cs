using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
