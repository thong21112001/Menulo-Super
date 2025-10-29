using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Sales.Dtos;
using Menulo.Application.Features.Sales.Interfaces;
using Menulo.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Menulo.Pages.Superadmin.Sales
{
    public class CreateModel : PageModel
    {
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;
        private readonly IIdentityService _identityService;

        [BindProperty]
        public SaleRequest.Create Input { get; set; } = new();

        public CreateModel(ISaleService saleService, IMapper mapper, IIdentityService identityService)
        {
            _saleService = saleService;
            _mapper = mapper;
            _identityService = identityService;
        }


        public void OnGet()
        {
        }


        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var dto = _mapper.Map<CreateSaleDto>(Input);

            // Kiểm tra Username
            if (await _identityService.IsUsernameTakenAsync(dto.Username, ct))
            {
                // Thêm lỗi vào ModelState, gán cho đúng field
                ModelState.AddModelError("Input.Username", "Tên tài khoản này đã tồn tại.");
            }

            // Kiểm tra Email
            if (await _identityService.IsEmailTakenAsync(dto.Email, ct))
            {
                ModelState.AddModelError("Input.Email", "Email này đã được sử dụng.");
            }

            // Kiểm tra Số điện thoại
            if (await _identityService.IsPhoneTakenAsync(dto.PhoneNumber, ct))
            {
                ModelState.AddModelError("Input.PhoneNumber", "Số điện thoại này đã được sử dụng.");
            }

            // Kiểm tra lại ModelState SAU KHI check thủ công
            if (!ModelState.IsValid)
            {
                TempData.SetError("Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.");
                return Page(); // Trả về Page() để hiển thị các lỗi đã AddModelError
            }

            try
            {
                // Gửi DTO "sạch" này vào Service
                var newSaleAccount = await _saleService.CreateAsync(dto, ct);

                TempData.SetSuccess($"Đã tạo tài khoản sale '{newSaleAccount.Username}' thành công.");
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex) // Bắt lỗi từ SaleService
            {
                TempData.SetError(ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống chung
                TempData.SetError("Đã xảy ra lỗi không mong muốn: " + ex.Message);
                return Page();
            }
        }
    }
}
