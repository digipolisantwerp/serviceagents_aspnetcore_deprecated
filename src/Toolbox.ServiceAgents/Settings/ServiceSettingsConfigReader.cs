using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Toolbox.Common.Validation;

namespace Toolbox.ServiceAgents.Settings
{
    internal class ServiceSettingsConfigReader
    {
        public ServiceAgentSettings ReadSettingsFile(IConfigurationRoot config)
        {
            var serviceAgentSettings = new ServiceAgentSettings();

            try
            {
                var sections = config.GetChildren().ToDictionary(s => s.Key);

                foreach (var item in sections)
                {
                    var properties = GetWritableProperties();

                    var settings = new ServiceSettings();

                    foreach (var property in properties)
                    {
                        var value = config.GetSection(item.Key)[property.Name];
                        property.SetValue(settings, value);
                    }
                    serviceAgentSettings.Services.Add(item.Key, settings);
                }

                return serviceAgentSettings;
            }
            catch (FormatException formatEx)
            {
                //throw AuthExceptionProvider.InvalidAuthConfigFile(formatEx);
            }
            return null;
        }

        private PropertyInfo[] GetWritableProperties()
        {
            var type = typeof(ServiceSettings);

            var properties = type.GetProperties().Where(p => p.CanWrite == true).ToArray();
            return properties;
        }
    }
}
