using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        private readonly ICurrentUser _currentUser;


        public CategoryService(IUnitOfWork uow, IMapper mapper, ICurrentUser currentUser)
        {
            _uow = uow;
            _repo = _uow.Repository<Category>();
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
        {
            // Khóa tenant: non-superadmin chỉ được tạo cho chính nhà hàng của họ
            var restaurantId = _currentUser.IsSuperAdmin
                ? dto.RestaurantId // superadmin có thể chọn
                : _currentUser.RestaurantId ?? throw new UnauthorizedAccessException("Missing tenant");

            var entity = new Category
            {
                CategoryName = dto.CategoryName.Trim(),
                RestaurantId = restaurantId,
                Priority = dto.Priority
            };

            await _repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            // Requery bằng ProjectTo để có RestaurantName
            var result = await _repo.GetQueryable()
                .AsNoTracking()
                .Where(c => c.CategoryId == entity.CategoryId)
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .FirstAsync(ct);

            // Ẩn name nếu non-superadmin
            if (!_currentUser.IsSuperAdmin)
            {
                result = result with { RestaurantName = null };
            }

            return result;
        }

        public async Task<CategoryDto> UpdateAsync(UpdateCategoryDto dto, CancellationToken ct = default)
        {
            // Lấy theo tenant
            var q = _repo.GetQueryable();

            if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
                q = q.Where(c => c.RestaurantId == rid);

            var entity = await q.FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId, ct)
                         ?? throw new KeyNotFoundException("Category not found");

            entity.CategoryName = dto.CategoryName.Trim();
            entity.Priority = dto.Priority;

            // Khóa tenant: non-superadmin không được đổi RestaurantId
            if (_currentUser.IsSuperAdmin)
            {
                entity.RestaurantId = dto.RestaurantId;
            }

            await _repo.UpdateAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            var result = await _repo.GetQueryable()
                .AsNoTracking()
                .Where(c => c.CategoryId == entity.CategoryId)
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .FirstAsync(ct);

            if (!_currentUser.IsSuperAdmin)
            {
                result = result with { RestaurantName = null };
            }

            return result;
        }

        public async Task DeleteAsync(int categoryId, CancellationToken ct = default)
        {
            // Lấy theo tenant
            var q = _repo.GetQueryable();

            if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
                q = q.Where(c => c.RestaurantId == rid);

            var entity = await q.FirstOrDefaultAsync(c => c.CategoryId == categoryId, ct)
                         ?? throw new KeyNotFoundException("Category not found");

            await _repo.DeleteAsync(entity, ct);

            await _uow.SaveChangesAsync(ct);
        }

        public async Task<CategoryDto?> GetByIdAsync(int categoryId, CancellationToken ct = default)
        {
            var q = _repo.GetQueryable().AsNoTracking();

            if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
                q = q.Where(c => c.RestaurantId == rid);

            var dto = await q
                .Where(c => c.CategoryId == categoryId)
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);

            if (dto is null) return null;

            if (!_currentUser.IsSuperAdmin)
            {
                dto = dto with { RestaurantName = null };
            }

            return dto;
        }

        public IQueryable<Category> GetQueryableCategoriesForCurrentUser()
        {
            var query = _repo.GetQueryable()
                .Include(c => c.Restaurant)
                .AsNoTracking();

            // Áp dụng logic lọc theo quyền
            if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
                query = query.Where(c => c.RestaurantId == rid);

            return query;
        }
    }
}
