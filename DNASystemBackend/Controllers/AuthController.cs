using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace DNASystemBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DnasystemContext _context; 
        private readonly IConfiguration _config;

        public AuthController(DnasystemContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
                return Unauthorized("Sai tài khoản hoặc mật khẩu.");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return BadRequest("Tên đăng nhập đã tồn tại.");

            // Check if email already exists
            if (!string.IsNullOrEmpty(model.Email) && await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest("Email đã được sử dụng.");

            // Generate a unique user ID (e.g., "U001", "U002", etc.)
            var lastUserId = await _context.Users
                .OrderByDescending(u => u.UserId)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            string newUserId = "U001";

            if (lastUserId != null && lastUserId.StartsWith("U"))
            {
                if (int.TryParse(lastUserId.Substring(1), out int lastId))
                {
                    newUserId = $"U{(lastId + 1):D03}";
                }
            }

            // Default role for new users (e.g., "Customer")
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == "Customer")
                ?? await _context.Roles.FirstOrDefaultAsync(); // Fallback to first role if "Customer" not found

            // Create new user
            var user = new User
            {
                UserId = newUserId,
                Username = model.Username,
                Password = model.Password, // Note: In a real application, this should be hashed
                Fullname = model.Fullname,
                Email = model.Email,
                Phone = model.Phone,
                Gender = model.Gender,
                Address = model.Address,
                RoleId = defaultRole?.RoleId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate token for newly registered user
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Đăng ký thành công!",
                userId = user.UserId,
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
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = jwtSettings["Key"];
            
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("JWT Key is not configured properly in appsettings.json");
                
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role?.Rolename ?? "User")
            };

            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "DNASystemApi",
                audience: jwtSettings["Audience"] ?? "DNASystemApiUser",
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
    }
}