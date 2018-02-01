using Digipolis.ServiceAgents.OAuth;
using Digipolis.ServiceAgents.Settings;
using Digipolis.ServiceAgents.UnitTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Xunit;

namespace Digipolis.ServiceAgents.UnitTests.Startup
{
    public class AddServiceAgentsTests
    {
        [Fact]
        private void ActionNullRaisesArgumentException()
        {
            Action<ServiceSettingsJsonFile> nullAction = null;
            var services = new ServiceCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => services.AddServiceAgents(nullAction));

            Assert.Equal("jsonSetupAction", ex.ParamName);
        }

        [Fact]
        private void HttpClientFactoryIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
                {
                    settings.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_1.json");
                    settings.Section = "TestAgent";
                },
                assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IHttpClientFactory))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        private void HttpClientFactoryClientActionIsPassed()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            HttpClient passedClient = null;
            IServiceProvider passedServiceProvider = null;
            var services = new ServiceCollection();
            services.AddServiceAgents(s =>
            {
                s.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_1.json");
                s.Section = "TestAgent";
            }, (serviceProvider, client) =>
            {
                passedClient = client;
                passedServiceProvider = serviceProvider;
            },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly
            );

            ///get the registrated HttpFactory
            var registration = services.Single(sd => sd.ServiceType == typeof(IHttpClientFactory));

            //Manually call the CreateClient on the factory (this normally happens when the service agent gets resolved
            var factory = registration.ImplementationFactory.Invoke(null) as HttpClientFactory;
            factory.CreateClient(serviceAgentSettings, new ServiceSettings { Host = "test.be" });

            Assert.NotNull(passedClient);
        }

        [Fact]
        private void ServiceAgentSettingsActionIsPassed()
        {
            var serviceAgentSettings = new ServiceAgentSettings();
            var services = new ServiceCollection();
            services.AddServiceAgents(json =>
            {
                json.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_1.json");
            }, settings =>
            {
                settings.Services["TestAgent"].Port = "15000";
            }, null,
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<ServiceAgentSettings>))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);

            var configOptions = registrations[0].ImplementationInstance as IConfigureOptions<ServiceAgentSettings>;
            Assert.NotNull(configOptions);

            serviceAgentSettings = new ServiceAgentSettings();
            configOptions.Configure(serviceAgentSettings);

            var serviceSettings = serviceAgentSettings.Services["TestAgent"];
            Assert.NotNull(serviceSettings);

            Assert.Equal("15000", serviceSettings.Port);
        }

        [Fact]
        private void ServiceAgentSettingsIsRegistratedAsSingleton()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_1.json");
            },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<ServiceAgentSettings>))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Singleton, registrations[0].Lifetime);
        }

        [Fact]
        private void ServiceAgentIsRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddSingleServiceAgent<TestAgent>(settings => { },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

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
                settings.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_2.json");
            },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

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
                settings.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_2.json");
            },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

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
                settings.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_3.json");
            },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(IInterfaceImplementingAgent) &&
                                                     sd.ImplementationType == typeof(InterfaceImplementingAgent))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }

        [Fact]
        private void AgentWithInheritedBaseIsRegistredAsScoped()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_5.json");
            },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(InheritingFromOtherClassAgent) &&
                                                     sd.ImplementationType == typeof(InheritingFromOtherClassAgent))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }

        [Fact]
        private void GenericAgentIsRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_4.json");
            },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(GenericAgent<>))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);

            registrations = services.Where(sd => sd.ServiceType == typeof(IConfigureOptions<ServiceAgentSettings>))
                                        .ToArray();

            var configOptions = registrations[0].ImplementationInstance as IConfigureOptions<ServiceAgentSettings>;
            Assert.NotNull(configOptions);

            var serviceAgentSettings = new ServiceAgentSettings();
            configOptions.Configure(serviceAgentSettings);

            Assert.Equal(1, serviceAgentSettings.Services.Count);

            var serviceSettings = serviceAgentSettings.Services["GenericAgent"];
            Assert.NotNull(serviceSettings);
        }

        [Fact]
        private void TokenHelperIsRegistratedAsScoped()
        {
            var services = new ServiceCollection();
            services.AddServiceAgents(settings =>
            {
                settings.FileName = Path.Combine(Directory.GetCurrentDirectory(), "_TestData/serviceagentconfig_3.json");
            },
            assembly: typeof(AddServiceAgentsTests).GetTypeInfo().Assembly);

            var registrations = services.Where(sd => sd.ServiceType == typeof(ITokenHelper) &&
                                                     sd.ImplementationType == typeof(TokenHelper))
                                        .ToArray();

            Assert.Equal(1, registrations.Count());
            Assert.Equal(ServiceLifetime.Scoped, registrations[0].Lifetime);
        }
    }
}
