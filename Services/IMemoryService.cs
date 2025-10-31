using System.Threading.Tasks;
using userinterface.Models;

namespace userinterface.Services
{
    public interface IMemoryService
    {
        Task<MemorySearchResponse> SearchAsync(int start, int quantity);
        Task<MemorySearchResponse> FilterAsync(int start, int quantity, string criteria);
    }
}
