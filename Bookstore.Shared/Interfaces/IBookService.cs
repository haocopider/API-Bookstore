using Bookstore.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponseDto>> GetAllBooksAsync();
        Task<IEnumerable<BookResponseDto>> SearchBooksAsync(BookFilterRequestDto filter);
        Task<IEnumerable<BookResponseDto>> GetBooksByIdsAsync(List<int> ids);
        Task<IEnumerable<BookResponseDto>> GetBooksByCategoryIdAsync(int categoryId);
    }
}
