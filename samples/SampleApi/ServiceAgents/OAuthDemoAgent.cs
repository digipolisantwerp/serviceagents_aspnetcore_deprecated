using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.OptionsModel;
using Toolbox.ServiceAgents;
using Toolbox.ServiceAgents.Settings;
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
