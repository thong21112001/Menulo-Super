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
        }
    }
}
