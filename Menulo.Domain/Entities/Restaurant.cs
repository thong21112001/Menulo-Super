namespace Menulo.Domain.Entities
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }

        public string Name { get; set; } = null!;

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public byte[]? LogoImage { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? CreatedBySaleId { get; set; }

        // ----------------- VietQR fields -----------------
        public string? BankCode { get; set; }

        public string? AccountNo { get; set; }

        public string? AccountName { get; set; }

        public string? DefaultMemoPrefix { get; set; } = "ORDER-";
        // -------------------------------------------------------

        public byte[]? StaticQrImageUrl { get; set; }   // mã qr thanh toán

        public bool EnableDynamicQr { get; set; } = false;  // Cờ để bật/tắt tính năng QR động (mặc định là tắt)

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<RestaurantTable> RestaurantTables { get; set; } = new List<RestaurantTable>();

        public ICollection<RestaurantAdmin> Admins { get; set; } = new List<RestaurantAdmin>();
    }
}
