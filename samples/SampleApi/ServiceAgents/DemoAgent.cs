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
    public class DemoAgent : AgentBase, IDemoAgent
    {
        public DemoAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options) 
            : base(serviceProvider, options)
        {
        }

        public Task<string> GetAsStringAsync()
        {
            return GetStringAsync("values/getsomevalue");
        }

        //A basic get operation
        public Task<Address> GetAddressAsync(int id)
        {
            return GetAsync<Address>($"adress?id={id}");
        }

        //A basic post operation
        public Task<Address> PostAddressAsync(Address adress)
        {
            return PostAsync<Address>($"adress", adress);
        }
    }
}
