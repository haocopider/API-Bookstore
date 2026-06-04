using Bookstore.Shared.Dtos;


namespace Bookstore.Shared.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<bool> RegisterAsync(RegisterRequest request);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<bool> UpdateFcmToken(int userId, string fcmToken);
        Task<UserInfoDto?> GetProfileAsync(int userId);
    }
}
