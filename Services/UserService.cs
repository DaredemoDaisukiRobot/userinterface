using System.Threading.Tasks;
using userinterface.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace userinterface.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _db;
        public UserService(UserDbContext db) { _db = db; }

        private string HashPassword(string password, string salt)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
        }

        public async Task<UserRegistrationResult> RegisterAsync(UserRegistrationRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.email == request.email))
                throw new Exception("Email 已被註冊");

            var salt = Guid.NewGuid().ToString("N");
            var hash = HashPassword(request.Password ?? "", salt);

            var user = new User
            {
                Name = request.username,
                email = request.email,
                PasswordHash = hash,
                Msg = salt
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // 指派預設 user 角色（依名稱或 id=2）
            var userRoleEntity = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "user") 
                                 ?? await _db.Roles.FirstOrDefaultAsync(r => r.Id == 2);
            if (userRoleEntity != null)
            {
                _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRoleEntity.Id });
                await _db.SaveChangesAsync();
            }

            var roles = await _db.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            return new UserRegistrationResult { UserId = user.Id, Roles = roles };
        }

        // 變更為回傳 Status
        public async Task<(bool Success, int? UserId, string? Username, IEnumerable<string> Roles, IEnumerable<string> Permissions)> LoginAsync(UserLoginRequest request)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.email == request.email);

            if (user == null) return (false, null, null, Array.Empty<string>(), Array.Empty<string>());

            var hash = HashPassword(request.Password ?? "", user.Msg);
            if (hash != user.PasswordHash)
                return (false, null, null, Array.Empty<string>(), Array.Empty<string>());

            var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
            var perms = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList();

            return (true, user.Id, user.Name, roles, perms);
        }

        public async Task<(bool Success, string? Message)> DeleteUserAsync(UserDeleteRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.user_ID);
            if (user == null)
                return (false, "找不到使用者");
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return (true, "刪除成功");
        }

        public async Task<IEnumerable<UserBasicInfo>> GetAllUsersAsync()
        {
            return await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Select(u => new UserBasicInfo
                {
                    Id = u.Id,
                    Name = u.Name,
                    email = u.email,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Message)> UpdateUserAsync(UserUpdateRequest request)
        {
            var user = await _db.Users.FindAsync(request.Id);
            if (user == null)
                return (false, "找不到使用者");
            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;
            await _db.SaveChangesAsync();
            return (true, "更新成功");
        }

        public async Task<(bool Success, string? Message)> AssignRoleAsync(AssignRoleRequest request)
        {
            if (!await _db.Users.AnyAsync(u => u.Id == request.UserId))
                return (false, "使用者不存在");
            if (!await _db.Roles.AnyAsync(r => r.Id == request.RoleId))
                return (false, "角色不存在");
            if (await _db.UserRoles.AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId))
                return (false, "已擁有該角色");
            _db.UserRoles.Add(new UserRole { UserId = request.UserId, RoleId = request.RoleId });
            await _db.SaveChangesAsync();
            return (true, "角色指派成功");
        }
    }
}