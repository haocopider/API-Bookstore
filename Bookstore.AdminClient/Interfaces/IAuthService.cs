using Bookstore.Shared.Dtos;

namespace Bookstore.AdminClient.Interfaces
{
    public interface IAuthService
    {
        Task<bool> Login(LoginDto loginModel);
        Task Logout();
        Task<string?> GetAccessTokenAsync();
        Task<string?> RefreshToken();
        Task<RegisterDto> Register(RegisterDto registerModel);
    }
}
