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
                var result = await _userService.RegisterAsync(request);
                return Ok(new { Message = "註冊成功", UserId = result.UserId, Status = result.Status });
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

        // POST: /User/Delete
        [HttpPost("Delete")]
        public async Task<IActionResult> Delete([FromBody] UserDeleteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _userService.DeleteUserAsync(request);
            if (success)
                return Ok(new { Message = message });
            else
                return BadRequest(new { Message = message });
        }
        // GET: /User/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserBasicInfo>>> GetAllUsers()
        {
           var users = await _userService.GetAllUsersAsync();
           return Ok(users);
        }
    }
}
