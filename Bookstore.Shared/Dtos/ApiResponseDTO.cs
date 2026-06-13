using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class ApiResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public object? Data { get; set; }
    }
}
