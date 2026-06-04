using System.ComponentModel.DataAnnotations;

namespace Bookstore.Shared.Dtos
{
    public class BookFilterRequestDto
    {
        public string? SearchText { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? AuthorName { get; set; }
        public string? Tag { get; set; }
    }

    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? AuthorName { get; set; }
        public string? Publisher { get; set; }
        public decimal Price { get; set; }
        public PromotionInfoDTO? Promotion { get; set; }
    }

    public class BookAdminDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? AuthorName { get; set; }
        public string? Publisher { get; set; }
        public decimal Price { get; set; }
        public PromotionInfoDTO? Promotion { get; set; }
        public int Stock { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class BookPerPage 
    {
        List<BookDto> Books { get; set; } = new List<BookDto>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class CreateBookDto
    {
        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public string? CoverImageUrl { get; set; }

        [Required]
        public string AuthorName { get; set; } = null!;

        [Required]
        public List<string> CategoryNames { get; set; } = new List<string>();

        [Required]
        public List<CreateBookFormatDto> Formats { get; set; } = new List<CreateBookFormatDto>();
    }

    public class CreateBookFormatDto
    {
        [Required]
        public int Format { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class UpdateBookDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public string? CoverImageUrl { get; set; }
        public string AuthorName { get; set; } = null!;
        public List<string> CategoryNames { get; set; } = new List<string>();
    }

    public class RestockDto
    {
        public int BookFormatId { get; set; }
        public int AddedQuantity { get; set; }
        public decimal Price { get; set; }
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
