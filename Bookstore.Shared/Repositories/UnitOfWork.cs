using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IRepository<Book> Books { get; private set; }
        public IRepository<BookFormat> BookFormats { get; private set; }
        public IRepository<Order> Orders { get; private set; }
        public IRepository<Author> Authors { get; private set; }
        public IRepository<Category> Categories { get; private set; }
        public IRepository<User> Users { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Books = new Repository<Book>(_context);
            BookFormats = new Repository<BookFormat>(_context);
            Orders = new Repository<Order>(_context);
            Authors = new Repository<Author>(_context);
            Categories = new Repository<Category>(_context);
            Users = new Repository<User>(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
