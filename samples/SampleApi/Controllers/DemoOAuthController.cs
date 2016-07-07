using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SampleApi.ServiceAgents;
using Digipolis.ServiceAgents;

namespace SampleApi.Controllers
{
    [Route("api/[controller]")]
    public class DemoOAuthController : Controller
    {
        private IOAuthDemoAgent _serviceAgent;

        public DemoOAuthController(IOAuthDemoAgent serviceAgent)
        {
            _serviceAgent = serviceAgent;
        }

        // GET: api/demooauth
        [HttpGet]
        public async Task<string> Get()
        {
            string result = String.Empty;
            try
            {
                //The call will throw an exception since no service is listening
                //result = await _serviceAgent.GetAddressAsync(1);

                result = await _serviceAgent.GetAsStringAsync();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return result;
        }

      
    }
}
