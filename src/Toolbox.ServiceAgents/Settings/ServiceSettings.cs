using System;

namespace Toolbox.ServiceAgents.Settings
{
    public class ServiceSettings
    {
        public AuthSettings AuthSettings { get; set; } = new AuthSettings();
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
