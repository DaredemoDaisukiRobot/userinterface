using System.Threading.Tasks;
using userinterface.Models;
using System.Collections.Generic;
using System.Data;
using Dapper;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;

namespace userinterface.Services
{
    public class UserService : IUserService
    {
        private readonly string _connectionString = "Server=26.9.28.191;Port=13306;Database=userdatabase;Uid=ccc;Pwd=bigred;";

        private string HashPassword(string password, string salt)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + salt);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<UserRegistrationResult> RegisterAsync(UserRegistrationRequest request)
        {
            using var connection = new MySqlConnection(_connectionString);

            // 產生隨機 salt 並雜湊密碼
            var salt = Guid.NewGuid().ToString("N");
            var hash = HashPassword(request.Password ?? "", salt);

            // 準備參數
            var parameters = new DynamicParameters();
            parameters.Add("Username", request.username);
            parameters.Add("Password", hash);
            parameters.Add("Msg", salt);

            bool hasStatus = !string.IsNullOrEmpty(request.status);
            if (hasStatus)
                parameters.Add("Status", request.status);

            string sql;
            int userId;

            if (request.ID.HasValue)
            {
                parameters.Add("Id", request.ID.Value);

                sql = hasStatus
                    ? "INSERT INTO users (id, name, password_hash, msg, status) VALUES (@Id, @Username, @Password, @Msg, @Status);"
                    : "INSERT INTO users (id, name, password_hash, msg) VALUES (@Id, @Username, @Password, @Msg);";

                await connection.ExecuteAsync(sql, parameters);
                userId = request.ID.Value;
            }
            else
            {
                sql = hasStatus
                    ? "INSERT INTO users (name, password_hash, msg, status) VALUES (@Username, @Password, @Msg, @Status); SELECT LAST_INSERT_ID();"
                    : "INSERT INTO users (name, password_hash, msg) VALUES (@Username, @Password, @Msg); SELECT LAST_INSERT_ID();";

                userId = await connection.ExecuteScalarAsync<int>(sql, parameters);
            }

            string statusSql = "SELECT status FROM users WHERE id = @Id";
            var status = await connection.ExecuteScalarAsync<string>(statusSql, new { Id = userId });

            return new UserRegistrationResult
            {
                UserId = userId,
                Status = status
            };
        }

        public async Task<(bool Success, string? Username)> LoginAsync(UserLoginRequest request)
        {
            using var connection = new MySqlConnection(_connectionString);
            // 取得 hash、salt、username
            string sql = "SELECT name, password_hash, msg FROM users WHERE id = @Id";
            var result = await connection.QueryFirstOrDefaultAsync<(string name, string password_hash, string msg)>(sql, new { Id = request.ID });
            if (result.password_hash == null || result.msg == null) return (false, null);
            var hash = HashPassword(request.Password ?? "", result.msg);
            if (hash == result.password_hash)
                return (true, result.name);
            else
                return (false, null);
        }

        public async Task<(bool Success, string? Message)> DeleteUserAsync(UserDeleteRequest request)
        {
            using var connection = new MySqlConnection(_connectionString);

            // 驗證管理員帳號密碼與權限
            string adminSql = "SELECT password_hash, msg, status FROM users WHERE id = @AdminId";
            var admin = await connection.QueryFirstOrDefaultAsync<(string password_hash, string msg, string status)>(
                adminSql, new { AdminId = request.admin_ID });

            if (admin.password_hash == null || admin.msg == null)
                return (false, "帳號不存在");

            if (admin.status != "admin")
                return (false, "權限不足");

            var adminHash = HashPassword(request.admin_pwd ?? "", admin.msg);
            if (adminHash != admin.password_hash)
                return (false, "帳號或密碼錯誤");

            // 刪除使用者
            string deleteSql = "DELETE FROM users WHERE id = @UserId";
            int affected = await connection.ExecuteAsync(deleteSql, new { UserId = request.user_ID });

            if (affected > 0)
                return (true, "刪除成功");
            else
                return (false, "找不到使用者");
        }
        public async Task<IEnumerable<UserBasicInfo>> GetAllUsersAsync()
        {
            using var connection = new MySqlConnection(_connectionString);

            var sql = "SELECT id, name FROM users";
            var users = await connection.QueryAsync<UserBasicInfo>(sql); // 只對應 id 和 name 就可以

            return users;
        }
    }
}