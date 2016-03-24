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

        /// <summary>
        /// A bool to indicate if the globaly defined api key should be used for this service agent when the AuthScheme is set to 'ApiKey'. 
        /// The api key is set as header with key 'apikey'.
        /// </summary>
        public bool UseGlobalApiKey { get; set; }

        /// <summary>
        /// The api key to use for this service agent when the AuthScheme is set to 'ApiKey' and the UseGlobalApiKey is set to 'false'. 
        /// The api key is set as header with key 'apikey'.
        /// </summary>
        public string ApiKey { get; set; }

        public string Url
        {
            get
            {
                return $"{Scheme}://{Host}{(String.IsNullOrWhiteSpace(Port) ? "" : $":{ Port}")}/{Path}";
            }
        }
    }
}
