using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using userinterface.Models;
using userinterface.Services;

namespace userinterface.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: /User/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                int userId = await _userService.RegisterAsync(request);
                return Ok(new { Message = "註冊成功", UserId = userId });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // POST: /User/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, username) = await _userService.LoginAsync(request);
            if (success)
                return Ok(new { Message = "登入成功", Username = username });
            else
                return Unauthorized(new { Message = "登入失敗，帳號或密碼錯誤" });
        }
    }
}
