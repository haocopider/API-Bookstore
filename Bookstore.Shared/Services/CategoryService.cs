using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<bool> CreateCategoryAsync(CreateCategoryDto request)
        {
            // Tự động tạo slug nếu trống
            var slug = string.IsNullOrWhiteSpace(request.Slug)
                ? request.Name.ToLower().Replace(" ", "-")
                : request.Slug;

            var category = new Category
            {
                Name = request.Name,
                Slug = slug,
                ParentId = request.ParentId
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(int id, UpdateCategoryDto request)
        {
            var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return false;

            var slug = string.IsNullOrWhiteSpace(request.Slug)
                ? request.Name.ToLower().Replace(" ", "-")
                : request.Slug;

            category.Name = request.Name;
            category.Slug = slug;
            category.ParentId = request.ParentId;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return false;

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
