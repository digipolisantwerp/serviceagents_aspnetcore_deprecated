using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.ServiceAgents.Settings;
using Xunit;

namespace Toolbox.ServiceAgents.UnitTests.Settings
{
    public class ServiceSettingsTests
    {
        [Fact]
        public void Defaults()
        {
            var settings = new ServiceSettings();

            Assert.Equal(AuthScheme.None, settings.AuthScheme);
            Assert.Equal(HttpSchema.Https, settings.Scheme);
            Assert.Equal("api", settings.Path);
        }

        [Fact]
        public void ConstuctUrl()
        {
            var settings = new ServiceSettings
            {
                Scheme = HttpSchema.Http,
                Host = "test.com",
                Port = "80",
                Path = "api"
            };

            Assert.Equal("http://test.com:80/api", settings.Url);
        }

        [Fact]
        public void ConstuctUrlWithoutPort()
        {
            var settings = new ServiceSettings
            {
                Scheme = HttpSchema.Http,
                Host = "test.com",
                Path = "api"
            };

            Assert.Equal("http://test.com/api", settings.Url);
        }
    }
}
