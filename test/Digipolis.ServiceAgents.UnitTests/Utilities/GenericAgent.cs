using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using System;

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
