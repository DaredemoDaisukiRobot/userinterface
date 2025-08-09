using System.Threading.Tasks;
using userinterface.Models;

namespace userinterface.Services
{
    public interface IUserService
    {
        Task<UserRegistrationResult> RegisterAsync(UserRegistrationRequest request);
        Task<(bool Success, string? Username, string? Status)> LoginAsync(UserLoginRequest request);
        Task<(bool Success, string? Message)> DeleteUserAsync(UserDeleteRequest request);
        Task<(bool Success, string? Message)> UpdateUserAsync(UserUpdateRequest request);
        Task<(bool Success, string? Message)> UpdatePasswordAsync(UserPasswordUpdateRequest request);
        Task<IEnumerable<UserBasicInfo>> GetAllUsersAsync();
    }
}
