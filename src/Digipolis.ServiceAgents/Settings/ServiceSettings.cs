using System;
using System.Collections.Generic;

namespace Digipolis.ServiceAgents.Settings
{
    public class ServiceSettings
    {
        public string AuthScheme { get; set; } = Defaults.ServiceSettings.AuthScheme;
        public string Scheme { get; set; } = Defaults.ServiceSettings.Scheme;
        public string Host { get; set; }
        public string Port { get; set; }
        public string Path { get; set; } = Defaults.ServiceSettings.Path;

        public string OAuthPathAddition { get; set; }

        public string OAuthClientId { get; set; }

        public string OAuthClientSecret { get; set; }
        public string OAuthScope { get; set; }

        /// <summary>
        /// The optional domain used for basic authentication scheme.
        /// </summary>
        public string BasicAuthDomain { get; set; }

        /// <summary>
        /// The user name used for basic authentication scheme.
        /// </summary>
        public string BasicAuthUserName { get; set; }

        /// <summary>
        /// The password used for basic authentication scheme.
        /// </summary>
        public string BasicAuthPassword { get; set; }

        /// <summary>
        /// A key value collection representing the headers to be added to the requests.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        public string Url
        {
            get
            {
                return $"{Scheme}://{Host}{(String.IsNullOrWhiteSpace(Port) ? "" : $":{ Port}")}/{Path}{(String.IsNullOrWhiteSpace(Path) ? "" : "/")}";
            }
        }

        public string OAuthTokenEndpoint
        {
            get
            {
                return $"{Url}{OAuthPathAddition}";
            }
        }

    }
}
