using Digipolis.ServiceAgents.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    public class TestOAuthController : Controller
    {
        private static TestModel _previousData;

      
        [HttpPost("api/oauth/token")]
        public TokenReply Post()
        {
            return new TokenReply { access_token = "accessToken", expires_in = 7200 };
        }

        [HttpPost("api/oauth/nocontent")]
        public object PostWithoutContent()
        {
            return "{ invalid json: ";
        }
    }
}
