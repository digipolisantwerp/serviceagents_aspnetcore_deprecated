using System;

namespace Toolbox.ServiceAgents.Settings
{
    public class ServiceSettings
    {
        public string AuthScheme { get; set; } = Defaults.ServiceSettings.AuthScheme;
        public string Scheme { get; set; } = Defaults.ServiceSettings.Scheme;
        public string Host { get; set; }
        public string Port { get; set; }
        public string Path { get; set; } = Defaults.ServiceSettings.Path;

        public string Url
        {
            get
            {
                return $"{Scheme}://{Host}{(String.IsNullOrWhiteSpace(Port) ? "" : $":{ Port}")}/{Path}";
            }
        }
    }
}
