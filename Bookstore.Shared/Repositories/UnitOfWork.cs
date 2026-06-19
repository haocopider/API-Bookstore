using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        private IDbContextTransaction _currentTransaction;
        public IRepository<Book> Books { get; private set; }
        public IRepository<BookFormat> BookFormats { get; private set; }
        public IRepository<Order> Orders { get; private set; }
        public IRepository<OrderItem> OrderDetails { get; private set; }
        public IRepository<Author> Authors { get; private set; }
        public IRepository<Category> Categories { get; private set; }
        public IRepository<User> Users { get; private set; }
        public IRepository<Promotion> Promotions { get; private set; }
        public IRepository<Review> Reviews { get; private set; }
        public IRepository<Role> Roles { get; private set; }
        public IRepository<Permission> Permissions { get; private set; }
        public IRepository<RolePermission> RolePermissions { get; private set; }
        public IRepository<Admin> Admins { get; private set; }
        public IRepository<Message> Messages { get; private set; }
        public IRepository<Conversation> Conversations { get; private set; }
        public IRepository<Notification> Notifications { get; private set; }
        public IRepository<AuditLog> AuditLogs { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Books = new Repository<Book>(_context);
            Reviews = new Repository<Review>(_context);
            BookFormats = new Repository<BookFormat>(_context);
            Orders = new Repository<Order>(_context);
            OrderDetails = new Repository<OrderItem>(_context);
            Authors = new Repository<Author>(_context);
            Categories = new Repository<Category>(_context);
            Promotions = new Repository<Promotion>(_context);
            Users = new Repository<User>(_context);
            Roles = new Repository<Role>(_context);
            Permissions = new Repository<Permission>(_context);
            RolePermissions = new Repository<RolePermission>(_context);
            Admins = new Repository<Admin>(_context);
            Messages = new Repository<Message>(_context);
            Conversations = new Repository<Conversation>(_context);
            Notifications = new Repository<Notification>(_context);
            AuditLogs = new Repository<AuditLog>(_context);
        }

        //public async Task<int> CommitAsync()
        //{
        //    return await _context.SaveChangesAsync();
        //}

        public async Task ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("Một Transaction khác đang được thực thi.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync();
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task<int> CommitAsync()
        {
            try
            {
                int result = await _context.SaveChangesAsync();

                if (_currentTransaction != null)
                {
                    await _currentTransaction.CommitAsync();
                }

                return result;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}
