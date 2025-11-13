using AutoMapper;
using AutoMapper.QueryableExtensions;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.MenuItems.Dtos;
using Menulo.Application.Features.MenuItems.Interfaces;
using Menulo.Application.Features.Restaurants.Dtos;
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
        private readonly ICurrentUser _currentUser;

        public MenuItemsService(
            IUnitOfWork uow,
            IMapper mapper,
            IImageStorageService imageService,
            ICurrentUser currentUser)
        {
            _uow = uow;
            _mapper = mapper;
            _imageService = imageService;
            _categoryRepo = _uow.Repository<Category>();
            _menuItemRepo = _uow.Repository<MenuItem>();
            _currentUser = currentUser;
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

        public IQueryable<MenuItemRowDto> GetQueryableMenuItemsForCurrentUser()
        {
            var query = _menuItemRepo.GetQueryable()
                .AsNoTracking()
                .Where(mi => mi.IsDeleted == false); //Logic lọc món đã xóa

            if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
            {
                query = query.Where(mi => mi.RestaurantId == rid);
            }

            var dtoQuery = query.ProjectTo<MenuItemRowDto>(_mapper.ConfigurationProvider);

            // Ẩn RestaurantName nếu không phải SuperAdmin (bảo mật JSON)
            if (!_currentUser.IsSuperAdmin)
            {
                dtoQuery = dtoQuery.Select(dto => new MenuItemRowDto
                {
                    ItemId = dto.ItemId,
                    ItemName = dto.ItemName,
                    Description = dto.Description,
                    Price = dto.Price,
                    IsAvailable = dto.IsAvailable,
                    CategoryId = dto.CategoryId,
                    CategoryName = dto.CategoryName,
                    RestaurantId = dto.RestaurantId,
                    RestaurantName = null
                });
            }

            return dtoQuery;
        }

        public async Task<bool> ToggleMenuItemAvailabilityAsync(int menuItemId, CancellationToken ct = default)
        {
            var query = _menuItemRepo.GetQueryable();

            if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
            {
                query = query.Where(mi => mi.RestaurantId == rid);
            }

            var menuItem = await query.FirstOrDefaultAsync(mi => mi.ItemId == menuItemId, ct)
                           ?? throw new KeyNotFoundException("Không tìm thấy món ăn.");

            menuItem.IsAvailable = !menuItem.IsAvailable;

            await _uow.SaveChangesAsync(ct);

            return menuItem.IsAvailable;
        }

        public async Task<List<MenuCategoryGroupDto>> GetMenuForCurrentUserAsync(CancellationToken ct = default)
        {
            // 1. Lấy Tenant ID (Bảo mật)
            var restaurantId = _currentUser.RestaurantId
                ?? throw new UnauthorizedAccessException("Người dùng không được gán cho nhà hàng nào.");

            // 2. Xây dựng truy vấn (Query)
            var itemsQuery = _menuItemRepo.GetQueryable()
                .AsNoTracking()
                .Where(m =>
                    m.RestaurantId == restaurantId && // Lọc theo tenant
                    m.IsDeleted == false &&          // Chỉ món đang bán
                    m.IsAvailable == true)           // Chỉ món đang có
                .Include(m => m.Category) // Cần Include để GroupBy và OrderBy
                // 3. Sắp xếp trong CSDL (hiệu năng cao)
                .OrderBy(m => m.Category!.Priority)
                .ThenBy(m => m.Category!.CategoryName)
                .ThenBy(m => m.ItemId);

            // 4. Tải dữ liệu (chỉ của 1 nhà hàng, đã lọc)
            var allItems = await itemsQuery.ToListAsync(ct);

            // 5. Nhóm (Group) và Map (ánh xạ) trong bộ nhớ
            var groupedMenu = allItems
                .GroupBy(item => item.CategoryId)
                .Select(group => {
                    var categoryInfo = group.First().Category;

                    return new MenuCategoryGroupDto(
                        CategoryId: categoryInfo!.CategoryId,
                        CategoryName: categoryInfo.CategoryName,
                        CategoryPriority: categoryInfo.Priority,
                        Items: group.Select(item =>
                        {
                            string? imageProxyUrl = string.IsNullOrWhiteSpace(item.ImageData)
                            ? null
                            : $"/api/images/menuitems/{item.ItemId}?w=400&h=300";

                            return new MenuItemCardDto(
                                item.ItemId,
                                item.ItemName,
                                item.Price,
                                imageProxyUrl
                            );
                        }).ToList()
                    );
                })
                .OrderBy(g => g.CategoryPriority)
                .ThenBy(g => g.CategoryName)
                .ToList();

            return groupedMenu;
        }

        public async Task<MenuItemDetailsDto?> GetByIdAsync(int menuItemId, CancellationToken ct = default)
        {
            var entity = await _menuItemRepo.GetQueryable()
                .AsNoTracking()
                .Include(m => m.Category)
                .FirstOrDefaultAsync(r => r.ItemId == menuItemId, ct);

            if (entity is null)
            {
                return null;
            }

            var dto = _mapper.Map<MenuItemDetailsDto>(entity);

            return dto;
        }

        public async Task<IReadOnlyList<MenuSimpleDto>> GetSimpleAsync(int restaurantId, CancellationToken ct = default)
        {
            return await _uow.Repository<MenuItem>().GetQueryable()
                .AsNoTracking()
                .Where(mi => mi.RestaurantId == restaurantId && !mi.IsDeleted && mi.IsAvailable)
                .OrderBy(mi => mi.ItemName)
                .Select(mi => new MenuSimpleDto
                {
                    ItemId = mi.ItemId,
                    ItemName = mi.ItemName,
                    Price = mi.Price
                })
                .ToListAsync(ct);
        }
    }
}
