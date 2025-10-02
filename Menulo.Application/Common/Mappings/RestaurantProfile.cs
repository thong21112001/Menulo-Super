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
            CreateMap<Restaurant, RestaurantDto>();

            // List DataTable
            CreateMap<Restaurant, RestaurantRowDto>();

            // Details API
            CreateMap<Restaurant, RestaurantDetailsDto>()
                .ForCtorParam("LogoUrl", opt => opt.MapFrom(src =>
                    src.LogoImage != null && src.LogoImage.Length > 0
                        ? $"data:image/png;base64,{Convert.ToBase64String(src.LogoImage)}"
                        : null))
                .ForCtorParam("CreatedAt", opt => opt.MapFrom(src => (DateTime?)src.CreatedAt));


            // ===== Request -> Command DTO =====
            // Tự động map
            CreateMap<RestaurantRequest.Create, CreateRestaurantDto>();
            CreateMap<RestaurantRequest.Update, UpdateRestaurantDto>();

            // ===== Command DTO -> Entity =====
            // Chỉ cần ForMember cho logic đặc biệt
            CreateMap<CreateRestaurantDto, Restaurant>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

            // Update: tránh overwrite bằng null (ví dụ LogoImage=null không xoá logo)
            CreateMap<UpdateRestaurantDto, Restaurant>()
                // Bỏ qua các thuộc tính null từ DTO để không ghi đè dữ liệu hiện có
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }
}
