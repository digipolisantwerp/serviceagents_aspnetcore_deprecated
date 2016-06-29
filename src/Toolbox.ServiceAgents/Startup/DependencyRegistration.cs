using Microsoft.Extensions.DependencyInjection;
using Toolbox.ServiceAgents;
using Toolbox.ServiceAgents.OAuth;

namespace Toolbox.ServiceAgents
{
    public static class DependencyRegistration
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // Register your business services here, e.g. services.AddTransient<IMyService, MyService>();
            services.AddTransient<ITokenHelper, TokenHelper>();

            return services;
        }
    }
}