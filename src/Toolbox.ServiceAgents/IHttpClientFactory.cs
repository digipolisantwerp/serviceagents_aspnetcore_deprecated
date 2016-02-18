using System.Net.Http;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents
{
    public interface IHttpClientFactory
    {
        HttpClient CreateClient(ServiceSettings settings);
    }
}