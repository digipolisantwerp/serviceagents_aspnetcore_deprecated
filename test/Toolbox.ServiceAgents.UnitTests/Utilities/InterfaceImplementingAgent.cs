using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents.UnitTests.Utilities
{
    public class InterfaceImplementingAgent : AgentBase, IInterfaceImplementingAgent
    {
        public InterfaceImplementingAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            :base(serviceProvider, options)
        {

        }
    }
}
