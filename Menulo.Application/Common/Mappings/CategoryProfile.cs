using AutoMapper;
using Menulo.Application.Features.Categories.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Common.Mappings
{
    public sealed class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryResponse>()
                .ForMember(
                    dest => dest.RestaurantName,
                    // Cấu hình này báo cho AutoMapper rằng thuộc tính RestaurantName trong CategoryResponse
                    // sẽ được lấy từ thuộc tính Name của đối tượng Restaurant có liên quan.
                    // Entity Framework Core đủ thông minh để dịch điều này thành một câu lệnh
                    // LEFT JOIN trong SQL và tự xử lý trường hợp Restaurant bị null.
                    opt => opt.MapFrom(s => s.Restaurant != null ? s.Restaurant.Name : null)
                );

            // Entity → DTO (dùng khi service trả về CategoryDto)
            CreateMap<Category, CategoryDto>()
                .ForMember(d => d.RestaurantName,
                    o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.Name : null));

            // Request → Entity (tạo mới)
            CreateMap<CreateCategoryRequest, Category>();

            // DTO → Entity (tạo mới)
            CreateMap<CreateCategoryDto, Category>();

            // DTO → Entity (cập nhật)
            CreateMap<UpdateCategoryDto, Category>();

            // Entity → DTO (load lên form edit)
            CreateMap<Category, UpdateCategoryDto>();
        }
    }
}
