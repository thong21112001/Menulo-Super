using System.ComponentModel.DataAnnotations;

namespace Menulo.Application.Features.Restaurants.Dtos
{
    public sealed class RestaurantRequest
    {
        public sealed class Create
        {
            [Display(Name = "Tên nhà hàng")]
            [Required(ErrorMessage = "{0} không được bỏ trống")]
            public string Name { get; set; } = null!;

            [Display(Name = "Địa chỉ")]
            public string? Address { get; set; }

            [Display(Name = "Số điện thoại")]
            public string? Phone { get; set; }

            [Display(Name = "Logo nhà hàng")]
            public string? LogoUrl { get; set; }

        }

        public sealed class Update
        {
            public int RestaurantId { get; set; }

            [Display(Name = "Tên nhà hàng")]
            [Required(ErrorMessage = "{0} không được bỏ trống")]
            public string Name { get; set; } = null!;

            [Display(Name = "Địa chỉ")]
            public string? Address { get; set; }

            [Display(Name = "Số điện thoại")]
            public string? Phone { get; set; }

            [Display(Name = "Logo nhà hàng")]
            public string? LogoUrl { get; set; }
        }

        // ===== Dùng để sale tạo thông tin và tài khoản cho nhà hàng =====
        public sealed class CreateWithAdmin
        {
            // --- Thông tin nhà hàng ---
            [Display(Name = "Tên nhà hàng")]
            [Required(ErrorMessage = "{0} không được để trống")]
            public string Name { get; set; } = string.Empty;

            [Display(Name = "Địa chỉ")]
            [Required(ErrorMessage = "{0} không được để trống")]
            public string Address { get; set; } = string.Empty;

            [Display(Name = "Số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Required(ErrorMessage = "{0} không được để trống")]
            public string Phone { get; set; } = string.Empty;

            // --- Thông tin tài khoản admin ---
            [Display(Name = "Tên chủ nhà hàng")]
            [Required(ErrorMessage = "{0} không được để trống")]
            public string FullName { get; set; } = string.Empty;

            [Display(Name = "Tài khoản đăng nhập")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Tài khoản chỉ chứa chữ, số, '.' và '_'.")]
            public string Username { get; set; } = string.Empty;

            [Display(Name = "Email chủ nhà hàng")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            [Display(Name = "Mật khẩu")]
            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Xác nhận mật khẩu")]
            [Required(ErrorMessage = "Mật khẩu xác nhận không được để trống")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
