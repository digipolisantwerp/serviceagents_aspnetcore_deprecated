using System;
using System.Collections.Generic;
using System.Linq;

namespace Digipolis.ServiceAgents.Settings
{
    public class ServiceAgentSettings
    {
        public ServiceAgentSettings()
        {
            Services = new Dictionary<string, ServiceSettings>();
        }

        public IDictionary<string, ServiceSettings> Services { get; private set; }

        /// <summary>
        /// search service agent settings by service type name
        /// </summary>
        public ServiceSettings GetServiceSettings(string typeName)
        {
            // corresponding type name and service setting key
            if (Services.Any(s => s.Key.ToLower() == typeName.ToLower()))
            {
                return Services.First(s => s.Key.ToLower() == typeName.ToLower()).Value;
            }

            // service setting key is part of type name ex. (type name) MyServiceAgent <>  (service settings key) MyService
            if (Services.Any(s => typeName.ToLower().Contains(s.Key.ToLower())))
            {
                return Services.FirstOrDefault(s => typeName.ToLower().Contains(s.Key.ToLower())).Value;
            }

            throw new Exception($"Settings not found for service agent {typeName}");
        }
    }
}
