using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services, Action<ServiceSettings> setupAction) where T : AgentBase
        {
            return AddSingleServiceAgent<T>(services, Assembly.GetCallingAssembly(), setupAction, null);
        }

        public static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services, Action<ServiceSettings> setupAction, Action<IServiceProvider, HttpClient> clientAction) where T : AgentBase
        {
            return AddSingleServiceAgent<T>(services, Assembly.GetCallingAssembly(), setupAction, clientAction);
        }

        private static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services,
            Assembly callingAssembly, 
            Action<ServiceSettings> setupAction, 
            Action<IServiceProvider, HttpClient> clientAction) where T : AgentBase
        {
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction), $"{nameof(setupAction)} cannot be null.");

            var serviceSettings = new ServiceSettings();
            setupAction.Invoke(serviceSettings);

            var type = typeof(T);

            services.Configure<ServiceAgentSettings>(s =>
            {
                s.Services.Add(type.Name, serviceSettings);
            });

            RegisterServices<T>(services, callingAssembly, clientAction);

            return services;
        }

        public static IServiceCollection AddServiceAgents(this IServiceCollection services, Action<ServiceSettingsJsonFile> setupAction)
        {
            return AddServiceAgents(services, Assembly.GetCallingAssembly(), setupAction, null);
        }

        public static IServiceCollection AddServiceAgents(this IServiceCollection services, Action<ServiceSettingsJsonFile> setupAction, Action<IServiceProvider, HttpClient> clientAction)
        {
            return AddServiceAgents(services, Assembly.GetCallingAssembly(), setupAction, clientAction);
        }

        private static IServiceCollection AddServiceAgents(this IServiceCollection services,
            Assembly callingAssembly, 
            Action<ServiceSettingsJsonFile> setupAction, 
            Action<IServiceProvider, HttpClient> clientAction)
        {
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction), $"{nameof(setupAction)} cannot be null.");

            var serviceSettingsJsonFile = new ServiceSettingsJsonFile();
            setupAction.Invoke(serviceSettingsJsonFile);

            var serviceAgentSettings = ConfigureServiceAgentSettings(services, serviceSettingsJsonFile);
            services.Configure<ServiceAgentSettings>(s =>
            {
                foreach (var item in serviceAgentSettings.Services)
                {
                    s.Services.Add(item.Key, item.Value);
                }
            });

            RegisterServices(services, serviceAgentSettings, callingAssembly, clientAction);

            return services;
        }

        private static ServiceAgentSettings ConfigureServiceAgentSettings(IServiceCollection services, ServiceSettingsJsonFile serviceSettingsJsonFile)
        {
            var builder = new ConfigurationBuilder().AddJsonFile(serviceSettingsJsonFile.FileName);
            var config = builder.Build();
            
            var configReader = new ServiceSettingsConfigReader();
            //Check niet nodig ofdat file bestaat gebeurt hierboven al
            var serviceAgentSettings = configReader.ReadSettingsFile(config);

            return serviceAgentSettings;
        }

        private static void RegisterServices<T>(IServiceCollection services, Assembly assembly, Action<IServiceProvider, HttpClient> clientAction) where T : AgentBase
        {
            RegisterClientFactory(services, clientAction);

            RegisterAgentType(services, assembly.GetTypes(), typeof(T));
        }

        private static void RegisterServices(IServiceCollection services, ServiceAgentSettings settings, Assembly assembly, Action<IServiceProvider, HttpClient> clientAction)
        {
            RegisterClientFactory(services, clientAction);

            var assemblyTypes = assembly.GetTypes();

            foreach (var item in settings.Services)
            {
                var type = assemblyTypes.Single(t => t.BaseType == typeof(AgentBase) &&
                                                    t.Name == item.Key);

                RegisterAgentType(services, assemblyTypes, type);
            }
        }

        private static void RegisterClientFactory(IServiceCollection services, Action<IServiceProvider, HttpClient> clientAction)
        {
            services.AddScoped<IHttpClientFactory, HttpClientFactory>(sp =>
            {
                var factory = new HttpClientFactory(sp);

                if (clientAction != null)
                    factory.AfterClientCreated += clientAction;

                return factory;
            });
        }

        private static void RegisterAgentType(IServiceCollection services, Type[] assemblyTypes, Type implementationType)
        {
            var interfaceTypeName = $"I{implementationType.Name}";
            var interfaceType = assemblyTypes.SingleOrDefault(t => t.Name == interfaceTypeName && t.IsInterface);

            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
            else
            {
                services.AddScoped(implementationType);
            }
        }

        private static Type TryGetInterface(Type[] assemblyTypes, Type type)
        {
            var interfaceTypeName = $"I{type.Name}";
            var interfaceType = assemblyTypes.SingleOrDefault(t => t.Name == interfaceTypeName && t.IsInterface);
            return interfaceType;
        }
    }
}
