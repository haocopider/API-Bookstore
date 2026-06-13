namespace Bookstore.Shared.Dtos
{
    public class NotiDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
        public int? OrderId { get; set; }
        public string? OrderCode { get; set; }
    }
}
