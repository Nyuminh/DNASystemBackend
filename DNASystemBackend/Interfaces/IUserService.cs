namespace DNASystemBackend.Interfaces
{
    using DNASystemBackend.DTOs;
    using DNASystemBackend.Models;
    using Microsoft.AspNetCore.Mvc;

    public interface IUserService
    {
        Task<string?> AuthenticateAsync(LoginDto loginDto);
        Task<(bool success, string? message, string? token)> RegisterAsync(RegisterDto registerDto);

        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersByRoleAsync(string roleName);
        Task<User?> GetCurrentUserAsync(string userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<(bool success, string? message)> CreateAsync(User user);
        Task<(bool success, string? message)> CreateUserAsync(CreateUserDto dto);
        Task<(bool success, string? message)> UpdateUserAsync(string userId, UpdateUserDto dto);
        Task<(bool success, string? message)> UpdateProfileAsync(string userId, UpdateProfileDto dto);

        Task<IActionResult> UpdateUserImageAsync(string id, [FromForm] UpdateUserImageDto dto);
        Task<(bool success, string? message)> DeleteUserAsync(string userId, string currentUserId);
        Task<object?> GetUserForEditAsync(string userId);
        
        // Password related methods
        Task<(bool success, string? message)> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<(bool success, string? message)> ResetPasswordAsync(ResetPasswordDto dto);
        Task<(bool success, string? message)> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    }


}
