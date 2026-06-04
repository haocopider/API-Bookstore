using Blazored.LocalStorage;
using Bookstore.AdminClient.Auth;
using Bookstore.AdminClient.Interfaces;
using Bookstore.Shared.Dtos;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Bookstore.AdminClient.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private class LoginResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }

            [JsonPropertyName("tokenExpiryUtc")]
            public DateTime TokenExpiryUtc { get; set; }
        }

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> Login(LoginDto loginModel)
        {
            if (_httpClient.BaseAddress == null)
            {
                throw new InvalidOperationException("HttpClient.BaseAddress is null. Configure the API base address in Program.cs (ApiBaseUrl) or register a client with a BaseAddress.");
            }

            var loginRequest = new { UserName = loginModel.UserName, Password = loginModel.Password };

            var requestUri = new Uri(_httpClient.BaseAddress, "api/admins/login");

            var response = await _httpClient.PostAsJsonAsync(requestUri, loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                return false;
            }

            await _localStorage.SetItemAsync("authToken", loginResponse.Token);
            await _localStorage.SetItemAsync("tokenExpiryUtc", loginResponse.TokenExpiryUtc);

            await ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(loginResponse.Token);

            return true;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("tokenExpiryUtc");
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            return token;
        }
        public async Task<RegisterDto> Register(RegisterDto registerModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", registerModel);

                // Đọc kết quả trả về từ Server
                var result = await response.Content.ReadFromJsonAsync<RegisterDto>();
                return result ?? new RegisterDto { IsSuccess = false, ErrorMessage = "Lỗi không xác định." };
            }
            catch (Exception ex)
            {
                return new RegisterDto { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<string?> RefreshToken()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                {
                    await Logout();
                    return null;
                }

                var refreshDto = new RefreshTokenDto
                {
                    Token = token,
                    RefreshToken = refreshToken
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", refreshDto);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                    if (result != null && result.IsSuccess && !string.IsNullOrEmpty(result.Token))
                    {
                        await _localStorage.SetItemAsync("authToken", result.Token);
                        await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
                        return result.Token;
                    }
                }
            }
            catch (Exception)
            {
                // Gặp lỗi mạng hoặc lỗi server
            }
            await Logout();
            return null;
        }
    }
}