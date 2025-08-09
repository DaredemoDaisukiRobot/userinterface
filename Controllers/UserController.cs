using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using userinterface.Models;
using userinterface.Services;
using System;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace userinterface.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public UserController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        // POST: /User/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.email))
                return BadRequest("Email為必填欄位");

            try
            {
                var result = await _userService.RegisterAsync(request);
                return Ok(new { Message = "註冊成功", UserId = result.UserId, Status = result.Status });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Error = ex.Message, Detail = ex.InnerException?.Message });
            }
        }

        // POST: /User/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.email))
                return BadRequest("Email為必填欄位");

            var (success, username, status) = await _userService.LoginAsync(request);
            if (success)
            {
                var token = GenerateJwtToken(username!, request.email, status);
                return Ok(new { Message = "登入成功", Username = username, Status = status, Token = token });
            }
            else
                return Unauthorized(new { Message = "登入失敗，帳號或密碼錯誤" });
        }

        private string GenerateJwtToken(string username, string email, string? status)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("status", status ?? "user")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // DELETE: /User/Delete
        [HttpDelete("Delete")]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] UserDeleteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var status = User.FindFirst("status")?.Value;
            if (!string.Equals(status, "admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var (success, message) = await _userService.DeleteUserAsync(request);
            if (success)
                return Ok(new { Message = message });
            else
                return BadRequest(new { Message = message });
        }
        // GET: /User/all
        [HttpGet("all")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserBasicInfo>>> GetAllUsers()
        {
            var status = User.FindFirst("status")?.Value;
            if (!string.Equals(status, "admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // PATCH: /User/Update
        [HttpPatch("Update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UserUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var status = User.FindFirst("status")?.Value;
            if (!string.Equals(status, "admin", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var (success, message) = await _userService.UpdateUserAsync(request);
            if (success)
                return Ok(new { Message = message });
            else
                return BadRequest(new { Message = message });
        }

        // PATCH: /User/Uppwd
        [HttpPatch("Uppwd")]
        public async Task<IActionResult> UpdatePassword([FromBody] UserPasswordUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _userService.UpdatePasswordAsync(request);
            if (success)
                return Ok(new { Message = message });
            else
                return BadRequest(new { Message = message });
        }
    }
}
