using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Toolbox.Common.Validation;

namespace Toolbox.ServiceAgents.Settings
{
    internal class ServiceSettingsFileReader
    {
        public ServiceAgentSettings ReadSettingsFile(string filePath)
        {
            ArgumentValidator.AssertNotNullOrWhiteSpace(filePath, nameof(filePath));

            var serviceAgentSettings = new ServiceAgentSettings();

            try
            {
                var config = ReadConfig(filePath);
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

        private IConfigurationRoot ReadConfig(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException($"File {filePath} does not exists.");

            var builder = new ConfigurationBuilder().SetBasePath(String.Empty);
            builder.AddJsonFile(filePath);
            var config = builder.Build();

            return config;
        }
    }
}
