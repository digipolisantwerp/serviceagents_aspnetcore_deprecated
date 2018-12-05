using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    public class GenericAgent<T> : AgentBase
    {
        public GenericAgent(HttpClient client, IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(client, serviceProvider, options)
        {
        }

        public ServiceSettings ServiceSettings
        {
            get { return _settings; }
        }
    }
}
