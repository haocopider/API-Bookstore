using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class PromotionInfoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CreatePromotionRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int DiscountType { get; set; }

        [Required]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public List<int> BookIds { get; set; } = new List<int>();
        public List<int> CategoryIds { get; set; } = new List<int>();
    }

    public class UpdatePromotionRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int DiscountType { get; set; }

        [Required]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
        public string? Banner { get; set; }

        // Cập nhật lại danh sách sách và thể loại áp dụng
        public List<int> BookIds { get; set; } = new List<int>();
        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
