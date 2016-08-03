using Digipolis.ServiceAgents.Settings;
using Digipolis.ServiceAgents.UnitTests.Utilities;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.ServiceAgents.UnitTests.Settings
{
    public class GenericAgentSettingsTest
    {
        [Fact]
        public void ShouldLoadSettings()
        {
            var settings = CreateServiceAgentSettings();
            var serviceProvider = CreateServiceProvider(settings);
            var agent = new GenericAgent<TestModel>(serviceProvider, Options.Create(settings));

            Assert.Same(settings.Services.First().Value, agent.ServiceSettings);
        }

        private ServiceAgentSettings CreateServiceAgentSettings()
        {
            var settings = new ServiceAgentSettings();
            settings.Services.Add("GenericAgent", new ServiceSettings { Scheme = HttpSchema.Http, Host = "localhost" });
            return settings;
        }

        private IServiceProvider CreateServiceProvider(ServiceAgentSettings settings)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            if (settings != null)
                serviceProviderMock.Setup(p => p.GetService(typeof(IOptions<ServiceAgentSettings>))).Returns(Options.Create(settings));

            serviceProviderMock.Setup(p => p.GetService(typeof(IHttpClientFactory))).Returns(new HttpClientFactory(serviceProviderMock.Object));

            return serviceProviderMock.Object;
        }
    }
}
