using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    class InheritingFromOtherClassAgent : InheritingFromBaseAgent
    {
        public InheritingFromOtherClassAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(serviceProvider, options)
        {
        }
    }


    public class InheritingFromBaseAgent : AgentBase
    {
        public InheritingFromBaseAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            :base(serviceProvider, options)
        {
        }
    }

}
