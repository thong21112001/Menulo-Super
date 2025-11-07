using AutoMapper;
using Menulo.Application.Features.MenuItems.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Common.Mappings
{
    public sealed class MenuItemsProfile : Profile
    {
        public MenuItemsProfile()
        {
            // Map từ Entity -> DTO trả về
            CreateMap<MenuItem, MenuItemDto>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty));

            // Map từ DTO (sạch) -> Entity
            CreateMap<CreateMenuItemDto, MenuItem>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? 0)) // Ví dụ: gán 0 nếu null
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false));

            // Map từ Entity -> DTO hàng (Row DTO)
            CreateMap<MenuItem, MenuItemRowDto>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty))
                .ForMember(dest => dest.RestaurantName,
                            opt => opt.MapFrom(src => src.Restaurant != null ? src.Restaurant.Name : string.Empty))
                .ForMember(dest => dest.Description,
                            opt => opt.MapFrom(src => src.Description ?? string.Empty));

            // Map DTO -> DTO (để DataTablesControllerBase hoạt động)
            CreateMap<MenuItemRowDto, MenuItemRowDto>();

            // Map từ Entity -> DTO details (xem chi tiết món)
            CreateMap<MenuItem, MenuItemDetailsDto>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty));
        }
    }
}
