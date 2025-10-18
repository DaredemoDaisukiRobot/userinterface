using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using userinterface.Services;
using userinterface.Models;

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
            if (request == null || string.IsNullOrWhiteSpace(request.email))
                return BadRequest("email為必填欄位");

            try
            {
                var result = await _userService.RegisterAsync(request);
                return Ok(new { Message = "註冊成功", result.UserId, Roles = result.Roles });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "註冊時發生錯誤" });
            }
        }

        // POST: /User/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.email))
                return BadRequest("email為必填欄位");

            var (success, userId, username, roles, perms) = await _userService.LoginAsync(request);
            if (!success)
                return Unauthorized(new { Message = "登入失敗" });

            var token = GenerateJwtToken(userId!.Value, username!, request.email, roles, perms);
            return Ok(new
            {
                Message = "登入成功",
                UserId = userId,
                Username = username,
                Roles = roles,
                Permissions = perms,
                Token = token
            });
        }

        // GET: /User/all 需 view 權限
        [HttpGet("all")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserBasicInfo>>> GetAllUsers()
        {
            if (!HasPermission("view")) return Forbid();
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // PATCH: /User/Update 需 edit_user
        [HttpPatch("Update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UserUpdateRequest request)
        {
            if (!HasPermission("edit_user")) return Forbid();
            var (success, message) = await _userService.UpdateUserAsync(request);
            return success ? Ok(new { Message = message }) : BadRequest(new { Message = message });
        }

        // DELETE: /User/Delete 需 delete_user
        [HttpDelete("Delete")]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] UserDeleteRequest request)
        {
            if (!HasPermission("delete_user")) return Forbid();
            var (success, message) = await _userService.DeleteUserAsync(request);
            return success ? Ok(new { Message = message }) : BadRequest(new { Message = message });
        }

        // POST: /User/AssignRole 需 permission_editing
        [HttpPost("AssignRole")]
        [Authorize]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            if (!HasPermission("permission_editing")) return Forbid();
            var (success, message) = await _userService.AssignRoleAsync(request);
            return success ? Ok(new { Message = message }) : BadRequest(new { Message = message });
        }

        private bool HasPermission(string perm) =>
            User.Claims.Any(c => c.Type == "perm" && c.Value == perm);

        private int? GetUserId()
        {
            var uid = User.FindFirst("uid")?.Value;
            return int.TryParse(uid, out var id) ? id : null;
        }

        private string GenerateJwtToken(int userId, string username, string email,
            IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            var claims = new List<Claim>
            {
                new Claim("uid", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.UniqueName, username)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            claims.AddRange(permissions.Select(p => new Claim("perm", p)));

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
    }
} 
