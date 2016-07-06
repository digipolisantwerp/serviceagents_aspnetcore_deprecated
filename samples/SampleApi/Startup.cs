using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
                .AddJsonFile("appsettings.json", false, true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.

            //To add a single serviceAgent just use the AddSingleServiceAgent<T> extension
            //services.AddSingleServiceAgent<DemoAgent>(settings =>
            //{
            //    settings.Scheme = HttpSchema.Http;
            //    settings.Host = "localhost";
            //    settings.Port = "50267";
            //    //settings.Path = "api/";
            //    settings.AuthScheme = AuthScheme.ApiKey;
            //    settings.ApiKey = "myapikey";
            //});


            //services.AddSingleServiceAgent<OAuthDemoAgent>(settings =>
            //{
            //    settings.Scheme = HttpSchema.Https;
            //    settings.Host = "mycompany.com";
            //    settings.Port = "443";
            //    settings.Path = "testoauthtoolbox/v2";
            //    settings.AuthScheme = AuthScheme.OAuthClientCredentials;
            //    settings.OAuthClientId = "f44d3641-8249-440d-a6e5-61b7b4893184";
            //    settings.OAuthClientSecret = "2659485f-f0be-4526-bb7a-0541365351f5";
            //    settings.OAuthScope = "testoauthtoolbox.v2.all";
            //    settings.OAuthPathAddition = "oauth2/token";
            //    settings.ApiKey = "";


            //});

            //services.AddServiceAgents(settings =>
            //{
            //    settings.FileName = "serviceagentconfig.json";
            //});

            //To use a json configuration file use the AddServiceAgents extension
            services.AddServiceAgents(settings =>
            {
                settings.FileName = "serviceagents.json";
            });

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

            app.UseMvc();
        }
    }
}
