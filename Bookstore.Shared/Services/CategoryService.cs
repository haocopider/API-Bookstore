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

    }
}
