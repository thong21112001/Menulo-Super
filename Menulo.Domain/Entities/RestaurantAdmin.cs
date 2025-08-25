namespace Menulo.Domain.Entities
{
    public class RestaurantAdmin
    {
        public string UserId { get; set; } = default!;  // Dùng string để tương thích với Identity

        public int RestaurantId { get; set; }   // Thuộc tính RestaurantId chỉ dành cho tài khoản admin

        public Restaurant? Restaurant { get; set; }  // navigation 1 chiều là đủ
    }
}
