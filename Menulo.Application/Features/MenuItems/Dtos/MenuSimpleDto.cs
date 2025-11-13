namespace Menulo.Application.Features.MenuItems.Dtos
{
    /// <summary>
    /// Dùng để hiển thị trên dropdown danh sách món ăn
    /// </summary>
    public sealed class MenuSimpleDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
