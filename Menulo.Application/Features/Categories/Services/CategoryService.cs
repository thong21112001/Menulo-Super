using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Categories.Dtos;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Application.Features.Categories.Services
{
    public sealed class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<Category> _repo;
        private readonly IMapper _mapper;


        public CategoryService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _repo = _uow.Repository<Category>();
            _mapper = mapper;
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
        {
            var entity = new Category { CategoryName = dto.CategoryName.Trim(), RestaurantId = dto.RestaurantId };
            await _repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<CategoryDto>(entity);
        }

        public async Task<CategoryDto> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(dto.CategoryId, ct) ?? throw new KeyNotFoundException("Category not found");
            entity.CategoryName = dto.CategoryName.Trim();
            entity.RestaurantId = dto.RestaurantId;
            await _repo.UpdateAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<CategoryDto>(entity);
        }

        public async Task DeleteAsync(int categoryId, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(categoryId, ct) ?? throw new KeyNotFoundException("Category not found");
            await _repo.DeleteAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<CategoryDto?> GetByIdAsync(int categoryId, CancellationToken ct = default)
        {
            var e = await _repo.GetByIdAsync(categoryId, ct);
            return e is null ? null : _mapper.Map<CategoryDto>(e);
        }

        public IQueryable<Category> GetQueryableCategories(bool isSuperAdmin, int? restaurantId)
        {
            // Giả định rằng _repo.GetAll() trả về IQueryable<Category>
            // Đây là một pattern phổ biến trong Generic Repository.
            var query = _repo.GetQueryable()
                .Include(c => c.Restaurant)
                .AsNoTracking();

            // Áp dụng logic lọc theo quyền
            if (!isSuperAdmin && restaurantId.HasValue)
            {
                query = query.Where(c => c.RestaurantId == restaurantId.Value);
            }

            return query;
        }
    }
}
