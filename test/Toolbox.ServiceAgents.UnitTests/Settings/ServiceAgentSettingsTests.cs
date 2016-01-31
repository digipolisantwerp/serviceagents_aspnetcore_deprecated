using System;
using Toolbox.ServiceAgents.Settings;
using Xunit;

namespace Toolbox.ServiceAgents.UnitTests.Settings
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
