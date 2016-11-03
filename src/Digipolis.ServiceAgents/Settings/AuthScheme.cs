using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Digipolis.ServiceAgents.Settings
{
    public class AuthScheme
    {
        public const string None = "None";
        public const string Bearer = "Bearer";
        public const string OAuthClientCredentials = "OAuthClientCredentials";
        public const string Basic = "Basic";
    }
}
