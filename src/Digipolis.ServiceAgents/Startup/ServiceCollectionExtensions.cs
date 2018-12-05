using Digipolis.ServiceAgents.OAuth;
using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Digipolis.ServiceAgents
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services, Action<ServiceSettings> setupAction, Assembly assembly = null) where T : AgentBase
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddSingleServiceAgent<T>(services, assembly, setupAction, null);
        }

        public static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services, Action<ServiceSettings> setupAction, Action<IServiceProvider, HttpClient> clientAction, Assembly assembly = null) where T : AgentBase
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddSingleServiceAgent<T>(services, assembly, setupAction, clientAction);
        }

        private static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services,
            Assembly callingAssembly, 
            Action<ServiceSettings> setupAction, 
            Action<IServiceProvider, HttpClient> clientAction) where T : AgentBase
        {
            if (setupAction == null) throw new ArgumentNullException(nameof(setupAction), $"{nameof(setupAction)} cannot be null.");

            // read service agent settings in local variable
            var serviceSettings = new ServiceSettings();
            setupAction.Invoke(serviceSettings);

            // configure service agent settings for DI container
            services.Configure<ServiceAgentSettings>(s =>
            {
                s.Services.Add(typeof(T).Name, serviceSettings);
            });

            ServiceAgentSettings serviceAgentSettings = new ServiceAgentSettings();
            serviceAgentSettings.Services.Add(typeof(T).Name, serviceSettings);

            RegisterServices<T>(services, serviceAgentSettings, callingAssembly, clientAction);

            return services;
        }

        public static IServiceCollection AddServiceAgents(this IServiceCollection services, Action<ServiceSettingsJsonFile> setupAction, Assembly assembly = null)
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddServiceAgents(services, assembly, setupAction, null, null);
        }

        public static IServiceCollection AddServiceAgents(this IServiceCollection services, Action<ServiceSettingsJsonFile> setupAction, Action<IServiceProvider, HttpClient> clientAction, Assembly assembly = null)
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddServiceAgents(services, assembly, setupAction, null, clientAction);
        }

        public static IServiceCollection AddServiceAgents(this IServiceCollection services, 
            Action<ServiceSettingsJsonFile> jsonSetupAction,
            Action<ServiceAgentSettings> settingsSetupAction,
            Action<IServiceProvider, HttpClient> clientAction,
            Assembly assembly = null)
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddServiceAgents(services, assembly, jsonSetupAction, settingsSetupAction, clientAction);
        }

        private static IServiceCollection AddServiceAgents(this IServiceCollection services,
            Assembly callingAssembly,
            Action<ServiceSettingsJsonFile> jsonSetupAction,
            Action<ServiceAgentSettings> settingsSetupAction,
            Action<IServiceProvider, HttpClient> clientAction)
        {
            if (jsonSetupAction == null) throw new ArgumentNullException(nameof(jsonSetupAction), $"{nameof(jsonSetupAction)} cannot be null.");

            var serviceSettingsJsonFile = new ServiceSettingsJsonFile();
            jsonSetupAction.Invoke(serviceSettingsJsonFile);

            var serviceAgentSettings = ConfigureServiceAgentSettings(services, serviceSettingsJsonFile);

            if (settingsSetupAction != null)
                settingsSetupAction.Invoke(serviceAgentSettings);

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
            var serviceAgentSettings = configReader.ReadConfig(config);

            return serviceAgentSettings;
        }

        private static void RegisterServices<T>(IServiceCollection services, ServiceAgentSettings settings, Assembly assembly, Action<IServiceProvider, HttpClient> clientAction) where T : AgentBase
        {
            //RegisterClientFactory(services, clientAction);

            RegisterAgentType(services, typeof(T), settings);

            services.AddScoped<ITokenHelper, TokenHelper>();
            services.AddScoped<IRequestHeaderHelper, RequestHeaderHelper>();
        }

        private static void RegisterServices(IServiceCollection services, ServiceAgentSettings settings, Assembly assembly, Action<IServiceProvider, HttpClient> clientAction)
        {
            //RegisterClientFactory(services, clientAction);

            var assemblyTypes = assembly.GetTypes();

            foreach (var item in settings.Services)
            {
                var type = assemblyTypes.Single(t => typeof(AgentBase).IsAssignableFrom(t.GetTypeInfo().BaseType) &&
                                                     t.Name.StartsWith(item.Key));

                RegisterAgentType(services, type, settings);
            }

            services.AddScoped<ITokenHelper, TokenHelper>();
            services.AddScoped<IRequestHeaderHelper, RequestHeaderHelper>();
        }

        //private static void RegisterClientFactory(IServiceCollection services, Action<IServiceProvider, HttpClient> clientAction)
        //{
        //    services.AddSingleton<IHttpClientFactory, HttpClientFactory>(sp =>
        //    {
        //        var factory = new HttpClientFactory(sp);

        //        if (clientAction != null)
        //            factory.AfterClientCreated += clientAction;

        //        return factory;
        //    });
        //}

        private static void RegisterAgentType(IServiceCollection services, Type implementationType, ServiceAgentSettings settings)
        {
            var interfaceType = implementationType.FindInterfaces(new TypeFilter(MyInterfaceFilter), implementationType.Name).SingleOrDefault();
            
            if (interfaceType != null)
            {
                //services.AddScoped(interfaceType, implementationType);

                // they type specified will be registered in the service collection as a transient service
                IHttpClientBuilder builder = services.AddHttpClient(implementationType.Name, (serviceProvider, client) =>
                {
                    var serviceSettings = settings.GetServiceSettings(implementationType.Name);

                    client.BaseAddress = new Uri(serviceSettings.Url);

                    IRequestHeaderHelper requestHeaderHelper = serviceProvider.GetService<IRequestHeaderHelper>();
                    requestHeaderHelper.InitializeHeaders(client, serviceSettings);

                    // invoke event
                    //AfterClientCreated?.Invoke(_serviceProvider, client);
                });

                // find the desired generic method for creating a typed HttpClient
                var genericMethods = typeof(HttpClientBuilderExtensions).GetMethods()
                    .Where(m => m.Name == "AddTypedClient"
                                && m.IsGenericMethod == true
                                && m.GetParameters().Count() == 1
                                && m.GetGenericArguments().Count() == 2).ToList();

                if (genericMethods == null || genericMethods.Count != 1) throw new Exception("Unable to find suitable method for constructing a typed HttpClient");
                
                var genericMethod = genericMethods.First().MakeGenericMethod(interfaceType, implementationType);
                genericMethod.Invoke(builder, new object[] { builder });
            }
            else
            { 
                services.AddScoped(implementationType);
            }
        }

        public static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
        {
            if (typeObj.ToString().EndsWith(criteriaObj.ToString()))
                return true;
            else
                return false;
        }
    }
}
