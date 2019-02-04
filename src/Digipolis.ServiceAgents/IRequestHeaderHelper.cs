using Digipolis.ServiceAgents.Settings;
using System.Net.Http;
using System.Threading.Tasks;

namespace Digipolis.ServiceAgents
{
    public interface IRequestHeaderHelper
    {
        Task InitializeHeaders(HttpClient client, ServiceSettings settings);

        Task ValidateAuthHeaders(HttpClient client, ServiceSettings settings);
    }
}