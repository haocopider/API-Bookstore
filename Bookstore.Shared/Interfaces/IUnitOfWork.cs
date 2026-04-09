using Bookstore.Shared.Models;

namespace Bookstore.Shared.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Book> Books { get; }
        IRepository<Order> Orders { get; }
        IRepository<BookFormat> BookFormats { get; }
        IRepository<Author> Authors { get; }
        IRepository<Category> Categories { get; }
        IRepository<User> Users { get; }
        Task<int> CommitAsync();
    }
}
