using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Toolbox.ServiceAgents.Settings
{
    public class AuthSettings
    {
        public string AuthScheme {get; set;} = Defaults.ServiceSettings.AuthScheme;
        public string Domain { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
