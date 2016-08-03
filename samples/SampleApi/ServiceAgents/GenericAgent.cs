
using Digipolis.ServiceAgents;
using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using SampleApi.Models;
using System;
using System.Threading.Tasks;

namespace SampleApi.ServiceAgents
{
    public class GenericAgent<T> : AgentBase
    {
        public GenericAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options) 
            : base(serviceProvider, options)
        {
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
