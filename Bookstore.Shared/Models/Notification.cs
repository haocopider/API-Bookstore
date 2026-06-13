namespace Bookstore.Shared.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? OrderId { get; set; }

    public int UserId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual User User { get; set; } = null!;
}
