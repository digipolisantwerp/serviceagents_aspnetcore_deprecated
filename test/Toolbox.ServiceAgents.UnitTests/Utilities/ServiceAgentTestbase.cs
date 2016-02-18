using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Toolbox.ServiceAgents.UnitTests.Utilities
{
    public class ServiceAgentTestBase
    {
        private readonly Action<IApplicationBuilder> _configureAppAction;
        private readonly Action<IServiceCollection> _configureServicesAction;
        private IHostingEnvironment _hostingEnv;
        private readonly TestStartup _startup;
        private IApplicationBuilder _app;

        public ServiceAgentTestBase()
        {
            var appEnv = CallContextServiceLocator.Locator.ServiceProvider.GetRequiredService<IApplicationEnvironment>();

            _startup = new TestStartup();

            _configureAppAction = (app =>
            {
                _app = app;
                _hostingEnv = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
                _startup.Configure(app, _hostingEnv);
            });

            _configureServicesAction = ConfigureServices;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            _startup.ConfigureServices(services);
        }

        public TestServer CreateTestServer()
        {
            return TestServer.Create(_configureAppAction, _configureServicesAction);
        }

        protected System.Net.Http.HttpClient CreateClient()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            return client;
        }
    }
}
