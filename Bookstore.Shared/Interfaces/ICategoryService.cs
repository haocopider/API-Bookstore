using Bookstore.Shared.Dtos;
using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<bool> CreateCategoryAsync(CreateCategoryDto request);
        Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto request);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
