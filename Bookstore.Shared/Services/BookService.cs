using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;

namespace Bookstore.Shared.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookResponseDto>> GetAllBooksAsync()
        {
            var booksFromDb = await _unitOfWork.Books.GetAllAsync(
                b => b.Author,
                b => b.BookFormats
            );
            return _mapper.Map<IEnumerable<BookResponseDto>>(booksFromDb);
        }

        public async Task<IEnumerable<BookResponseDto>> SearchBooksAsync(BookFilterRequestDto filter)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b =>
                    b.IsDeleted == false &&
                    // Lọc theo tên sách
                    (string.IsNullOrEmpty(filter.SearchText) || b.Title.Contains(filter.SearchText)) &&
                    (string.IsNullOrEmpty(filter.Tag) || b.Categories.Any(c => c.Slug == filter.Tag)) &&
                    (!filter.MinPrice.HasValue || b.BookFormats.Any(f => f.Price >= filter.MinPrice.Value)) &&
                    (!filter.MaxPrice.HasValue || b.BookFormats.Any(f => f.Price <= filter.MaxPrice.Value)),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories
            );

            return _mapper.Map<IEnumerable<BookResponseDto>>(booksFromDb);
        }

        public async Task<IEnumerable<BookResponseDto>> GetBooksByIdsAsync(List<int> ids)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => ids.Contains(b.Id),
                b => b.Author,
                b => b.BookFormats
            );
            return _mapper.Map<IEnumerable<BookResponseDto>>(booksFromDb);
        }

        public async Task<IEnumerable<BookResponseDto>> GetBooksByCategoryIdAsync(int categoryId)
        {
            var books = await _unitOfWork.Books.FindAsync(b => b.Categories.Any(c => c.Id == categoryId), b => b.Author, b => b.BookFormats);
            return _mapper.Map<IEnumerable<BookResponseDto>>(books);
        }
    }
}
