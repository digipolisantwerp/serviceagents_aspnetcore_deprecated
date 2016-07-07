using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Digipolis.ServiceAgents;
using Digipolis.ServiceAgents.Settings;
using SampleApi.Models;
using System.Threading;

namespace SampleApi.ServiceAgents
{
    public class OAuthDemoAgent : AgentBase, IOAuthDemoAgent
    {
        public OAuthDemoAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options) 
            : base(serviceProvider, options)
        {
        }

        public Task<string> GetAsStringAsync()
        {
            return GetStringAsync("get");
        }
       
    }
}
