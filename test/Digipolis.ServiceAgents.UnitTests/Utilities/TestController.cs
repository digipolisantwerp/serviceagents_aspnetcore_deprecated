using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        private static TestModel _previousData; 

        [HttpGet("1")]
        public TestModel Get()
        {
            return new TestModel();
        }

        [HttpGet("previousData")]
        public TestModel GetPreviousData()
        {
            return _previousData;
        }

        [HttpGet("2")]
        public Task GetLongRunning()
        {
            return Task.Delay(5000);
        }

        [HttpPost("3")]
        public TestModel Post([FromBody]TestModel data)
        {
            return data;
        }

        [HttpPost("4")]
        public TestResponseModel PostWithOtherReturnType([FromBody]TestModel data)
        {
            return new TestResponseModel { Something = data.Name, Id = data.Number };
        }

        [HttpPut("5")]
        public TestModel Put([FromBody]TestModel data)
        {
            return data;
        }

        [HttpPut("6")]
        public void PutWithoutReturnData([FromBody]TestModel data)
        {
            _previousData = data;
        }

        [HttpDelete("7")]
        public void Delete()
        {
            _previousData = new TestModel { Name = "Deleted", Number = 123 };
        }
    }
}
