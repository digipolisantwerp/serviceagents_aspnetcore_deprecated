using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Toolbox.ServiceAgents;
using Microsoft.Extensions.OptionsModel;
using Toolbox.ServiceAgents.Settings;
using Toolbox.ServiceAgents.UnitTests.Utilities;
using System.Net.Http;

namespace Toolbox.ServiceAgents.UnitTests.Startup
{
    public class AddServiceAgentsTests
    {
        [Fact]
        private void ActionNullRaisesArgumentException()
        {
            Action<ServiceSettingsJsonFile> nullAction = null;
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => services.AddServiceAgents(nullAction));

            Assert.Equal("setupAction", ex.ParamName);
        }

        [Fact]
        private void HttpClientFactoryIsRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = "_TestData/serviceagentconfig_1.json";
                settings.Section = "TestAgent";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IHttpClientFactory))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }

        [Fact]
        private void HttpClientFactoryClientActionIsPassed()
        {
            HttpClient passedClient = null;
            IServiceProvider passedServiceProvider = null;
            var services = new ServiceCollection();
            services.AddServiceAgents(s =>
            {
                s.FileName = "_TestData/serviceagentconfig_1.json";
                s.Section = "TestAgent";
            }, (serviceProvider, client) =>
            {
                passedClient = client;
                passedServiceProvider = serviceProvider;
            });

            ///get the registrated HttpFactory
            var registration = services.Single(sd => sd.ServiceType == typeof(IHttpClientFactory));

            //Manually call the CreateClient on the factory (this normally happens when the service agent gets resolved
            var factory = registration.ImplementationFactory.Invoke(null) as HttpClientFactory;
            factory.CreateClient(new ServiceSettings {  Host = "test.be" });

            Assert.NotNull(passedClient);
        }

        [Fact]
        private void ServiceAgentSettingsIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = "_TestData/serviceagentconfig_1.json";
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

            Assert.Equal(AuthScheme.None, serviceSettings.AuthScheme);
            Assert.Equal("test.be", serviceSettings.Host);
            Assert.Equal("api", serviceSettings.Path);
            Assert.Equal("5001", serviceSettings.Port);
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
        private void MultipleServiceAgents()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = "_TestData/serviceagentconfig_2.json";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<ServiceAgentSettings>))
                                        .ToArray();

            var configOptions = registrations[0].ImplementationInstance as IConfigureOptions<ServiceAgentSettings>;
            Assert.NotNull(configOptions);

            var serviceAgentSettings = new ServiceAgentSettings();
            configOptions.Configure(serviceAgentSettings);

            Assert.Equal(2, serviceAgentSettings.Services.Count);

            var serviceSettings = serviceAgentSettings.Services["TestAgent"];
            Assert.NotNull(serviceSettings);

            Assert.Equal("None", serviceSettings.AuthScheme);
            Assert.Equal("test.be", serviceSettings.Host);
            Assert.Equal("api", serviceSettings.Path);
            Assert.Equal("5001", serviceSettings.Port);
            Assert.Equal(HttpSchema.Http, serviceSettings.Scheme);

            serviceSettings = serviceAgentSettings.Services["OtherTestAgent"];
            Assert.NotNull(serviceSettings);

            Assert.Equal(AuthScheme.Bearer, serviceSettings.AuthScheme);
            Assert.Equal("other.be", serviceSettings.Host);
            Assert.Equal("path", serviceSettings.Path);
            Assert.Equal("5002", serviceSettings.Port);
            Assert.Equal(HttpSchema.Https, serviceSettings.Scheme);
        }

        [Fact]
        private void MultipleServiceAgentsAreRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = "_TestData/serviceagentconfig_2.json";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(TestAgent) ||
                                                     sd.ServiceType == typeof(OtherTestAgent))
                                        .ToArray();

            Assert.Equal(2, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
            Assert.Equal(nameof(OtherTestAgent), registrations[0].ServiceType.Name);

            Assert.Equal(ServiceLifetime.Scoped, registrations[1].Lifetime);
            Assert.Equal(nameof(TestAgent), registrations[1].ServiceType.Name);
        }

        [Fact]
        private void ServiceAgentInterfaceIsRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = "_TestData/serviceagentconfig_3.json";
            });

            var registrations = services.Where(sd => sd.ServiceType == typeof(IInterfaceImplementingAgent) &&
                                                     sd.ImplementationType == typeof(InterfaceImplementingAgent))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }
    }
}
