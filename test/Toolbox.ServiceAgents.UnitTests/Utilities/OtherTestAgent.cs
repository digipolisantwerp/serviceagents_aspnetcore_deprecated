using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents.UnitTests.Utilities
{
    public class OtherTestAgent : AgentBase
    {
        public OtherTestAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(serviceProvider, options)
        {
        }
    }
}
