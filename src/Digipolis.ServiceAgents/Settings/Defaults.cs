namespace Digipolis.ServiceAgents.Settings
{
    public class Defaults
    {
        public class ServiceSettingsJsonFile
        {
            public const string FileName = "serviceagentconfig.json";
        }

        public class ServiceSettings
        {
            public const string AuthScheme = Settings.AuthScheme.None;
            public const string Scheme = HttpSchema.Https;
            public const string Path = "";
        }
    }
}
