using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Book, BookDto>()
                            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.CoverImageUrl))
                            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : "Đang cập nhật"))
                            .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                                src.BookFormats.Any() ? src.BookFormats.Min(f => f.Price) : 0));
            CreateMap<Review, ReviewBookDto>();
            CreateMap<Author, AuthorDto>().ForMember(dest => dest.Books, opt => opt.MapFrom(src => src.Books)); ;
            CreateMap<Category, CategoryDto>();

            CreateMap<OrderItem, OrderItemDto>()
                            .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.ItemId))
                            .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.SnapshotBookTitle))
                            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.SnapshotBookImg))
                            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.PriceAtPurchase))
                            .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.SnapshotUnitPrice));

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<User, UserInfoDto>();
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
            CreateMap<Promotion, PromotionInfoDTO>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Banner));
        }
    }
}
