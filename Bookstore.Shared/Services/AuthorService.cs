using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync()
        {
            var authors = await _unitOfWork.Authors.GetAllAsync(a=> a.Books);
            return _mapper.Map<IEnumerable<AuthorDto>>(authors);
        }

        public async Task<AuthorDto?> GetAuthorByIdAsync(int id)
        {
            var author = await _unitOfWork.Authors.GetFirstOrDefaultAsync(
                a => a.Id == id,
                a => a.Books
            );

            return _mapper.Map<AuthorDto?>(author);
        }

        public async Task<IEnumerable<AuthorDto>> SearchAuthorsAsync(string name)
        {
            var authors = await _unitOfWork.Authors.FindAsync(a => a.Name.Contains(name), a => a.Books);
            return _mapper.Map<IEnumerable<AuthorDto>>(authors);
        }
    }
}
