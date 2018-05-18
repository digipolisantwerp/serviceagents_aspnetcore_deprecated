using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Digipolis.ServiceAgents.Settings;
using Xunit;
using Digipolis.ServiceAgents.OAuth;
using Digipolis.ServiceAgents.Models;
using Microsoft.AspNetCore.Hosting;

namespace Digipolis.ServiceAgents.UnitTests.HttpClientFactoryTests
{
    public class HttpClientFactoryTests
    {
        [Fact]
        public void CreateDefaultClient()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var settings = new ServiceSettings { Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(settings);

            Assert.NotNull(client);
            Assert.Equal("http://test.be/api/", client.BaseAddress.AbsoluteUri);
            Assert.Equal("application/json", client.DefaultRequestHeaders.Accept.Single().MediaType);
            Assert.Null(client.DefaultRequestHeaders.Authorization);
        }

        [Fact]
        public void CreateClientWithBearerAuth()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var settings = new ServiceSettings { AuthScheme = AuthScheme.Bearer, Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(settings);

            Assert.NotNull(client);
            Assert.Equal("http://test.be/api/", client.BaseAddress.AbsoluteUri);
            Assert.Equal("application/json", client.DefaultRequestHeaders.Accept.Single().MediaType);
            Assert.Equal(AuthScheme.Bearer, client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("TokenValue", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void CreateClientWithBasicAuthentication()
        {
            var serviceAgentSettings = new ServiceAgentSettings { };
            var settings = new ServiceSettings { AuthScheme = AuthScheme.Basic, BasicAuthUserName = "Aladdin", BasicAuthPassword = "OpenSesame", Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(settings);

            Assert.NotNull(client);
            Assert.Equal(AuthScheme.Basic, client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("QWxhZGRpbjpPcGVuU2VzYW1l", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void CreateClientWithBasicAuthenticationAndDomain()
        {
            var serviceAgentSettings = new ServiceAgentSettings { };
            var settings = new ServiceSettings { AuthScheme = AuthScheme.Basic, BasicAuthDomain = "ICA", BasicAuthUserName = "Aladdin", BasicAuthPassword = "OpenSesame", Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(settings);

            Assert.NotNull(client);
            Assert.Equal(AuthScheme.Basic, client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("SUNBXEFsYWRkaW46T3BlblNlc2FtZQ==", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void CreateClientWithOAuthClientCredentials()
        {
            var serviceAgentSettings = new ServiceAgentSettings { };
            var settings = new ServiceSettings { AuthScheme = AuthScheme.OAuthClientCredentials, OAuthClientId = "clientId", OAuthClientSecret = "clientSecret", Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(settings);

            Assert.NotNull(client);
            Assert.Equal(AuthScheme.Bearer, client.DefaultRequestHeaders.Authorization.Scheme);
            Assert.Equal("AccessToken", client.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Fact]
        public void ThrowExceptionWhenNonHttpsSchemeUsedWithBasicAuthentication()
        {
            var serviceAgentSettings = new ServiceAgentSettings { };
            var settings = new ServiceSettings { AuthScheme = AuthScheme.Basic, BasicAuthUserName = "Aladdin", BasicAuthPassword = "OpenSesame", Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            Assert.Throws<ServiceAgentException>(() => clientFactory.CreateClient(settings));
        }

        [Fact]
        public void DoesntThrowExceptionWhenNonHttpsSchemeUsedWithBasicAuthenticationInDevelopmentEnvironment()
        {
            var serviceAgentSettings = new ServiceAgentSettings { };
            var settings = new ServiceSettings { AuthScheme = AuthScheme.Basic, BasicAuthUserName = "Aladdin", BasicAuthPassword = "OpenSesame", Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings, isDevelopmentEnvironment: true));

            clientFactory.CreateClient(settings);
        }

        [Fact]
        public void AfterClientCreatedGetsRaised()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var settings = new ServiceSettings { AuthScheme = AuthScheme.Bearer, Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));
            HttpClient passedClient = null;
            clientFactory.AfterClientCreated += (sp, c) => passedClient = c;

            clientFactory.CreateClient(settings);

            Assert.NotNull(passedClient);
        }

        [Fact]
        public void CreateClientWithHeaders()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var headers = new Dictionary<string, string>()
            {
                { "api-key", "localapikey" },
                 { "X-Custom-Header", "customvalue" },
            };
            var settings = new ServiceSettings { Headers = headers, Scheme = HttpSchema.Http, Host = "test.be", Path = "api" };
            var clientFactory = new HttpClientFactory(CreateServiceProvider(settings));

            var client = clientFactory.CreateClient(settings);

            Assert.NotNull(client);
            Assert.Equal("localapikey", client.DefaultRequestHeaders.First(h => h.Key == "api-key").Value.First());
            Assert.Equal("customvalue", client.DefaultRequestHeaders.First(h => h.Key == "X-Custom-Header").Value.First());
        }

        private IServiceProvider CreateServiceProvider(ServiceSettings settings, bool isDevelopmentEnvironment = false)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            if (settings != null)
                serviceProviderMock.Setup(p => p.GetService(typeof(IOptions<ServiceSettings>))).Returns(Options.Create(settings));

            var authContextMock = new Mock<IAuthContext>();
            authContextMock.Setup(c => c.UserToken).Returns("TokenValue");

            serviceProviderMock.Setup(p => p.GetService(typeof(IAuthContext))).Returns(authContextMock.Object);

            var mockTokenHelper = new Mock<ITokenHelper>();
            mockTokenHelper.Setup(h => h.ReadOrRetrieveToken(settings))
                .ReturnsAsync(new TokenReply { access_token = "AccessToken", expires_in = 7200 });

            serviceProviderMock.Setup(p => p.GetService(typeof(ITokenHelper))).Returns(mockTokenHelper.Object);

            var mockHostingEnvironment = new Mock<IHostingEnvironment>();
            mockHostingEnvironment.Setup(h => h.EnvironmentName)
                .Returns(isDevelopmentEnvironment ? "Development" : "");

            serviceProviderMock.Setup(p => p.GetService(typeof(IHostingEnvironment))).Returns(mockHostingEnvironment.Object);

            return serviceProviderMock.Object;
        }
    }
}
