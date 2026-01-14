using WilliamMetalAPI.Models;

namespace WilliamMetalAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserDto?> GetCurrentUserAsync(string userId);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ValidateTokenAsync(string token);

        // Single-user / no-JWT mode helper
        Task<UserDto?> GetDefaultUserAsync();

        // Register new user
        Task<UserDto> RegisterAsync(RegisterRequest request);
    }
}