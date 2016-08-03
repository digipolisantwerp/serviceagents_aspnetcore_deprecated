using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Digipolis.ServiceAgents.Settings;
using Digipolis.ServiceAgents.UnitTests.Utilities;
using Xunit;

namespace Digipolis.ServiceAgents.UnitTests.BaseClass
{
    public class AgentBaseTests : ServiceAgentTestBase
    {
        [Fact]
        public async void Get()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.GetTestDataAsync();

            Assert.NotNull(response);
            Assert.Equal("Name", response.Name);
            Assert.Equal(150, response.Number);
        }

        [Fact]
        public async void GetAsString()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.GetTestDataAsStringAsync();

            Assert.NotNull(response);
            Assert.Equal("{\"name\":\"Name\",\"number\":150}", response);
        }

        [Fact]
        public async void Post()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.PostTestDataAsync(new TestModel { Name = "Name2", Number = 250 });

            Assert.NotNull(response);
            Assert.Equal("Name2", response.Name);
            Assert.Equal(250, response.Number);
        }

        [Fact]
        public async void PostWithOtherReturnType()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.PostTestDataWithOtherReturnTypeAsync(new TestModel { Name = "Name2", Number = 250 });

            Assert.NotNull(response);
            Assert.Equal("Name2", response.Something);
            Assert.Equal(250, response.Id);
        }

        [Fact]
        public async void Put()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            var response = await agent.PutTestDataAsync(new TestModel { Name = "Name2", Number = 250 });

            Assert.NotNull(response);
            Assert.Equal("Name2", response.Name);
            Assert.Equal(250, response.Number);
        }

        [Fact]
        public async void PutWithEmptyResult()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            await agent.PutWithEmptyResultAsync(new TestModel { Name = "Name3", Number = 350 });

            var sentData = await agent.GetPreviousDataAsync();
            Assert.NotNull(sentData);
            Assert.Equal("Name3", sentData.Name);
            Assert.Equal(350, sentData.Number);
        }

        [Fact]
        public async void Delete()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new TestAgent(serviceProvider, Options.Create(settings));
            agent.HttpClient = CreateClient();

            await agent.DeleteAsync();

            var sentData = await agent.GetPreviousDataAsync();
            Assert.NotNull(sentData);
            Assert.Equal("Deleted", sentData.Name);
            Assert.Equal(123, sentData.Number);
        }

        private ServiceAgentSettings CreateServiceAgentSettings()
        {
            var settings = new ServiceAgentSettings();
            settings.Services.Add("TestAgent", new ServiceSettings { Scheme = HttpSchema.Http, Host = "localhost" });
            return settings;
        }

        private IServiceProvider CreateServiceProvider(ServiceAgentSettings settings)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            if (settings != null)
                serviceProviderMock.Setup(p => p.GetService(typeof(IOptions<ServiceAgentSettings>))).Returns(Options.Create(settings));

            var authContextMock = new Mock<IAuthContext>();
            authContextMock.Setup(c => c.UserToken).Returns("TokenValue");

            serviceProviderMock.Setup(p => p.GetService(typeof(IAuthContext))).Returns(authContextMock.Object);
            serviceProviderMock.Setup(p => p.GetService(typeof(IHttpClientFactory))).Returns(new HttpClientFactory(serviceProviderMock.Object));


            return serviceProviderMock.Object;
        }
    }
}
