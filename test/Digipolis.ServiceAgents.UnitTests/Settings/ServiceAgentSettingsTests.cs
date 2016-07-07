using System;
using Digipolis.ServiceAgents.Settings;
using Xunit;

namespace Digipolis.ServiceAgents.UnitTests.Settings
{
    public class ServiceAgentSettingsTests
    {
        [Fact]
        private void ServicesIsInitialized()
        {
            var settings = new ServiceAgentSettings();
            Assert.NotNull(settings.Services);
        }
    }
}
