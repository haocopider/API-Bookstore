using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPromotionService _promotionService;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper, IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _promotionService = promotionService;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var booksFromDb = await _unitOfWork.Books.GetAllAsync(
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews
            );
            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }
            return bookDto;
        }

        public async Task<IEnumerable<BookDto>> SearchBooksAsync(BookFilterRequestDto filter)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b =>
                    b.IsDeleted == false &&
                    (
                        string.IsNullOrEmpty(filter.SearchText) ||
                        b.Title.Contains(filter.SearchText) ||
                        (b.Author != null && b.Author.Name.Contains(filter.SearchText))
                    ) &&
                    (string.IsNullOrEmpty(filter.Tag) || b.Categories.Any(c => c.Slug == filter.Tag)) &&
                    (!filter.MinPrice.HasValue || b.BookFormats.Any(f => f.Price >= filter.MinPrice.Value)) &&
                    (!filter.MaxPrice.HasValue || b.BookFormats.Any(f => f.Price <= filter.MaxPrice.Value)),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews
            );

            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }

            return bookDto;
        }
        public async Task<IEnumerable<BookDto>> GetBooksByIdsAsync(List<int> ids)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => ids.Contains(b.Id),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews
            );

            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }
            return bookDto;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByCategoryIdAsync(int categoryId)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => b.Categories.Any(c => c.Id == categoryId),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Reviews);
            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }
            return bookDto;
        }

        public async Task<bool> ReviewBookAsync(ReviewBookDto reviewRequest)
        {
            var review = _mapper.Map<Review>(reviewRequest);
            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<BookDto> GetBookByIdAsync(int id)
        {
            var bookFromDb = await _unitOfWork.Books.FindAsync(
                b => b.Id == id,
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories
            );

            var bookDto = _mapper.Map<BookDto>(bookFromDb.FirstOrDefault());

            if (bookDto != null)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(bookDto.Id, bookDto.Price);
                if (promotion != null)
                {
                    bookDto.Promotion = promotion;
                }
            }

            return bookDto;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByPromotionIdAsync(int promotionId)
        {
            var now = DateTime.UtcNow;

            var targetPromotionList = await _unitOfWork.Promotions.FindAsync(
                p => p.Id == promotionId,
                p => p.Categories
            );

            var targetPromotion = targetPromotionList.FirstOrDefault();
            if (targetPromotion == null)
                return Enumerable.Empty<BookDto>();

            var categoryIds = targetPromotion.Categories.Select(c => c.Id).ToList();

            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => b.Promotions.Any(p => p.Id == promotionId) ||
                     b.Categories.Any(c => categoryIds.Contains(c.Id)),
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories,
                b => b.Promotions
            );

            var bookDtos = _mapper.Map<List<BookDto>>(booksFromDb);

            var activePromotions = await _unitOfWork.Promotions.FindAsync(
                p => p.IsActive && p.StartDate <= now && p.EndDate >= now,
                p => p.Books,
                p => p.Categories
            );

            foreach (var book in bookDtos)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }

            return bookDtos;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByAuthorIdAsync(int authorId)
        {
            var booksFromDb = await _unitOfWork.Books.FindAsync(
                b => b.AuthorId == authorId,
                b => b.Author,
                b => b.BookFormats,
                b => b.Categories
            );
            var bookDto = _mapper.Map<IEnumerable<BookDto>>(booksFromDb);

            foreach (var book in bookDto)
            {
                var promotion = await _promotionService.GetBestPromotionForBookAsync(book.Id, book.Price);
                if (promotion != null)
                {
                    book.Promotion = promotion;
                }
            }

            return bookDto;
        }
    }
}
