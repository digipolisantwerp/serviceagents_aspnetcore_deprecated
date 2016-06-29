using System.Threading.Tasks;

namespace SampleApi.ServiceAgents
{
    public interface IOAuthDemoAgent
    {
        Task<string> GetAsStringAsync();
    }
}