using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using SampleApi.ServiceAgents;
using Toolbox.ServiceAgents;

namespace SampleApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private IDemoAgent _serviceAgent;

        public ValuesController(IDemoAgent serviceAgent)
        {
            _serviceAgent = serviceAgent;
        }

        // GET: api/values
        [HttpGet]
        public async Task<string> Get()
        {
            try
            {
                //The call will throw an exception since no service is listening
                var result = await _serviceAgent.GetAddressAsync(1);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "Get completed";
        }

    }
}
