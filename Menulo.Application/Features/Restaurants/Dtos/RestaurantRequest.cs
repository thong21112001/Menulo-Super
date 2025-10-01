using System.ComponentModel.DataAnnotations;

namespace Menulo.Application.Features.Restaurants.Dtos
{
    public sealed class RestaurantRequest
    {
        public sealed class Create
        {
            [Display(Name = "Tên nhà hàng")]
            [Required(ErrorMessage = "{0} không được bỏ trống")]
            public string RestaurantName { get; set; } = null!;

            [Display(Name = "Địa chỉ")]
            public string? RestaurantAddress { get; set; }

            [Display(Name = "Số điện thoại")]
            public string? RestaurantPhone { get; set; }

            [Display(Name = "Logo nhà hàng")]
            public byte[]? RestaurantLogo { get; set; }

        }

        public sealed class Update
        {
            public int RestaurantId { get; set; }

            [Display(Name = "Tên nhà hàng")]
            [Required(ErrorMessage = "{0} không được bỏ trống")]
            public string RestaurantName { get; set; } = null!;

            [Display(Name = "Địa chỉ")]
            public string? RestaurantAddress { get; set; }

            [Display(Name = "Số điện thoại")]
            public string? RestaurantPhone { get; set; }

            [Display(Name = "Logo nhà hàng")]
            public byte[]? RestaurantLogo { get; set; }
        }
    }
}
