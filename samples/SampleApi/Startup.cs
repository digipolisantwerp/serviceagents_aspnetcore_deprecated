using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.ServiceAgents;
using SampleApi.ServiceAgents;
using Toolbox.ServiceAgents.Settings;

namespace SampleApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            builder.AddEnvironmentVariables();
            Configuration = builder.Build().ReloadOnChanged("appsettings.json");
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.

            //To add a single serviceAgent just use the AddSingleServiceAgent<T> extension
            services.AddSingleServiceAgent<DemoAgent>(settings =>
            {
                settings.Scheme = HttpSchema.Http;
                settings.Host = "test.be";
            });

            //To use a json configuration file use the AddServiceAgents extension
            //services.AddServiceAgents(settings =>
            //{
            //    settings.FileName = "serviceagents.json";
            //});

            //When combined with CorrelationId use an overload to add client behaviour (Dependency on Toolbox.WebApi required)
            //services.AddServiceAgents(settings =>
            //{
            //    settings.FileName = "serviceagents.json";
            //}, (serviceProvider, client) => client.SetCorrelationValues(serviceProvider));

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseIISPlatformHandler();

            app.UseMvc();
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
