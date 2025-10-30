using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Menulo.Pages.RoleSale.CreateRestaurant
{
    [Authorize(Roles = "sale")]
    public class CreateModel : PageModel
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IIdentityService _identityService; // Cần cho validation
        private readonly ICurrentUser _currentUser; // Cần để lấy SaleId
        private readonly IMapper _mapper;

        public CreateModel(
            IRestaurantService restaurantService,
            IIdentityService identityService,
            ICurrentUser currentUser,
            IMapper mapper)
        {
            _restaurantService = restaurantService;
            _identityService = identityService;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        [BindProperty]
        public RestaurantRequest.CreateWithAdmin Input { get; set; } = new();


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            // 1. Validation DataAnnotations (từ DTO)
            if (!ModelState.IsValid)
                return Page();

            // 2. Custom Validation (thay thế ValidateInputAsync)
            if (await _identityService.IsUsernameTakenAsync(Input.Username, ct))
            {
                ModelState.AddModelError("Input.Username", "Tên đăng nhập đã tồn tại.");
            }

            if (await _identityService.IsEmailTakenAsync(Input.Email, ct))
            {
                ModelState.AddModelError("Input.Email", "Email đã được sử dụng.");
            }

            if (await _identityService.IsPhoneTakenAsync(Input.Phone, ct))
            {
                ModelState.AddModelError("Input.Phone", "Số điện thoại đã được sử dụng.");
            }

            if (!ModelState.IsValid)
                return Page();

            // 3. Lấy Sale User ID (từ ICurrentUser thay vì UserManager)
            var saleUserId = _currentUser.UserId;
            if (string.IsNullOrEmpty(saleUserId))
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy thông tin người dùng sale.");
                return Page();
            }

            try
            {
                // 4. Map Request -> DTO (dùng AutoMapper)
                var dto = _mapper.Map<CreateRestaurantWithAdminDto>(Input);

                // 5. GỌI SERVICE (nơi chứa transaction)
                var newRestaurant = await _restaurantService.CreateRestaurantWithAdminAsync(dto, saleUserId, ct);

                TempData.SetSuccess($"Tạo nhà hàng '{newRestaurant.Name}' và tài khoản admin thành công!");
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                // 6. Bắt lỗi nghiệp vụ (ví dụ: tạo user thất bại)
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception)
            {
                // 7. Bắt lỗi hệ thống
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.");
                return Page();
            }
        }
    }
}
