namespace DNASystemBackend.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DNASystemBackend.DTOs;
using DNASystemBackend.Interfaces;
using DNASystemBackend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly DnasystemContext _context;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    
    // Store reset tokens with expiration times and associated usernames
    private static readonly Dictionary<string, DateTime> _resetTokens = new Dictionary<string, DateTime>();
    private static readonly Dictionary<string, string> _tokenToUsername = new Dictionary<string, string>();

    public UserService(IUserRepository userRepo, DnasystemContext context, IConfiguration config, IEmailService emailService)
    {
        _userRepo = userRepo;
        _context = context;
        _config = config;
        _emailService = emailService;
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
            RoleId = defaultRole?.RoleId,
            Birthdate = dto.Birthdate,
            Image = dto.Image
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

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<(bool success, string? message)> CreateAsync(User user)
    {
        try
        {
            // Check if user with email already exists
            if (!string.IsNullOrEmpty(user.Email))
            {
                var existingUser = await GetUserByEmailAsync(user.Email);
                if (existingUser != null)
                    return (false, "User with this email already exists.");
            }

            // Check if username already exists
            if (!string.IsNullOrEmpty(user.Username))
            {
                if (await _userRepo.UsernameExistsAsync(user.Username))
                    return (false, "Username already exists.");
            }

            // Generate unique user ID if not provided
            if (string.IsNullOrEmpty(user.UserId))
            {
                user.UserId = await GenerateUniqueUserIdAsync();
            }

            // Set default role if not provided
            if (string.IsNullOrEmpty(user.RoleId))
            {
                Console.WriteLine("Looking for default role 'Customer'");
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == "Customer");
                
                if (defaultRole != null)
                {
                    Console.WriteLine($"Found default role: {defaultRole.RoleId} - {defaultRole.Rolename}");
                    user.RoleId = defaultRole.RoleId;
                }
                else
                {
                    Console.WriteLine("No 'Customer' role found, checking all available roles...");
                    var allRoles = await _context.Roles.ToListAsync();
                    foreach (var role in allRoles)
                    {
                        Console.WriteLine($"Available role: {role.RoleId} - {role.Rolename}");
                    }
                    
                    // Try to find any role that might be a customer/user role
                    var fallbackRole = allRoles.FirstOrDefault(r => 
                        r.Rolename?.ToLower().Contains("customer") == true ||
                        r.Rolename?.ToLower().Contains("user") == true ||
                        r.Rolename?.ToLower().Contains("member") == true);
                        
                    if (fallbackRole != null)
                    {
                        Console.WriteLine($"Using fallback role: {fallbackRole.RoleId} - {fallbackRole.Rolename}");
                        user.RoleId = fallbackRole.RoleId;
                    }
                    else if (allRoles.Any())
                    {
                        // Use the first available role as a last resort
                        var firstRole = allRoles.First();
                        Console.WriteLine($"Using first available role: {firstRole.RoleId} - {firstRole.Rolename}");
                        user.RoleId = firstRole.RoleId;
                    }
                    else
                    {
                        return (false, "No roles found in the database. Please ensure the Role table is populated.");
                    }
                }
            }

            await _userRepo.AddAsync(user);
            await _userRepo.SaveAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error creating user: {ex.Message}");
        }
    }

    public async Task<(bool success, string? message)> UpdateUserAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return (false, "Không tìm thấy người dùng.");

        if (string.IsNullOrEmpty(dto.Username))
            return (false, "Username không được để trống.");
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username && u.UserId != userId))
            return (false, "Tên đăng nhập đã tồn tại.");

        user.Username = dto.Username;
        user.RoleId = dto.RoleId;
        user.Birthdate = dto.Birthdate;
        user.Address = dto.Address;

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
    public async Task<IActionResult> UpdateUserImageAsync(string id, [FromForm] UpdateUserImageDto dto)
    {
        var UpdatedUser = await _userRepo.GetByIdAsync(id);
        if (UpdatedUser == null)
            return new NotFoundObjectResult(new { message = "Không tìm thấy người dùng." });

        if (dto.picture!= null && dto.picture.Length > 0) {
            var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "images", dto.picture.FileName);
            using (var stream = System.IO.File.Create(path))
            {
                await dto.picture.CopyToAsync(stream);
            }
            UpdatedUser.Image = "/images/" + dto.picture.FileName; // Assuming you want to store the filename in the database
        }
        else
        {
            UpdatedUser.Image = null; // Clear image if no file is provided
        }


            await _userRepo.UpdateAsync(UpdatedUser);
             await _userRepo.SaveAsync();
           return new OkObjectResult(new { message = "Cập nhật hình ảnh người dùng thành công.", user = UpdatedUser });

    }

    public async Task<(bool success, string? message)> DeleteUserAsync(string userId, string? currentUserId)
    {
        if (currentUserId == userId)
            return (false, "Không thể xóa chính tài khoản của bạn.");

        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return (false, "Không tìm thấy người dùng.");

        try
        {
            // Begin transaction to ensure data integrity
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                try
                {
                    // Delete related Notifications - wrapped in try-catch because table might not exist
                    var notifications = await _context.Notifications
                        .Where(n => n.UserId == userId)
                        .ToListAsync();
                    if (notifications.Any())
                    {
                        _context.Notifications.RemoveRange(notifications);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not delete notifications: {ex.Message}");
                    // Continue with other deletions even if notifications fail
                }
                
                // Delete related Relatives
                var relatives = await _context.Relatives
                    .Where(r => r.UserId == userId)
                    .ToListAsync();
                _context.Relatives.RemoveRange(relatives);
                
                // Update or delete related Bookings where user is customer
                var customerBookings = await _context.Bookings
                    .Where(b => b.CustomerId == userId)
                    .ToListAsync();
                foreach (var booking in customerBookings)
                {
                    // Delete related TestResults for this booking
                    var testResults = await _context.TestResults
                        .Where(tr => tr.BookingId == booking.BookingId)
                        .ToListAsync();
                    _context.TestResults.RemoveRange(testResults);
                    
                    // Delete related Kits for this booking
                    var kits = await _context.Kits
                        .Where(k => k.BookingId == booking.BookingId)
                        .ToListAsync();
                    _context.Kits.RemoveRange(kits);
                    
                    // Delete related Invoices for this booking
                    var invoices = await _context.Invoices
                        .Where(i => i.BookingId == booking.BookingId)
                        .ToListAsync();
                    foreach (var invoice in invoices)
                    {
                        // Delete related InvoiceDetails
                        var invoiceDetails = await _context.InvoiceDetails
                            .Where(id => id.InvoiceId == invoice.InvoiceId)
                            .ToListAsync();
                        _context.InvoiceDetails.RemoveRange(invoiceDetails);
                    }
                    _context.Invoices.RemoveRange(invoices);
                }
                _context.Bookings.RemoveRange(customerBookings);
                
                // Update related Bookings where user is staff
                var staffBookings = await _context.Bookings
                    .Where(b => b.StaffId == userId)
                    .ToListAsync();
                foreach (var booking in staffBookings)
                {
                    booking.StaffId = null; // Set to null instead of deleting
                }
                
                // Delete related TestResults where user is customer or staff
                var userTestResults = await _context.TestResults
                    .Where(tr => tr.CustomerId == userId || tr.StaffId == userId)
                    .ToListAsync();
                _context.TestResults.RemoveRange(userTestResults);
                
                // Delete related Kits where user is customer or staff
                var userKits = await _context.Kits
                    .Where(k => k.CustomerId == userId || k.StaffId == userId)
                    .ToListAsync();
                _context.Kits.RemoveRange(userKits);
                
                // Delete related Feedbacks
                var feedbacks = await _context.Feedbacks
                    .Where(f => f.CustomerId == userId)
                    .ToListAsync();
                _context.Feedbacks.RemoveRange(feedbacks);
                
                // Update related Courses
                var courses = await _context.Courses
                    .Where(c => c.ManagerId == userId)
                    .ToListAsync();
                foreach (var course in courses)
                {
                    course.ManagerId = null; // Set to null instead of deleting
                }
                
                // Finally delete the user
                _context.Users.Remove(user);
                
                // Save changes and commit transaction
                await _userRepo.SaveAsync();
                await transaction.CommitAsync();
                
                return (true, "Xóa người dùng thành công.");
            }
            catch (Exception ex)
            {
                // If any error occurs, roll back the transaction
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi xóa dữ liệu liên quan: {ex.Message}", ex);
            }
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
                RoleName = u.Role != null ? u.Role.Rolename : "Unknown"
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
            RoleId = role.RoleId,
            Birthdate = dto.Birthdate,
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

    public async Task<(bool success, string? message)> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
                return (false, "Tên đăng nhập không tồn tại trong hệ thống.");

            if (string.IsNullOrEmpty(user.Email))
                return (false, "Tài khoản này chưa có email. Vui lòng liên hệ quản trị viên.");

            var resetToken = GenerateResetToken();

            // Store token with expiration time (30 minutes from now) and associated username
            var expirationTime = DateTime.Now.AddMinutes(30);
            _resetTokens[resetToken] = expirationTime;
            _tokenToUsername[resetToken] = dto.Username;

            // Clean up expired tokens
            CleanupExpiredTokens();

            // Send email with reset code
            bool emailSent = await _emailService.SendResetPasswordEmailAsync(user.Email, resetToken, user.Username);

            if (emailSent)
            {
                return (true, $"Mã xác thực đã được gửi đến email: {MaskEmail(user.Email)}\nMã sẽ hết hạn sau 30 phút.");
            }
            else
            {
                // If email fails, still return the code for testing/fallback
                return (true, $"Không thể gửi email. Mã xác thực của bạn là: {resetToken}\nMã này sẽ hết hạn sau 30 phút.");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi xử lý quên mật khẩu: {ex.Message}");
        }
    }

    public async Task<(bool success, string? message)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.Token))
            {
                return (false, "Token không hợp lệ.");
            }

            // Validate 6-digit code
            if (dto.Token.Length != 6 || !int.TryParse(dto.Token, out _))
            {
                return (false, "Mã xác thực phải là 6 chữ số.");
            }

            // Check if token exists and is not expired
            if (!_resetTokens.ContainsKey(dto.Token))
            {
                return (false, "Mã xác thực không hợp lệ hoặc đã được sử dụng.");
            }

            if (DateTime.Now > _resetTokens[dto.Token])
            {
                // Remove expired token
                _resetTokens.Remove(dto.Token);
                _tokenToUsername.Remove(dto.Token);
                return (false, "Mã xác thực đã hết hạn. Vui lòng yêu cầu mã mới.");
            }

            // Get the username associated with this token
            var username = _tokenToUsername[dto.Token];
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                // Clean up invalid token
                _resetTokens.Remove(dto.Token);
                _tokenToUsername.Remove(dto.Token);
                return (false, "Người dùng không tồn tại.");
            }

            if (string.IsNullOrEmpty(dto.NewPassword))
                return (false, "Mật khẩu mới không được để trống.");

            // Update password and remove used token
            user.Password = dto.NewPassword;
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();
            
            // Remove used token from both dictionaries
            _resetTokens.Remove(dto.Token);
            _tokenToUsername.Remove(dto.Token);

            return (true, "Đặt lại mật khẩu thành công.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi đặt lại mật khẩu: {ex.Message}");
        }
    }

    public async Task<(bool success, string? message)> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return (false, "Người dùng không tồn tại.");

            // Verify current password
            if (user.Password != dto.CurrentPassword) // TODO: Use proper password hashing comparison
                return (false, "Mật khẩu hiện tại không chính xác.");

            if (string.IsNullOrEmpty(dto.NewPassword))
                return (false, "Mật khẩu mới không được để trống.");

            // Update password
            user.Password = dto.NewPassword; // TODO: Hash password in production
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveAsync();

            return (true, "Đổi mật khẩu thành công.");
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi đổi mật khẩu: {ex.Message}");
        }
    }

    private string GenerateResetToken()
    {
        // Generate a random 6-digit code for password reset
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private void CleanupExpiredTokens()
    {
        // Remove expired tokens to prevent memory leaks
        var expiredTokens = _resetTokens.Where(kvp => DateTime.Now > kvp.Value)
                                       .Select(kvp => kvp.Key)
                                       .ToList();
        
        foreach (var token in expiredTokens)
        {
            _resetTokens.Remove(token);
            _tokenToUsername.Remove(token);
        }
    }

    private string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            return email;

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 3)
        {
            return $"{localPart[0]}***@{domain}";
        }

        return $"{localPart.Substring(0, 2)}***{localPart[^1]}@{domain}";
    }

    public async Task<(bool success, string? message)> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return (false, "Không tìm thấy người dùng.");

        if (string.IsNullOrEmpty(dto.Username))
            return (false, "Username không được để trống.");
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username && u.UserId != userId))
            return (false, "Tên đăng nhập đã tồn tại.");
            
        user.Username = dto.Username;
        user.Fullname = dto.Fullname;
        user.Phone = dto.Phone;
        user.Birthdate = dto.Birthdate;
        user.Address = dto.Address;

        if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
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
}
