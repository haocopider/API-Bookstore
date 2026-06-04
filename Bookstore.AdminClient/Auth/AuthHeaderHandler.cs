using Blazored.LocalStorage;
using Bookstore.AdminClient.Interfaces;
using System.Net.Http.Headers;

namespace Bookstore.AdminClient.Auth
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IServiceProvider _serviceProvider;

        public AuthHeaderHandler(ILocalStorageService localStorage, IServiceProvider serviceProvider)
        {
            _localStorage = localStorage;
            _serviceProvider = serviceProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Gửi request
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (request.RequestUri!.AbsolutePath.Contains("refresh-token") ||
                    request.RequestUri!.AbsolutePath.Contains("login"))
                {
                    return response;
                }

                var authService = _serviceProvider.GetRequiredService<IAuthService>();

                var newToken = await authService.RefreshToken();

                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }
}