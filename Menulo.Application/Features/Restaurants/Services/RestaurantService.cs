using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Application.Features.Restaurants.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<Restaurant> _repo;
        private readonly IMapper _mapper;
        private readonly IImageStorageService _images;


        public RestaurantService(IUnitOfWork uow,
            IMapper mapper, IImageStorageService images)
        {
            _uow = uow;
            _repo = _uow.Repository<Restaurant>();
            _mapper = mapper;
            _images = images;
        }

        public async Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<Restaurant>(dto);
            await _repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<RestaurantDto>(entity);
        }

        public async Task<RestaurantDto> UpdateAsync(UpdateRestaurantDto dto, CancellationToken ct = default)
        {
            var entity = await _repo.GetQueryable()
                .FirstOrDefaultAsync(r => r.RestaurantId == dto.RestaurantId, ct)
                         ?? throw new KeyNotFoundException("Restaurant not found");

            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.Phone = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.LogoUrl))
            {
                entity.LogoUrl = dto.LogoUrl;
            }

            await _repo.UpdateAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<RestaurantDto>(entity);
        }

        public async Task<RestaurantDto> CreateWithLogoAsync(
            string name, string? address, string? phone,
            Stream logoStream, string logoFileName, string contentType,
            CancellationToken ct = default)
        {
            string? uploadedUrl = null;

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // 1) Tạo DB trước
                var created = await CreateAsync(new CreateRestaurantDto(name, address, phone, null), ct);

                // 2) Upload Drive
                uploadedUrl = await _images.UploadAsync(
                    logoStream, logoFileName, contentType,
                    restaurantId: created.RestaurantId,
                    restaurantName: created.Name,
                    logicalName: "logo");

                // 3) Cập nhật LogoUrl
                await UpdateAsync(new UpdateRestaurantDto(
                    created.RestaurantId, created.Name, created.Address, created.Phone, uploadedUrl
                ), ct);
                await _uow.SaveChangesAsync(ct);

                await _uow.CommitTransactionAsync(ct);

                // 4) Lấy lại entity rồi map -> RestaurantDto
                var entity = await _repo.GetByIdAsync(created.RestaurantId); // entity domain
                return _mapper.Map<RestaurantDto>(entity!);
            }
            catch
            {
                // bẫy: nếu đã upload xong mà DB rollback → xóa ảnh để tránh orphan
                if (!string.IsNullOrWhiteSpace(uploadedUrl))
                {
                    try { await _images.DeleteByPublicUrlAsync(uploadedUrl); } catch { /* log nếu cần */ }
                }
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }
        }

        public async Task<RestaurantDto> ReplaceLogoAsync(
            int restaurantId, string restaurantName,
            Stream newLogoStream, string newLogoFileName, string contentType,
            CancellationToken ct = default)
        {
            var current = await _repo.GetByIdAsync(restaurantId)
                 ?? throw new KeyNotFoundException("Restaurant not found.");

            // 1) Upload ảnh mới
            var newUrl = await _images.UploadAsync(
                newLogoStream, newLogoFileName, contentType,
                restaurantId, restaurantName, "logo");

            await _uow.BeginTransactionAsync(ct);
            try
            {
                // 2) Update DB trong TX
                await UpdateAsync(new UpdateRestaurantDto(
                    restaurantId, current.Name, current.Address, current.Phone, newUrl
                ), ct);

                await _uow.CommitTransactionAsync(ct);
            }
            catch
            {
                // Commit thất bại → xoá ảnh mới để không orphan
                try { await _images.DeleteByPublicUrlAsync(newUrl); } catch { /* log if needed */ }
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }

            // 3) Sau khi commit thành công, xoá ảnh cũ (best-effort)
            if (!string.IsNullOrWhiteSpace(current.LogoUrl))
            {
                try { await _images.DeleteByPublicUrlAsync(current.LogoUrl); } catch { /* log nếu cần */ }
            }

            var entity = await _repo.GetByIdAsync(restaurantId);
            return _mapper.Map<RestaurantDto>(entity!);
        }

        public async Task DeleteAsync(int restaurantId, CancellationToken ct = default)
        {
            var entity = await _repo.GetQueryable()
                        .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId, ct)
                         ?? throw new KeyNotFoundException("Restaurant not found");

            // Ktra du lieu da phat sinh chua
            var cateRepo = _uow.Repository<Category>().GetQueryable();
            var itemRepo = _uow.Repository<MenuItem>().GetQueryable();
            var orderRepo = _uow.Repository<Order>().GetQueryable();
            var tableRepo = _uow.Repository<RestaurantTable>().GetQueryable();
            var itemsTmpRepo = _uow.Repository<ItemsTmp>().GetQueryable();

            // ItemsTmp không có RestaurantId trực tiếp → kiểm qua bàn
            var hasTmp = await tableRepo
                    .Where(t => t.RestaurantId == restaurantId)
                    .AnyAsync(t => t.ItemsTmps.Any(), ct);

            var hasData =
                       await cateRepo.AnyAsync(x => x.RestaurantId == restaurantId, ct)
                    || await itemRepo.AnyAsync(x => x.RestaurantId == restaurantId, ct)
                    || await orderRepo.AnyAsync(x => x.RestaurantId == restaurantId, ct)
                    || await tableRepo.AnyAsync(x => x.RestaurantId == restaurantId, ct)
                    || hasTmp;

            if (hasData)
                throw new InvalidOperationException("Nhà hàng đã phát sinh dữ liệu nên không thể xoá.");

            // Cho xoá khi sạch dữ liệu
            await _repo.DeleteAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<RestaurantDetailsDto?> GetByIdAsync(int restaurantId, CancellationToken ct = default)
        {
            // Bước 1: Lấy toàn bộ entity 'Restaurant' từ database về.
            // Lúc này, LogoImage vẫn là một mảng byte (byte[]).
            var entity = await _repo.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId, ct);

            // Nếu không tìm thấy entity thì trả về null luôn.
            if (entity is null)
            {
                return null;
            }

            // Bước 2: Dùng _mapper.Map() để thực hiện việc mapping trong bộ nhớ.
            // Ở bước này, AutoMapper sẽ chạy code C# và thực hiện Convert.ToBase64String() một cách bình thường.
            var dto = _mapper.Map<RestaurantDetailsDto>(entity);

            return dto;
        }

        public IQueryable<Restaurant> GetQueryableRestaurantsForCurrentUser()
        {
            var query = _repo.GetQueryable();

            return query;
        }
    }
}
