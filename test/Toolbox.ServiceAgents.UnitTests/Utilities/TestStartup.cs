using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.ServiceAgents.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNet.TestHost;
using Microsoft.AspNet.Http;

namespace Toolbox.ServiceAgents.UnitTests.Utilities
{
    public class TestStartup
    {
        public TestStartup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleServiceAgent<TestAgent>(settings =>
            {
                settings.Scheme = HttpSchema.Http;
                settings.Host = "test.be";
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }

        public static void Main(string[] args) => WebApplication.Run<TestStartup>(args);
    }
}
