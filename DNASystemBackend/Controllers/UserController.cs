using System.Security.Claims;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DnasystemContext _context;

        public UserController(DnasystemContext context)
        {
            _context = context;
        }

        // Chỉ Admin mới được gọi API này
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // Chỉ Manager hoặc Admin
        [HttpGet("managers")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetManagers()
        {
            return await _context.Users.Where(u => u.Role.Rolename == "Manager").ToListAsync();
        }

        // Ai đăng nhập cũng dùng được
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<User>> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();
            return user;
        }
    }
}