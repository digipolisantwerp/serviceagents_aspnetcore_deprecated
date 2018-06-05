using Digipolis.ServiceAgents.Settings;
using System.Net.Http;

namespace Digipolis.ServiceAgents
{
    public interface IRequestHeaderHelper
    {
        void InitializeHeaders(HttpClient client, ServiceSettings settings);

        void ValidateAuthHeaders(HttpClient client, ServiceSettings settings);
    }
}