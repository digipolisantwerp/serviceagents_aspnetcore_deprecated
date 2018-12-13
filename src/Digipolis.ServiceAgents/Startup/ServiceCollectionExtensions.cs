using Digipolis.ServiceAgents.OAuth;
using Digipolis.ServiceAgents.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Digipolis.ServiceAgents
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services, 
                                                                  Action<ServiceSettings> serviceSettingsSetupAction, 
                                                                  Assembly assembly = null) where T : AgentBase
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddSingleServiceAgent<T>(services: services,
                                            serviceSettingsSetupAction: serviceSettingsSetupAction,
                                            clientCreatedAction: null, 
                                            clientBuildAction : null, 
                                            callingAssembly: assembly);
        }

        public static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services, 
                                                                  Action<ServiceSettings> serviceSettingsSetupAction, 
                                                                  Action<IServiceProvider, HttpClient> clientCreatedAction,
                                                                  Action<string, IHttpClientBuilder> clientBuildAction = null,
                                                                  Assembly assembly = null) where T : AgentBase
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddSingleServiceAgent<T>(services: services,
                                            serviceSettingsSetupAction: serviceSettingsSetupAction,
                                            clientCreatedAction: clientCreatedAction,
                                            clientBuildAction: clientBuildAction,
                                            callingAssembly: assembly);
        }

        private static IServiceCollection AddSingleServiceAgent<T>(this IServiceCollection services,
                                                                   Assembly callingAssembly,
                                                                   Action<ServiceSettings> serviceSettingsSetupAction, 
                                                                   Action<IServiceProvider, HttpClient> clientCreatedAction,
                                                                   Action<string, IHttpClientBuilder> clientBuildAction = null) where T : AgentBase
        {
            if (serviceSettingsSetupAction == null) throw new ArgumentNullException(nameof(serviceSettingsSetupAction), $"{nameof(serviceSettingsSetupAction)} cannot be null.");

            // read service agent settings in local variable
            var serviceSettings = new ServiceSettings();
            serviceSettingsSetupAction.Invoke(serviceSettings);

            // configure service agent settings for DI container
            services.Configure<ServiceAgentSettings>(s =>
            {
                s.Services.Add(typeof(T).Name, serviceSettings);
            });

            ServiceAgentSettings serviceAgentSettings = new ServiceAgentSettings();
            serviceAgentSettings.Services.Add(typeof(T).Name, serviceSettings);

            RegisterServices<T>(services, serviceAgentSettings, callingAssembly, clientCreatedAction, clientBuildAction);

            return services;
        }
        
        public static IServiceCollection AddServiceAgents(this IServiceCollection services, 
                                                          Action<ServiceSettingsJsonFile> jsonConfigurationFileSetupAction, 
                                                          Assembly assembly = null)
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddServiceAgents(services: services,
                                    jsonConfigurationFileSetupAction: jsonConfigurationFileSetupAction,
                                    settingsSetupAction: null,
                                    clientCreatedAction: null,
                                    clientBuildAction: null,
                                    callingAssembly: assembly);
        }
                
        public static IServiceCollection AddServiceAgents(this IServiceCollection services,
                                                          Action<ServiceSettingsJsonFile> jsonConfigurationFileSetupAction,
                                                          Action<IServiceProvider, HttpClient> clientCreatedAction,
                                                          Action<string, IHttpClientBuilder> clientBuildAction = null,
                                                          Assembly assembly = null)
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddServiceAgents(services: services,
                                    jsonConfigurationFileSetupAction: jsonConfigurationFileSetupAction,
                                    settingsSetupAction: null,
                                    clientCreatedAction: clientCreatedAction,
                                    clientBuildAction: clientBuildAction,
                                    callingAssembly: assembly);
        }

        public static IServiceCollection AddServiceAgents(this IServiceCollection services, 
                                                          Action<ServiceSettingsJsonFile> jsonConfigurationFileSetupAction,
                                                          Action<ServiceAgentSettings> serviceSettingsSetupAction,
                                                          Action<IServiceProvider, HttpClient> clientCreatedAction,
                                                          Action<string, IHttpClientBuilder> clientBuildAction = null,
                                                          Assembly assembly = null)
        {
            assembly = assembly == null ? Assembly.GetEntryAssembly() : assembly;
            return AddServiceAgents(services: services,
                                    jsonConfigurationFileSetupAction: jsonConfigurationFileSetupAction,
                                    settingsSetupAction: serviceSettingsSetupAction,
                                    clientCreatedAction: clientCreatedAction,
                                    clientBuildAction: clientBuildAction,
                                    callingAssembly: assembly);
        }

        private static IServiceCollection AddServiceAgents(this IServiceCollection services,
            Assembly callingAssembly,
            Action<ServiceSettingsJsonFile> jsonConfigurationFileSetupAction,
            Action<ServiceAgentSettings> settingsSetupAction,
            Action<IServiceProvider, HttpClient> clientCreatedAction,
            Action<string, IHttpClientBuilder> clientBuildAction)
        {
            if (jsonConfigurationFileSetupAction == null) throw new ArgumentNullException(nameof(jsonConfigurationFileSetupAction), $"{nameof(jsonConfigurationFileSetupAction)} cannot be null.");

            var serviceSettingsJsonFile = new ServiceSettingsJsonFile();
            jsonConfigurationFileSetupAction.Invoke(serviceSettingsJsonFile);

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

            RegisterServices(services, serviceAgentSettings, callingAssembly, clientCreatedAction, clientBuildAction);

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

        private static void RegisterServices<T>(IServiceCollection services, 
                                    ServiceAgentSettings settings, 
                                    Assembly assembly, 
                                    Action<IServiceProvider, HttpClient> clientCreatedAction,
                                    Action<string, IHttpClientBuilder> clientBuildAction) where T : AgentBase
        {
            RegisterAgentType<T>(services, settings, clientCreatedAction, clientBuildAction);

            RegisterCommonServices(services);
        }

        private static void RegisterServices(IServiceCollection services, 
                                ServiceAgentSettings settings, 
                                Assembly assembly, 
                                Action<IServiceProvider, HttpClient> clientCreatedAction,
                                Action<string, IHttpClientBuilder> clientBuildAction)
        {
            var assemblyTypes = assembly.GetTypes();

            foreach (var item in settings.Services)
            {
                var type = assemblyTypes.Single(t => typeof(AgentBase).IsAssignableFrom(t.GetTypeInfo().BaseType) &&
                                                     t.Name.StartsWith(item.Key));

                RegisterAgentType(services, type, settings, clientCreatedAction, clientBuildAction);
            }

            RegisterCommonServices(services);
        }

        private static void RegisterCommonServices(IServiceCollection services)
        {
            services.AddScoped<ITokenHelper, TokenHelper>();
            services.AddScoped<IRequestHeaderHelper, RequestHeaderHelper>();
        }

        private static void RegisterAgentType(IServiceCollection services, 
                                              Type implementationType, 
                                              ServiceAgentSettings settings, 
                                              Action<IServiceProvider, HttpClient> clientCreatedAction,
                                              Action<string, IHttpClientBuilder> clientBuildAction)
        {
            // registering service agents with generic parameters causes an error: "Late bound operations cannot be performed on types or methods for which ContainsGenericParameters is true."
            if (implementationType.ContainsGenericParameters) throw new Exception($"Error registering service {implementationType.Name}. A service agent with generic parameters can't be registered at runtime and must be registered with the service collection via the AddSingleServiceAgent<T> method");

            // the specified type will be registered in the service collection as a transient service
            IHttpClientBuilder builder = services.AddHttpClient(implementationType.Name, (serviceProvider, client) =>
            {
                var serviceSettings = settings.GetServiceSettings(implementationType.Name);

                ConfigureHttpClient(serviceProvider, client, serviceSettings, clientCreatedAction);
            });
            
            var interfaceType = implementationType.FindInterfaces(new TypeFilter(MyInterfaceFilter), implementationType.Name).SingleOrDefault();
            
            if (interfaceType != null)
            {
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
                // find the desired generic method for creating a typed HttpClient
                var genericMethods = typeof(HttpClientBuilderExtensions).GetMethods()
                    .Where(m => m.Name == "AddTypedClient"
                                && m.IsGenericMethod == true
                                && m.GetParameters().Count() == 1
                                && m.GetGenericArguments().Count() == 1).ToList();

                if (genericMethods == null || genericMethods.Count != 1) throw new Exception("Unable to find suitable method for constructing a typed HttpClient");

                var genericMethod = genericMethods.First().MakeGenericMethod(implementationType);
                genericMethod.Invoke(builder, new object[] { builder });
            }

            // invoke additional actions for HttpClientBuilder ex. attaching DelegatingHandlers
            clientBuildAction?.Invoke(implementationType.Name, builder);
        }

        private static void RegisterAgentType<T>(IServiceCollection services,
                                              ServiceAgentSettings settings,
                                              Action<IServiceProvider, HttpClient> clientCreatedAction,
                                              Action<string, IHttpClientBuilder> clientBuildAction) where T: class
        {
            // the type specified will be registered in the service collection as a transient service
            IHttpClientBuilder builder = services.AddHttpClient<T>(typeof(T).Name, (serviceProvider, client) =>
            {
                var serviceSettings = settings.GetServiceSettings(typeof(T).Name);

                ConfigureHttpClient(serviceProvider, client, serviceSettings, clientCreatedAction);
            });

            // invoke additional actions for HttpClientBuilder ex. attaching DelegatingHandlers
            clientBuildAction?.Invoke(typeof(T).Name, builder);
        }

        private static void ConfigureHttpClient(IServiceProvider serviceProvider, HttpClient client, ServiceSettings serviceSettings, Action<IServiceProvider, HttpClient> clientCreatedAction)
        {
            client.BaseAddress = new Uri(serviceSettings.Url);

            IRequestHeaderHelper requestHeaderHelper = serviceProvider.GetService<IRequestHeaderHelper>();
            requestHeaderHelper.InitializeHeaders(client, serviceSettings).Wait();

            // invoke event
            clientCreatedAction?.Invoke(serviceProvider, client);
        }

        private static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
        {
            if (typeObj.ToString().EndsWith(criteriaObj.ToString()))
                return true;
            else
                return false;
        }

    }
}
