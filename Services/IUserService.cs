using System.Threading.Tasks;
using userinterface.Models;

namespace userinterface.Services
{
    public interface IUserService
    {
        Task<int> RegisterAsync(UserRegistrationRequest request);
        Task<(bool Success, string? Username)> LoginAsync(UserLoginRequest request);
    }
}
