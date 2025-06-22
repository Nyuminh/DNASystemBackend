namespace DNASystemBackend.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly DnasystemContext _context;
    private readonly IConfiguration _config;

    public UserService(IUserRepository userRepo, DnasystemContext context, IConfiguration config)
    {
        _userRepo = userRepo;
        _context = context;
        _config = config;
    }

    public async Task<string?> AuthenticateAsync(LoginDto loginDto)
    {
        var user = await _userRepo.GetByUsernameAndPasswordAsync(loginDto.Username, loginDto.Password);
        if (user == null) return null;

        return GenerateJwtToken(user);
    }

    public async Task<(bool success, string? message, string? token)> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.UsernameExistsAsync(dto.Username))
            return (false, "Tên đăng nhập đã tồn tại.", null);

        if (!string.IsNullOrEmpty(dto.Email) && await _userRepo.EmailExistsAsync(dto.Email))
            return (false, "Email đã được sử dụng.", null);

        string newUserId = await GenerateUniqueUserIdAsync();

        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == "Customer")
                          ?? await _context.Roles.FirstOrDefaultAsync();

        var user = new User
        {
            UserId = newUserId,
            Username = dto.Username,
            Password = dto.Password, // TODO: Hash password in production
            Fullname = dto.Fullname,
            Email = dto.Email,
            Phone = dto.Phone,
            Gender = dto.Gender,
            Address = dto.Address,
            RoleId = defaultRole?.RoleId
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveAsync();

        var token = GenerateJwtToken(user);

        return (true, null, token);
    }

    public async Task<List<User>> GetAllUsersAsync() => await _userRepo.GetAllAsync();

    public async Task<List<User>> GetUsersByRoleAsync(string roleName)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Where(u => u.Role != null && u.Role.Rolename == roleName)
            .ToListAsync();
    }

    public async Task<User?> GetCurrentUserAsync(string userId)
    {
        return await _userRepo.GetByIdAsync(userId);
    }

    public async Task<(bool success, string? message)> CreateUserAsync(CreateUserDto dto)
    {
        if (await _userRepo.UsernameExistsAsync(dto.Username))
            return (false, "Tên đăng nhập đã tồn tại.");

        if (!string.IsNullOrEmpty(dto.Email) && await _userRepo.EmailExistsAsync(dto.Email))
            return (false, "Email đã được sử dụng.");

        string newUserId = await GenerateUniqueUserIdAsync();

        Role? role = null;
        if (!string.IsNullOrEmpty(dto.RoleId))
        {
            role = await _context.Roles.FindAsync(dto.RoleId);
            if (role == null)
                return (false, "Role không tồn tại.");
        }
        else if (!string.IsNullOrEmpty(dto.RoleName))
        {
            role = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == dto.RoleName);
            if (role == null)
                return (false, $"Role '{dto.RoleName}' không tồn tại.");
        }
        else
        {
            role = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == "Customer");
            if (role == null)
                return (false, "Không thể tìm thấy role mặc định.");
        }

        var user = new User
        {
            UserId = newUserId,
            Username = dto.Username,
            Password = dto.Password, // TODO: Hash password
            Fullname = dto.Fullname,
            Email = dto.Email,
            Phone = dto.Phone,
            Gender = dto.Gender,
            Address = dto.Address,
            RoleId = role.RoleId
        };

        try
        {
            await _userRepo.AddAsync(user);
            await _userRepo.SaveAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi tạo người dùng: {ex.Message}");
        }
    }

    public async Task<(bool success, string? message)> UpdateUserAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return (false, "Không tìm thấy người dùng.");

        if (string.IsNullOrEmpty(dto.Username))
            return (false, "Username không được để trống.");

        if (string.IsNullOrEmpty(dto.Password))
            return (false, "Password không được để trống.");

        if (string.IsNullOrEmpty(dto.RoleId))
            return (false, "Role không được để trống.");

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username && u.UserId != userId))
            return (false, "Tên đăng nhập đã tồn tại.");

        var role = await _context.Roles.FindAsync(dto.RoleId);
        if (role == null)
            return (false, "Role không tồn tại.");

        user.Username = dto.Username;
        user.Password = dto.Password; // TODO: Hash password
        user.RoleId = dto.RoleId;

        if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
        if (!string.IsNullOrEmpty(dto.Fullname)) user.Fullname = dto.Fullname;
        if (!string.IsNullOrEmpty(dto.Phone)) user.Phone = dto.Phone;

        try
        {
            await _userRepo.SaveAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi cập nhật người dùng: {ex.Message}");
        }
    }

    public async Task<(bool success, string? message)> DeleteUserAsync(string userId, string? currentUserId)
    {
        if (currentUserId == userId)
            return (false, "Không thể xóa chính tài khoản của bạn.");

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return (false, "Không tìm thấy người dùng.");

        try
        {
            _context.Users.Remove(user);
            await _userRepo.SaveAsync();
            return (true, "Xóa người dùng thành công.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi xóa người dùng: {ex.Message}");
        }
    }

    public async Task<object?> GetUserForEditAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.UserId == userId)
            .Select(u => new
            {
                u.UserId,
                u.Username,
                u.Password,
                u.RoleId,
                RoleName = u.Role.Rolename
            })
            .FirstOrDefaultAsync();

        if (user == null) return null;

        var roles = await _context.Roles
            .Select(r => new { r.RoleId, r.Rolename })
            .ToListAsync();

        return new
        {
            User = user,
            AvailableRoles = roles
        };
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

    private async Task<string> GenerateUniqueUserIdAsync()
    {
        var existingIds = await _context.Users
            .Select(u => u.UserId)
            .Where(id => id.StartsWith("U") && id.Length == 4)
            .ToListAsync();

        int counter = 1;
        string newId;
        do
        {
            newId = $"U{counter:D03}";
            counter++;
        } while (existingIds.Contains(newId) && counter < 1000);

        if (counter >= 1000)
        {
            newId = $"U{DateTime.Now.Ticks % 1000000:D06}";
        }

        return newId;
    }
}
