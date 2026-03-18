using Bookstore.Api.Models;
using Bookstore.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Api.Services
{
    public class BookService
    {
        private readonly BookstoreContext _context;

        public BookService(BookstoreContext context)
        {
            _context = context;
        }

        public async Task<List<BookDto>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Include(b => b.BookFormats)
                .Where(b => b.IsDeleted != true)
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author != null ? b.Author.Name : "Ẩn danh",
                    Description = b.Description,
                    // Lấy giá thấp nhất từ các định dạng (Giấy/Ebook)
                    MinPrice = b.BookFormats.Any() ? b.BookFormats.Min(f => f.Price) : 0,
                    // Lấy danh sách tên thể loại trực tiếp từ thực thể Category
                    Categories = b.Categories.Select(c => c.Name).ToList()
                })
                .ToListAsync();
        }
    }
}
