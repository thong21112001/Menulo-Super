using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.ResTables.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Common.Mappings
{
    public sealed class ResTableProfile : Profile
    {
        public ResTableProfile()
        {
            // ===== Entity -> Response =====
            CreateMap<RestaurantTable, ResTableDto>();

            // List DataTable and Details API
            CreateMap<RestaurantTable, ResTableResponse>().ForMember(
                    dest => dest.RestaurantName,
                    opt => opt.MapFrom<RestaurantNameResolver>()
                );

            // ===== Request -> Command DTO =====
            // Tự động map
            CreateMap<ResTableRequest.Create, CreateResTableDto>();
            CreateMap<ResTableRequest.Update, UpdateResTableDto>();

            // Update: tránh overwrite bằng null
            CreateMap<UpdateResTableDto, RestaurantTable>()
                // Bỏ qua các thuộc tính null từ DTO để không ghi đè dữ liệu hiện có
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val != null));
        }
    }

    public class RestaurantNameResolver : IValueResolver<RestaurantTable, ResTableResponse, string?>
    {
        private readonly ICurrentUser _current;
        public RestaurantNameResolver(ICurrentUser current) => _current = current;

        public string? Resolve(RestaurantTable src, ResTableResponse dest, string? destMember, ResolutionContext context)
        {
            return _current.IsSuperAdmin ? src.Restaurant?.Name : null;
        }
    }
}
