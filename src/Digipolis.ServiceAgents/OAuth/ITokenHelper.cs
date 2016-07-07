using System.Threading.Tasks;
using Digipolis.ServiceAgents.Models;
using Digipolis.ServiceAgents.Settings;

namespace Digipolis.ServiceAgents.OAuth
{
    public interface ITokenHelper
    {
        Task<TokenReply> ReadOrRetrieveToken(ServiceSettings options, bool forceNewRetrieval = false);
    }
}