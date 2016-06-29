using System.Threading.Tasks;
using Toolbox.ServiceAgents.Models;
using Toolbox.ServiceAgents.Settings;

namespace Toolbox.ServiceAgents.OAuth
{
    public interface ITokenHelper
    {
        Task<TokenReply> ReadOrRetrieveToken(ServiceSettings options, bool forceNewRetrieval = false);
    }
}