using System.Threading;
using System.Threading.Tasks;
using SampleApi.Models;

namespace SampleApi.ServiceAgents
{
    public interface IDemoAgent
    {
        Task<Address> GetAddressAsync(int id);
        Task<Address> PostAddressAsync(Address adress);
    }
}