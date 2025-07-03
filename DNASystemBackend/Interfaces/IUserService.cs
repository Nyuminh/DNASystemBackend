namespace DNASystemBackend.Interfaces
{
    using DNASystemBackend.DTOs;
    using DNASystemBackend.Models;

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
        Task<(bool success, string? message)> DeleteUserAsync(string userId, string currentUserId);
        Task<object?> GetUserForEditAsync(string userId);
    }


}
