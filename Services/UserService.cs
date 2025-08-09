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

        public UserService(UserDbContext db)
        {
            _db = db;
        }

        private string HashPassword(string password, string salt)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<UserRegistrationResult> RegisterAsync(UserRegistrationRequest request)
        {
            // 新增：檢查 email 是否已存在
            if (await _db.Users.AnyAsync(u => u.email == request.email))
                throw new Exception("Email 已被註冊");

            // 產生隨機 salt 並雜湊密碼
            var salt = Guid.NewGuid().ToString("N");
            var hash = HashPassword(request.Password ?? "", salt);

            var user = new User
            {
                Id = request.ID ?? 0,
                Name = request.username,
                email = request.email,
                PasswordHash = hash,
                Msg = salt,
                Status = request.status ?? "user" 
            };

            if (request.ID.HasValue)
            {
                user.Id = request.ID.Value;
                _db.Users.Add(user);
            }
            else
            {
                _db.Users.Add(user);
            }

            await _db.SaveChangesAsync();

            return new UserRegistrationResult
            {
                UserId = user.Id,
                Status = user.Status
            };
        }

        // 變更為回傳 Status
        public async Task<(bool Success, string? Username, string? Status)> LoginAsync(UserLoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.email == request.email);
            if (user == null) return (false, null, null);
            var hash = HashPassword(request.Password ?? "", user.Msg);
            if (hash == user.PasswordHash)
                return (true, user.Name, user.Status);
            else
                return (false, null, null);
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
                .Select(u => new UserBasicInfo
                {
                    Id = u.Id,
                    Name = u.Name,
                    Status = u.Status,
                    email = u.email
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Message)> UpdateUserAsync(UserUpdateRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
            if (user == null)
                return (false, "找不到使用者");

            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;
            if (!string.IsNullOrWhiteSpace(request.Status))
                user.Status = request.Status;

            await _db.SaveChangesAsync();
            return (true, "更新成功");
        }

        public async Task<(bool Success, string? Message)> UpdatePasswordAsync(UserPasswordUpdateRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.Id);
            if (user == null)
                return (false, "找不到使用者");

            var oldHash = HashPassword(request.OldPassword ?? "", user.Msg);
            if (oldHash != user.PasswordHash)
                return (false, "密碼錯誤");

            var newHash = HashPassword(request.NewPassword ?? "", user.Msg);
            if (newHash == user.PasswordHash)
                return (false, "新密碼不能與舊密碼相同( ◜ω◝ )");

            user.PasswordHash = newHash;
            await _db.SaveChangesAsync();
            return (true, "密碼更新成功");
        }
    }
}