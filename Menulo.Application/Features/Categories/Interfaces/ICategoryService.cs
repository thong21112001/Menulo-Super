using Menulo.Application.Features.Categories.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Features.Categories.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
        Task<CategoryDto> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default);
        Task DeleteAsync(int categoryId, CancellationToken ct = default);
        Task<CategoryDto?> GetByIdAsync(int categoryId, CancellationToken ct = default);
        IQueryable<Category> GetQueryableCategories(bool isSuperAdmin, int? restaurantId);
    }
}
