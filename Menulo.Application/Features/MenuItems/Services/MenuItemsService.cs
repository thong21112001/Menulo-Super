using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.MenuItems.Dtos;
using Menulo.Application.Features.MenuItems.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Application.Features.MenuItems.Services
{
    public class MenuItemsService : IMenuItemsService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IImageStorageService _imageService;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<MenuItem> _menuItemRepo;

        public MenuItemsService(
            IUnitOfWork uow,
            IMapper mapper,
            IImageStorageService imageService)
        {
            _uow = uow;
            _mapper = mapper;
            _imageService = imageService;
            _categoryRepo = _uow.Repository<Category>();
            _menuItemRepo = _uow.Repository<MenuItem>();
        }


        public async Task<MenuItemDto> CreateMenuItemAsync(
            CreateMenuItemDto dto,
            Stream? imageStream,
            string? imageFileName,
            string? contentType,
            CancellationToken ct = default)
        {
            // 1. Lấy thông tin nhà hàng từ Category (để bảo mật)
            var category = await _categoryRepo.GetQueryable()
                .AsNoTracking()
                .Include(c => c.Restaurant) // Lấy tên nhà hàng
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId && c.RestaurantId == dto.RestaurantId, ct)
                ?? throw new KeyNotFoundException("Không tìm thấy danh mục hợp lệ.");

            string? imageUrl = null;

            // 2. Xử lý ảnh (logic cũ từ ProcessPhotoUploadAsync)
            if (imageStream != null && imageFileName != null && contentType != null)
            {
                string? restaurantName = category.Restaurant?.Name 
                    ?? throw new InvalidOperationException("Category does not have a valid Restaurant.");

                // Logic upload ảnh đã được "bá đạo" hóa trong IImageStorageService
                // (Giả định: ImageService của bạn xử lý resize, v.v.)
                imageUrl = await _imageService.UploadAsync(
                    imageStream,
                    imageFileName,
                    contentType,
                    category.RestaurantId,
                    restaurantName,
                    $"menu-item-{dto.ItemName.Replace(" ", "-").ToLower()}"
                );
            }
            // (Chúng ta bỏ qua logic ảnh mặc định từ wwwroot, 
            //  vì giờ đã dùng Google Drive. Việc hiển thị ảnh mặc định
            //  nên do Frontend xử lý nếu imageUrl là null).

            // 3. Map DTO -> Entity
            var entity = _mapper.Map<MenuItem>(dto);
            entity.ImageData = imageUrl;
            // (RestaurantId và CategoryId đã được map từ DTO)

            // 4. Lưu CSDL
            await _menuItemRepo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            // 5. Trả về DTO hoàn chỉnh
            var resultDto = _mapper.Map<MenuItemDto>(entity);
            // Gán CategoryName (vì entity mới tạo chưa có)
            resultDto = resultDto with { CategoryName = category.CategoryName };

            return resultDto;
        }
    }
}
