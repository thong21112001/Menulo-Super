using AutoMapper;
using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Common.Mappings
{
    public sealed class RestaurantProfile : Profile
    {
        public RestaurantProfile()
        {
            // ===== Entity -> Response =====
            // List
            CreateMap<Restaurant, RestaurantRowDto>();

            // Details
            CreateMap<Restaurant, RestaurantDetailsDto>()
                .ForMember(d => d.LogoUrl, o => o.MapFrom(s =>
                    s.LogoImage != null && s.LogoImage.Length > 0
                        ? $"data:image/png;base64,{Convert.ToBase64String(s.LogoImage)}"
                        : null));

            // ===== Request -> Command DTO =====
            CreateMap<RestaurantRequest.Create, CreateRestaurantDto>();
            CreateMap<RestaurantRequest.Update, UpdateRestaurantDto>();

            // ===== Command DTO -> Entity =====
            CreateMap<CreateRestaurantDto, Restaurant>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

            // Update: tránh overwrite bằng null (ví dụ LogoImage=null không xoá logo)
            CreateMap<UpdateRestaurantDto, Restaurant>()
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}
