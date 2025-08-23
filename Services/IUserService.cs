using System.Threading.Tasks;
using userinterface.Models;

namespace userinterface.Services
{
    public interface IUserService
    {
        Task<UserRegistrationResult> RegisterAsync(UserRegistrationRequest request);
        Task<(bool Success, int? UserId, string? Username, IEnumerable<string> Roles, IEnumerable<string> Permissions)> LoginAsync(UserLoginRequest request);
        Task<(bool Success, string? Message)> DeleteUserAsync(UserDeleteRequest request);
        Task<IEnumerable<UserBasicInfo>> GetAllUsersAsync();
        Task<(bool Success, string? Message)> UpdateUserAsync(UserUpdateRequest request);
        Task<(bool Success, string? Message)> AssignRoleAsync(AssignRoleRequest request);
    }
}
