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
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserModel model)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                return BadRequest("Tên đăng nhập đã tồn tại.");

            // Check if email already exists
            if (!string.IsNullOrEmpty(model.Email) && await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest("Email đã được sử dụng.");

            // Generate a unique user ID that doesn't exist yet
            string newUserId = await GenerateUniqueUserIdAsync();

            // Find the requested role or default to Customer
            Role role;
            if (!string.IsNullOrEmpty(model.RoleId))
            {
                role = await _context.Roles.FindAsync(model.RoleId);
                if (role == null)
                    return BadRequest("Role không tồn tại.");
            }
            else if (!string.IsNullOrEmpty(model.RoleName))
            {
                role = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == model.RoleName);
                if (role == null)
                    return BadRequest($"Role '{model.RoleName}' không tồn tại.");
            }
            else
            {
                // Default to Customer role
                role = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == "Customer");
                if (role == null)
                    return StatusCode(500, "Không thể tìm thấy role mặc định.");
            }

            // Create new user
            var user = new User
            {
                UserId = newUserId,
                Username = model.Username,
                Password = model.Password,
                Fullname = model.Fullname,
                Email = model.Email,
                Phone = model.Phone,
                Gender = model.Gender,
                Address = model.Address,
                RoleId = role.RoleId
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetUsers), new { id = user.UserId }, user);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception
                return StatusCode(500, "Có lỗi khi tạo người dùng: " + ex.InnerException?.Message ?? ex.Message);
            }
        }


        // Helper method to generate a unique user ID
        private async Task<string> GenerateUniqueUserIdAsync()
        {
            // Get all existing user IDs
            var existingIds = await _context.Users
                .Select(u => u.UserId)
                .Where(id => id.StartsWith("U") && id.Length == 4)
                .ToListAsync();

            // Start from U001
            int counter = 1;
            string newId;

            // Keep incrementing until we find an ID that doesn't exist
            do
            {
                newId = $"U{counter:D03}";
                counter++;
            }
            while (existingIds.Contains(newId) && counter < 1000);

            // If we've tried 999 IDs and still can't find a unique one, use timestamp
            if (counter >= 1000)
            {
                // Use timestamp-based ID as a fallback
                newId = $"U{DateTime.Now.Ticks % 1000000:D06}";
            }

            return newId;
        }
        [HttpGet("{id}/edit")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetUserForEdit(string id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.UserId == id)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Password, // Trong thực tế không nên trả password
                    u.RoleId,
                    RoleName = u.Role.Rolename,
                    // Thêm các field khác nếu User model có:
                    // u.Email,
                    // u.Fullname,
                    // u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("Không tìm thấy người dùng.");

            // Lấy danh sách roles để hiển thị trong dropdown
            var roles = await _context.Roles
                .Select(r => new { r.RoleId, r.Rolename })
                .ToListAsync();

            return Ok(new
            {
                User = user,
                AvailableRoles = roles
            });
        }

        // PUT /users/{id} - Cập nhật thông tin người dùng (Admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserModel model)
        {
            try
            {
                // Tìm người dùng cần cập nhật
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound("Không tìm thấy người dùng.");

                // Validation - kiểm tra tất cả các field bắt buộc
                if (string.IsNullOrEmpty(model.Username))
                    return BadRequest("Username không được để trống.");

                if (string.IsNullOrEmpty(model.Password))
                    return BadRequest("Password không được để trống.");

                if (string.IsNullOrEmpty(model.RoleId))
                    return BadRequest("Role không được để trống.");

                // Kiểm tra username trùng lặp (trừ chính user này)
                if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserId != id))
                    return BadRequest("Tên đăng nhập đã tồn tại.");

                // Kiểm tra role tồn tại
                var role = await _context.Roles.FindAsync(model.RoleId);
                if (role == null)
                    return BadRequest("Role không tồn tại.");

                // Cập nhật thông tin
                user.Username = model.Username;
                user.Password = model.Password; // Trong production, hash password
                user.RoleId = model.RoleId;

                // Cập nhật các field khác nếu có:
                // if (!string.IsNullOrEmpty(model.Email))
                // {
                //     if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != id))
                //         return BadRequest("Email đã được sử dụng.");
                //     user.Email = model.Email;
                // }

                // if (!string.IsNullOrEmpty(model.Fullname))
                //     user.Fullname = model.Fullname;

                await _context.SaveChangesAsync();

                // Trả về thông tin đã cập nhật
                var updatedUser = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.UserId == id)
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        Role = u.Role.Rolename,
                        Message = "Cập nhật thông tin thành công.",
                        UpdatedAt = DateTime.UtcNow
                    })
                    .FirstOrDefaultAsync();

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi cập nhật thông tin người dùng.",
                    error = ex.Message
                });
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                // Lấy thông tin admin hiện tại
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Không cho phép admin tự xóa chính mình
                if (currentUserId == id)
                    return BadRequest("Không thể xóa chính tài khoản của bạn.");

                // Tìm người dùng cần xóa
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                    return NotFound("Không tìm thấy người dùng.");

                // Xóa user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xóa người dùng thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Có lỗi xảy ra khi xóa người dùng.",
                    error = ex.Message
                });
            }
        }
    }
    public class UpdateProfileModel
    {

        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? RoleId { get; set; }

        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public string? Phone { get; set; }
    }
    public class UpdateUserModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? RoleId { get; set; }

        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public string? Phone { get; set; }
        //public DateTime? DateOfBirth { get; set; }
    }


    public class CreateUserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }


}
