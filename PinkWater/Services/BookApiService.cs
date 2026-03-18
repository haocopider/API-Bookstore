using Bookstore.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;

namespace PinkWater.Services
{
    public class BookApiService
    {
        private readonly HttpClient _httpClient;
        private string _baseUrl = "https://10.160.2.177:7259/api";

        public BookApiService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _httpClient = new HttpClient(handler);
        }

        public async Task<List<BookDto>> GetBooksAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<BookDto>>($"{_baseUrl}/books") ?? new();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi rồi bạn ơi: {ex.Message}");
                return new List<BookDto>();
            }
        }
    }
}
