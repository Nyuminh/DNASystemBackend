using System.Security.Claims;
using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /api/user
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: /api/user/managers
        [HttpGet("managers")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetManagers()
        {
            var users = await _userService.GetUsersByRoleAsync("Manager");
            return Ok(users);
        }

        // GET: /api/user/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Không xác định được người dùng.");

            var user = await _userService.GetCurrentUserAsync(userId);
            if (user == null) return NotFound();

            return Ok(user);
        }

        // POST: /api/user
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var (success, message) = await _userService.CreateUserAsync(dto);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Tạo người dùng thành công." });
        }

        // GET: /api/user/{id}/edit
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> GetUserForEdit(string id)
        {
            var data = await _userService.GetUserForEditAsync(id);
            if (data == null) return NotFound("Không tìm thấy người dùng.");
            return Ok(data);
        }

        // PUT: /api/user/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var (success, message) = await _userService.UpdateUserAsync(id, dto);
            if (!success) return BadRequest(message);
            return Ok(new { message = "Cập nhật người dùng thành công." });
        }

        // DELETE: /api/user/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var (success, message) = await _userService.DeleteUserAsync(id, currentUserId);
            if (!success) return BadRequest(message);
            return Ok(new { message });
        }
    }
}
