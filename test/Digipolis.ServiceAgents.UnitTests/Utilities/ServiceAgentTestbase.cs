using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
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
            //var appEnv = PlatformServices.Default.Application;
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
            var hostBuilder = new WebHostBuilder().ConfigureServices(_configureServicesAction).Configure(_configureAppAction);
            return new TestServer(hostBuilder);
        }

        protected System.Net.Http.HttpClient CreateClient()
        {
            var server = CreateTestServer();
            var client = server.CreateClient();
            return client;
        }
    }
}
