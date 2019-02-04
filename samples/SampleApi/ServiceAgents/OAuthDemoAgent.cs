using Digipolis.ServiceAgents;
using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleApi.ServiceAgents
{
    public class OAuthDemoAgent : AgentBase, IOAuthDemoAgent
    {
        public OAuthDemoAgent(HttpClient client, IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options) 
            : base(client, serviceProvider, options)
        {
        }

        public Task<string> GetAsStringAsync()
        {
            return GetStringAsync("get");
        }
       
    }
}
