using Digipolis.ServiceAgents;
using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Options;
using SampleApi.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleApi.ServiceAgents
{
    public class DemoAgent : AgentBase, IDemoAgent
    {
        public DemoAgent(HttpClient client, IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options) 
            : base(client, serviceProvider, options)
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
