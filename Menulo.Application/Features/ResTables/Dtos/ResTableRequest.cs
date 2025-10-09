using System.ComponentModel.DataAnnotations;

namespace Menulo.Application.Features.ResTables.Dtos
{
    public sealed class ResTableRequest
    {
        public sealed class Create
        {
            [Display(Name = "Tên nhà hàng")]
            public int RestaurantId { get; set; }

            [Display(Name = "Mã bàn")]
            public string? TableCode { get; set; }

            [Display(Name = "Thông tin bàn")]
            public string? Description { get; set; }
        }

        public sealed class Update
        {
            public int TableId { get; set; }

            [Display(Name = "Tên nhà hàng")]
            public int RestaurantId { get; set; }

            [Display(Name = "Mã bàn")]
            public string? TableCode { get; set; }

            [Display(Name = "Thông tin bàn")]
            public string? Description { get; set; }
        }
    }
}
