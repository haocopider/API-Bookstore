using Bookstore.Shared.Dtos;
using Bookstore.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Interfaces
{
    public interface IOrderService
    {
        Task<CheckoutResponseDto> CheckoutAsync(CheckoutRequestDto request);

        Task<IEnumerable<OrderHistoryDto>> GetMyOrdersAsync(int userId);
    }
}
