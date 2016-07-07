using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Http;

namespace Digipolis.ServiceAgents.UnitTests.Utilities
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

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
            .UseStartup<TestStartup>()
            .Build();
            host.Run();
        }

        //=> WebApplication.Run<TestStartup>(args);
    }
}
