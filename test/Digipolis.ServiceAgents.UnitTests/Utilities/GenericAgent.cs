using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    public class GenericAgent<T> : AgentBase
    {
        public GenericAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(serviceProvider, options)
        {
        }

        public ServiceSettings ServiceSettings
        {
            get { return _settings; }
        }
    }
}
