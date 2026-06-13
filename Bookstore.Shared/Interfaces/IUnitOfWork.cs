using Bookstore.Shared.Models;

namespace Bookstore.Shared.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Book> Books { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderDetails { get; }
        IRepository<BookFormat> BookFormats { get; }
        IRepository<Author> Authors { get; }
        IRepository<Category> Categories { get; }
        IRepository<User> Users { get; }
        IRepository<Promotion> Promotions { get; }
        IRepository<Review> Reviews { get; }
        IRepository<Role> Roles { get; }
        IRepository<Permission> Permissions { get; }
        IRepository<RolePermission> RolePermissions { get; }
        IRepository<Admin> Admins { get; }
        IRepository<Message> Messages { get; }
        IRepository<Conversation> Conversations { get; }
        IRepository<Notification> Notifications { get; }
        Task<int> CommitAsync();
    }
}
