using Bookstore.Shared.Dtos;
using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync();
        Task<AuthorDto?> GetAuthorByIdAsync(int id);
        Task<IEnumerable<AuthorDto>> SearchAuthorsAsync(string name);
    }
}
