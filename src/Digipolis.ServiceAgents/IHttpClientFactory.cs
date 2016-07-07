using System.Net.Http;
using Digipolis.ServiceAgents.Settings;

namespace Digipolis.ServiceAgents
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient(ServiceAgentSettings serviceAgentSettings, ServiceSettings settings);
    }
}