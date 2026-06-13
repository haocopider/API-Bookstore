using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Address { get; set; }

    public int CurrentPoints { get; set; }

    public int? Rank { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public int TotalPoints { get; set; }

    public string? PhoneNumber { get; set; }

    public string? FcmToken { get; set; }

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
