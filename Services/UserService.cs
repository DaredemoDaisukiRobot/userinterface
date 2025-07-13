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

        public async Task<int> RegisterAsync(UserRegistrationRequest request)
        {
            using var connection = new MySqlConnection(_connectionString);
            // 產生隨機 salt
            var salt = Guid.NewGuid().ToString("N");
            var hash = HashPassword(request.Password ?? "", salt);

            int userId;
            if (request.ID.HasValue)
            {
                // 優先使用傳入的 ID
                string sql = "INSERT INTO users (id, name, password_hash, msg) VALUES (@Id, @Username, @Password, @Msg);";
                await connection.ExecuteAsync(
                    sql,
                    new { Id = request.ID.Value, Username = request.username, Password = hash, Msg = salt }
                );
                userId = request.ID.Value;
            }
            else
            {
                // 沒有傳入 ID 則自動產生
                string sql = "INSERT INTO users (name, password_hash, msg) VALUES (@Username, @Password, @Msg); SELECT LAST_INSERT_ID();";
                userId = await connection.ExecuteScalarAsync<int>(
                    sql,
                    new { Username = request.username, Password = hash, Msg = salt }
                );
            }
            return userId;
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
    }
}
