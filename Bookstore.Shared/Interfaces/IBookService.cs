using Bookstore.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto> GetBookByIdAsync(int id);
        Task<IEnumerable<BookDto>> SearchBooksAsync(BookFilterRequestDto filter);
        Task<IEnumerable<BookDto>> GetBooksByIdsAsync(List<int> ids);
        Task<IEnumerable<BookDto>> GetBooksByCategoryIdAsync(int categoryId);
        Task<IEnumerable<BookDto>> GetBooksByPromotionIdAsync(int promotionId);
        Task<IEnumerable<BookDto>> GetBooksByAuthorIdAsync(int authorId);
        Task<bool> ReviewBookAsync(ReviewBookDto reviewRequest);

        Task<bool> CreateBookAsync(CreateBookDto request);
        Task<bool> UpdateBookAsync(int id, UpdateBookDto request);
        Task<bool> RestockAsync(RestockDto request);
    }
}
