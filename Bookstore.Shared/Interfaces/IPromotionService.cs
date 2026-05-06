using Bookstore.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Interfaces
{
    public interface IPromotionService
    {
        Task<PromotionInfoDTO?> GetBestPromotionForBookAsync(int bookId, decimal originalPrice);
        Task<IEnumerable<PromotionInfoDTO>> GetPromotionInfosAsync();
        Task<bool> CreatePromotionAsync(CreatePromotionRequest request);
    }
}
