using Microsoft.Extensions.OptionsModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Toolbox.ServiceAgents.Settings;
using Toolbox.ServiceAgents.UnitTests.Utilities;
using Xunit;

namespace Toolbox.ServiceAgents.UnitTests.HttpClientFactoryTests
{
    public class HttpClientFactoryTests
    {
        [Fact]
        public void CreateDefaultClient()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var settings = new ServiceSettings { Scheme = HttpSchema.Http, Host = "test.be", Path = "api"};
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(serviceAgentSettings, settings);

            Assert.NotNull(client);
            Assert.Equal("http://test.be/api", client.BaseAddress.AbsoluteUri);
            Assert.Equal("application/json", client.DefaultRequestHeaders.Accept.Single().MediaType);
            Assert.Null(client.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public void CreateClientWithBearerAuth()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var settings = new ServiceSettings {  AuthScheme = AuthScheme.Bearer, Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(serviceAgentSettings, settings);

            Assert.NotNull(client);
            Assert.Equal("http://test.be/api", client.BaseAddress.AbsoluteUri);
            Assert.Equal("application/json", client.DefaultRequestHeaders.Accept.Single().MediaType);
            Assert.Equal(AuthScheme.Bearer, client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("TokenValue", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void CreateClientWithLocalApiKeyByDefault()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var settings = new ServiceSettings { AuthScheme = AuthScheme.ApiKey, ApiKey = "localapikey", Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(serviceAgentSettings, settings);

            Assert.NotNull(client);
            Assert.Equal("localapikey", client.DefaultRequestHeaders.First(h => h.Key == AuthScheme.ApiKey).Value.First());
        }

        [Fact]
        public void CreateClientWithGlobalApiKey()
        {
            var serviceAgentSettings = new ServiceAgentSettings { GlobalApiKey = "globalapikey" };
            var settings = new ServiceSettings { AuthScheme = AuthScheme.ApiKey, ApiKey = "localapikey", UseGlobalApiKey = true, Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(serviceAgentSettings, settings);

            Assert.NotNull(client);
            Assert.Equal("globalapikey", client.DefaultRequestHeaders.First(h => h.Key == AuthScheme.ApiKey).Value.First());
        }

        [Fact]
        public void AfterClientCreatedGetsRaised()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var settings = new ServiceSettings { AuthScheme = AuthScheme.Bearer, Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));
            HttpClient passedClient = null;
            clientFactory.AfterClientCreated += (sp,c) => passedClient = c;

            clientFactory.CreateClient(serviceAgentSettings, settings);

            Assert.NotNull(passedClient);
        }

        private IServiceProvider CreateServiceProvider(ServiceSettings settings)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            if (settings != null)
                serviceProviderMock.Setup(p => p.GetService(typeof(IOptions<ServiceSettings>))).Returns(Options.Create(settings));

            var authContextMock = new Mock<IAuthContext>();
            authContextMock.Setup(c => c.UserToken).Returns("TokenValue");

            serviceProviderMock.Setup(p => p.GetService(typeof(IAuthContext))).Returns(authContextMock.Object);

            return serviceProviderMock.Object;
        }
    }
}
