using System.ComponentModel.DataAnnotations;

namespace Bookstore.Shared.Dtos
{
    public class BookFilterRequestDto
    {
        public string? SearchText { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Tag { get; set; }
    }

    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? AuthorName { get; set; }
        public decimal Price { get; set; }
        public PromotionInfoDTO? Promotion { get; set; }
    }

    public class ReviewBookDto
    {
        [Required]
        public int UserId { get; set; }
        public string UsertName { get; set; }
        [Required]
        public int BookId { get; set; }
        [Required]
        public int RatingValue { get; set; }
        [MaxLength(500)]
        public string? Comment { get; set; }
        public string? ReviewImg { get; set; }
    }
}
