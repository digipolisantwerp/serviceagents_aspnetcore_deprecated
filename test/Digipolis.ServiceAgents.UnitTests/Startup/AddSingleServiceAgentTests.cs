using Digipolis.ServiceAgents.OAuth;
using Digipolis.ServiceAgents.Settings;
using Digipolis.ServiceAgents.UnitTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Xunit;

namespace Digipolis.ServiceAgents.UnitTests.Startup
{
    public class AddSingleServiceAgentTests
    {
        [Fact]
        private void ActionNullRaisesArgumentException()
        {
            Action<ServiceSettings> nullAction = null;
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => services.AddSingleServiceAgent<TestAgent>(nullAction));

            Assert.Equal("setupAction", ex.ParamName);
        }

        [Fact]
        private void HttpClientFactoryIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddSingleServiceAgent<TestAgent>(settings => { });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IHttpClientFactory))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        private void HttpClientFactoryClientActionIsPassed()
        {
            HttpClient passedClient = null;
            var services = new ServiceCollection();
            services.AddSingleServiceAgent<TestAgent>(settings => { }, (sp, client) => passedClient = client);

            ///get the registrated HttpFactory
            var registration = services.Single(sd => sd.ServiceType == typeof(IHttpClientFactory));

            //Manually call the CreateClient on the factory (this normally happens when the service agent gets resolved
            var factory = registration.ImplementationFactory.Invoke(services.BuildServiceProvider()) as HttpClientFactory;
            factory.CreateClient(new ServiceSettings { Host = "test.be" });

            Assert.NotNull(passedClient);
        }

        [Fact]
        private void ServiceAgentSettingsIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddSingleServiceAgent<TestAgent>(settings =>
            {
                settings.AuthScheme = AuthScheme.Bearer;
                settings.Host = "test.be";
                settings.Path = "api";
                settings.Port = "5000";
                settings.Scheme = HttpSchema.Http;
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<ServiceAgentSettings>))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);

            var configOptions = registrations[0].ImplementationInstance as IConfigureOptions<ServiceAgentSettings>;
            Assert.NotNull(configOptions);

            var serviceAgentSettings = new ServiceAgentSettings();
            configOptions.Configure(serviceAgentSettings);

            Assert.Equal(1, serviceAgentSettings.Services.Count);

            var serviceSettings = serviceAgentSettings.Services["TestAgent"];
            Assert.NotNull(serviceSettings);

            Assert.Equal(AuthScheme.Bearer, serviceSettings.AuthScheme);
            Assert.Equal("test.be", serviceSettings.Host);
            Assert.Equal("api", serviceSettings.Path);
            Assert.Equal("5000", serviceSettings.Port);
            Assert.Equal(HttpSchema.Http, serviceSettings.Scheme);
        }

        [Fact]
        private void ServiceAgentIsRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddSingleServiceAgent<TestAgent>(settings => { });

            var registrations = services.Where(sd => sd.ServiceType == typeof(TestAgent) &&
                                                     sd.ImplementationType == typeof(TestAgent))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }

        [Fact]
        private void ServiceAgentInterfaceIsRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddSingleServiceAgent<InterfaceImplementingAgent>(servicSettings => { },
                assembly: typeof(InterfaceImplementingAgent).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IInterfaceImplementingAgent) &&
                                                     sd.ImplementationType == typeof(InterfaceImplementingAgent))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }

        [Fact]
        private void TokenHelperIsRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddSingleServiceAgent<InterfaceImplementingAgent>(settings => { },
                assembly: typeof(InterfaceImplementingAgent).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ITokenHelper) &&
                                                     sd.ImplementationType == typeof(TokenHelper))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
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
            serviceProviderMock.Setup(p => p.GetService(typeof(IRequestHeaderHelper))).Returns(new RequestHeaderHelper(serviceProviderMock.Object));

            return serviceProviderMock.Object;
        }
    }
}
