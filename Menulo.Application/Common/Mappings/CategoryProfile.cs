using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Categories.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Common.Mappings
{
    public sealed class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // A) Entity → Response (cho API/Datatables)
            CreateMap<Category, CategoryResponse>()
                .ForMember(
                    dest => dest.RestaurantName,
                    opt => opt.MapFrom<RestaurantNameResolver>()
                );

            // B) Entity → DTO (cho Application/service trả về)
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.RestaurantName,
                    o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.Name : null));

            // C) Request → Entity (tạo mới trực tiếp)
            CreateMap<CreateCategoryRequest, Category>();

            // D) DTO → Entity (cập nhật) - (thêm Condition để tránh overwrite null)
            CreateMap<UpdateCategoryDto, Category>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // E) Entity → DTO (load lên form edit)
            CreateMap<Category, UpdateCategoryDto>();
        }

        /// <summary>
        /// RestaurantNameResolver tiêm ICurrentUser ⇒ non-superadmin nhận RestaurantName = null.
        /// (Shaping theo role ở Web layer.)
        /// </summary>
        public class RestaurantNameResolver : IValueResolver<Category, CategoryResponse, string?>
        {
            private readonly ICurrentUser _current;
            public RestaurantNameResolver(ICurrentUser current) => _current = current;

            public string? Resolve(Category src, CategoryResponse dest, string? destMember, ResolutionContext context)
            {
                return _current.IsSuperAdmin ? src.Restaurant?.Name : null;
            }
        }
    }
}
