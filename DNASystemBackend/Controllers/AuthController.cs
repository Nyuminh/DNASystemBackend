using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var token = await _userService.AuthenticateAsync(model);
            if (token == null)
                return Unauthorized("Sai tài khoản hoặc mật khẩu.");

            return Ok(new { token });
        }

        /// <summary>
        /// Đăng ký
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var (success, message, token) = await _userService.RegisterAsync(model);
            if (!success)
                return BadRequest(message);

            return Ok(new
            {
                message = "Đăng ký thành công!",
                token
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); 
            return Ok(new { message = "Đăng xuất thành công." });
        }
    }
}
