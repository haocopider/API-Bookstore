using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.DTOs;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Book, BookResponseDto>()
                            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.CoverImageUrl))
                            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : "Đang cập nhật"))
                            .ForMember(dest => dest.StartingPrice, opt => opt.MapFrom(src =>
                                src.BookFormats.Any() ? src.BookFormats.Min(f => f.Price) : 0));
            CreateMap<Order, OrderHistoryDto>();
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<Author, AuthorDto>().ForMember(dest => dest.Books, opt => opt.MapFrom(src => src.Books)); ;
            CreateMap<Category, CategoryDto>();

            CreateMap<User, UserInfoDto>();
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
