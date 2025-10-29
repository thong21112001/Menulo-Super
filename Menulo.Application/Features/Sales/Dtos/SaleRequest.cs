using System.ComponentModel.DataAnnotations;

namespace Menulo.Application.Features.Sales.Dtos
{
    public sealed class SaleRequest
    {
        public sealed class Create
        {
            [Display(Name = "Họ và tên")]
            [Required(ErrorMessage = "{0} không được bỏ trống")]
            public string FullName { get; set; } = null!;

            [Display(Name = "Tên tài khoản")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Tên tài khoản chỉ được chứa chữ, số và dấu gạch dưới.")]
            public string Username { get; set; } = null!;

            [Required(ErrorMessage = "{0} không được để trống")]
            [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; } = null!;

            [Display(Name = "Số điện thoại")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [StringLength(11, ErrorMessage = "Số điện thoại không được vượt quá {1} ký tự.")]
            public string PhoneNumber { get; set; } = null!;

            [Display(Name = "Mật khẩu")]
            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = null!;

            [Display(Name = "Xác nhận mật khẩu")]
            [Required(ErrorMessage = "Mật khẩu xác nhận không được để trống")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = null!;
        }

        //public sealed class Update
        //{
        //    public int RestaurantId { get; set; }

        //    [Display(Name = "Tên nhà hàng")]
        //    [Required(ErrorMessage = "{0} không được bỏ trống")]
        //    public string Name { get; set; } = null!;

        //    [Display(Name = "Địa chỉ")]
        //    public string? Address { get; set; }

        //    [Display(Name = "Số điện thoại")]
        //    public string? Phone { get; set; }

        //    [Display(Name = "Logo nhà hàng")]
        //    public string? LogoUrl { get; set; }
        //}
    }
}
