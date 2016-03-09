using Microsoft.Extensions.OptionsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents.UnitTests.Utilities
{
    public class TestAgent : AgentBase
    {
        public TestAgent(IServiceProvider serviceProvider, IOptions<ServiceAgentSettings> options)
            : base(serviceProvider, options)
        {
        }

        //Expose the client in order to test the AgentBase
        public HttpClient HttpClient
        {
            get { return _client; }
            set { _client = value; }
        }

        public Task<TestModel> GetTestDataAsync()
        {
            return base.GetAsync<TestModel>($"test/1");
        }

        public Task<TestModel> GetPreviousDataAsync()
        {
            return base.GetAsync<TestModel>($"test/previousData");
        }

        public Task<TestModel> PostTestDataAsync(TestModel data)
        {
            return base.PostAsync($"test/3", data);
        }

        public Task<TestResponseModel> PostTestDataWithOtherReturnTypeAsync(TestModel data)
        {
            return base.PostAsync<TestModel, TestResponseModel>($"test/4", data);
        }

        public Task<TestModel> PutTestDataAsync(TestModel data)
        {
            return base.PutAsync<TestModel>($"test/5", data);
        }

        public Task PutWithEmptyResultAsync(TestModel data)
        {
            return base.PutWithEmptyResultAsync($"test/6", data);
        }

        public Task DeleteAsync()
        {
            return base.DeleteAsync($"test/7");
        }
    }
}
