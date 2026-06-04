using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Helpers;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PromotionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<bool> CreatePromotionAsync(CreatePromotionRequest request)
        {
            if (request.EndDate <= request.StartDate)
            {
                throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");
            }

            var promotion = new Promotion
            {
                Name = request.Name,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,

                Books = new List<Book>(),
                Categories = new List<Category>()
            };

            if (request.BookIds != null && request.BookIds.Any())
            {
                var books = await _unitOfWork.Books.FindAsync(b => request.BookIds.Contains(b.Id));

                foreach (var book in books)
                {
                    promotion.Books.Add(book);
                }
            }

            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                var categories = await _unitOfWork.Categories.FindAsync(c => request.CategoryIds.Contains(c.Id));

                foreach (var category in categories)
                {
                    promotion.Categories.Add(category);
                }
            }

            await _unitOfWork.Promotions.AddAsync(promotion);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<PromotionInfoDTO?> GetBestPromotionForBookAsync(int bookId, decimal originalPrice)
        {
            var now = DateTime.UtcNow;

            var applicablePromotions = await _unitOfWork.Promotions.FindAsync(p =>
                p.IsActive &&
                p.StartDate <= now &&
                p.EndDate >= now &&
                (
                    p.Books.Any(b => b.Id == bookId)
                    || p.Categories.Any(c => c.Books.Any(b => b.Id == bookId))
                )
            );

            var bestPromo = PromotionHelper.GetBestPromotionFromList(applicablePromotions, originalPrice);

            if (bestPromo == null) return null;

            return _mapper.Map<PromotionInfoDTO>(bestPromo);
        }
        public async Task<IEnumerable<PromotionInfoDTO>> GetPromotionInfosAsync()
        {
            var data = await _unitOfWork.Promotions.GetAllAsync();
            return _mapper.Map<IEnumerable<PromotionInfoDTO>>(data);
        }

        public async Task<PromotionInfoDTO?> GetPromotionInfoByIdAsync(int id)
        {
            var promotion = await _unitOfWork.Promotions.GetFirstOrDefaultAsync(p => p.Id == id);
            if (promotion == null) return null;
            var promoInfo = _mapper.Map<PromotionInfoDTO>(promotion);
            return promoInfo;
        }

        public async Task<bool> UpdatePromotionAsync(int id, UpdatePromotionRequest request)
        {
            if (request.EndDate <= request.StartDate)
                throw new ArgumentException("Ngày kết thúc phải lớn hơn ngày bắt đầu.");

            // Include cả Books và Categories để Entity Framework theo dõi sự thay đổi
            var promotions = await _unitOfWork.Promotions.FindAsync(
                p => p.Id == id,
                p => p.Books,
                p => p.Categories
            );
            var promotion = promotions.FirstOrDefault();

            if (promotion == null) return false;

            // 1. Cập nhật thông tin cơ bản
            promotion.Name = request.Name;
            promotion.DiscountType = request.DiscountType;
            promotion.DiscountValue = request.DiscountValue;
            promotion.StartDate = request.StartDate;
            promotion.EndDate = request.EndDate;
            promotion.IsActive = request.IsActive;
            promotion.Banner = request.Banner;

            // 2. Cập nhật danh sách Sách áp dụng
            promotion.Books.Clear(); // Xóa các sách cũ khỏi khuyến mãi
            if (request.BookIds != null && request.BookIds.Any())
            {
                var books = await _unitOfWork.Books.FindAsync(b => request.BookIds.Contains(b.Id));
                foreach (var book in books)
                {
                    promotion.Books.Add(book);
                }
            }

            // 3. Cập nhật danh sách Thể loại áp dụng
            promotion.Categories.Clear(); // Xóa các thể loại cũ
            if (request.CategoryIds != null && request.CategoryIds.Any())
            {
                var categories = await _unitOfWork.Categories.FindAsync(c => request.CategoryIds.Contains(c.Id));
                foreach (var category in categories)
                {
                    promotion.Categories.Add(category);
                }
            }

            _unitOfWork.Promotions.Update(promotion);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            var promotion = await _unitOfWork.Promotions.GetFirstOrDefaultAsync(p => p.Id == id);
            if (promotion == null) return false;

            promotion.IsActive = false;

            _unitOfWork.Promotions.Update(promotion);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}
